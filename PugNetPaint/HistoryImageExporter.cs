using System;
using System.IO;
using System.Text;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace PugNetPaint;

/// <summary>
/// THE GROUNDBREAKING TECHNOLOGY CLASS! 🚀
/// 
/// This class handles the experimental PNG/JPEG export with embedded history.
/// 
/// HOW IT WORKS (for your smol brain):
/// 1. When you export an image, we take your strokes (the ISF data)
/// 2. Convert them to Base64 (a text format that can hold binary data)
/// 3. STUFF that Base64 string into the image's METADATA
/// 4. When you open the image later, we READ that metadata
/// 5. Convert the Base64 BACK to strokes
/// 6. BOOM - your drawing history is restored!
/// 
/// It's literally black magic but with code. You're welcome.
/// </summary>
public static class HistoryImageExporter
{
    // ============================================================
    // CONSTANTS
    // ============================================================

    /// <summary>
    /// This is the "key" we use to store our stroke data in the image metadata.
    /// It's like labeling a box "PugNetPaint Strokes" so we can find it later.
    /// 
    /// We use a custom metadata path in the PNG/JPEG structure.
    /// "/iTXt/Keyword" is a standard PNG text chunk.
    /// "/xmp/PugNetPaintStrokes" is XMP metadata for JPEG.
    /// </summary>
    private const string METADATA_KEY = "PugNetPaintStrokes";

    /// <summary>
    /// Magic header to identify our embedded data.
    /// If metadata starts with this, we know it's PugNetPaint stroke data!
    /// </summary>
    private const string MAGIC_HEADER = "PNPS1:"; // PugNetPaint Strokes version 1

    // ============================================================
    // EXPORT METHODS
    // ============================================================

    /// <summary>
    /// Exports the canvas as a PNG with embedded stroke history.
    /// The strokes are stored in a custom PNG text chunk!
    /// </summary>
    /// <param name="canvas">The InkCanvas to export</param>
    /// <param name="filePath">Where to save the file</param>
    /// <param name="strokes">The stroke collection to embed</param>
    public static void ExportPngWithHistory(Visual canvas, string filePath, StrokeCollection strokes)
    {
        // First, render the canvas to a bitmap
        Rect bounds = VisualTreeHelper.GetDescendantBounds(canvas);
        if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new InvalidOperationException("Canvas is empty! Draw something first ya goofball!");
        }

        double dpi = 96d;
        RenderTargetBitmap rtb = new((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);

        DrawingVisual dv = new();
        using (DrawingContext dc = dv.RenderOpen())
        {
            VisualBrush vb = new(canvas);
            dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        }
        rtb.Render(dv);

        // Convert strokes to Base64
        string strokeData = ConvertStrokesToBase64(strokes);

        // Create PNG with metadata
        PngBitmapEncoder encoder = new();
        BitmapFrame frame = BitmapFrame.Create(rtb);

        // Create a new frame with metadata
        BitmapMetadata metadata = new("png");
        try
        {
            // PNG uses iTXt chunks for text data
            // We'll store our data as a comment with our magic header
            metadata.Comment = MAGIC_HEADER + strokeData;
        }
        catch
        {
            // Some PNG metadata operations can fail, fallback to basic save
        }

        // Create frame with metadata
        BitmapFrame frameWithMetadata = BitmapFrame.Create(
            rtb,
            null,
            metadata,
            null
        );

        encoder.Frames.Add(frameWithMetadata);

        // Save to file
        using FileStream fs = new(filePath, FileMode.Create);
        encoder.Save(fs);
    }

    /// <summary>
    /// Exports the canvas as a JPEG with embedded stroke history.
    /// JPEG uses EXIF/XMP metadata to store the data!
    /// </summary>
    public static void ExportJpegWithHistory(Visual canvas, string filePath, StrokeCollection strokes)
    {
        // Render the canvas to a bitmap
        Rect bounds = VisualTreeHelper.GetDescendantBounds(canvas);
        if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new InvalidOperationException("Canvas is empty! Draw something first ya goofball!");
        }

        double dpi = 96d;
        RenderTargetBitmap rtb = new((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);

        DrawingVisual dv = new();
        using (DrawingContext dc = dv.RenderOpen())
        {
            VisualBrush vb = new(canvas);
            dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
        }
        rtb.Render(dv);

        // Convert strokes to Base64
        string strokeData = ConvertStrokesToBase64(strokes);

        // Create JPEG with metadata
        JpegBitmapEncoder encoder = new();
        encoder.QualityLevel = 95; // High quality!

        // Try to create metadata (JPEG is trickier with metadata)
        BitmapMetadata metadata = new("jpg");
        try
        {
            // Store in the Comment field (most compatible)
            metadata.Comment = MAGIC_HEADER + strokeData;
        }
        catch
        {
            // Metadata might fail, continue anyway
        }

        // Create frame with metadata
        BitmapFrame frameWithMetadata = BitmapFrame.Create(
            rtb,
            null,
            metadata,
            null
        );

        encoder.Frames.Add(frameWithMetadata);

        // Save to file
        using FileStream fs = new(filePath, FileMode.Create);
        encoder.Save(fs);
    }

    // ============================================================
    // IMPORT METHODS
    // ============================================================

    /// <summary>
    /// Attempts to load stroke history from an image file.
    /// If the image was exported with history, it returns the strokes!
    /// If not, it returns null.
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>StrokeCollection if found, null if not</returns>
    public static StrokeCollection? LoadStrokesFromImage(string filePath)
    {
        try
        {
            // Open the image and read its metadata
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            BitmapDecoder decoder;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".png")
            {
                decoder = new PngBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }
            else if (ext == ".jpg" || ext == ".jpeg")
            {
                decoder = new JpegBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }
            else
            {
                return null; // Unsupported format
            }

            if (decoder.Frames.Count == 0) return null;

            BitmapFrame frame = decoder.Frames[0];
            BitmapMetadata? metadata = frame.Metadata as BitmapMetadata;

            if (metadata == null) return null;

            // Try to read the comment field
            string? comment = null;
            try
            {
                comment = metadata.Comment;
            }
            catch
            {
                // Metadata access can fail
            }

            if (string.IsNullOrEmpty(comment)) return null;

            // Check for our magic header
            if (!comment.StartsWith(MAGIC_HEADER)) return null;

            // Extract the Base64 data (everything after the header)
            string base64Data = comment.Substring(MAGIC_HEADER.Length);

            // Convert back to strokes
            return ConvertBase64ToStrokes(base64Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load strokes from image: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if an image file contains embedded stroke history.
    /// Quick check without fully loading the strokes.
    /// </summary>
    public static bool HasEmbeddedHistory(string filePath)
    {
        try
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            BitmapDecoder decoder;

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".png")
            {
                decoder = new PngBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }
            else if (ext == ".jpg" || ext == ".jpeg")
            {
                decoder = new JpegBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }
            else
            {
                return false;
            }

            if (decoder.Frames.Count == 0) return false;

            BitmapFrame frame = decoder.Frames[0];
            BitmapMetadata? metadata = frame.Metadata as BitmapMetadata;

            if (metadata == null) return false;

            string? comment = null;
            try
            {
                comment = metadata.Comment;
            }
            catch
            {
                return false;
            }

            return !string.IsNullOrEmpty(comment) && comment.StartsWith(MAGIC_HEADER);
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // CONVERSION HELPERS
    // ============================================================

    /// <summary>
    /// Converts a StrokeCollection to a Base64 string.
    /// 
    /// What's happening here:
    /// 1. StrokeCollection.Save() gives us binary ISF data
    /// 2. We convert that binary to Base64 (text that represents binary)
    /// 3. Base64 can be safely stored in text fields like metadata!
    /// </summary>
    private static string ConvertStrokesToBase64(StrokeCollection strokes)
    {
        using MemoryStream ms = new();
        strokes.Save(ms);
        byte[] bytes = ms.ToArray();
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Converts a Base64 string back to a StrokeCollection.
    /// 
    /// The reverse of the above:
    /// 1. Decode Base64 to get binary data
    /// 2. Load that binary as ISF data
    /// 3. Create a new StrokeCollection from it!
    /// </summary>
    private static StrokeCollection ConvertBase64ToStrokes(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        using MemoryStream ms = new(bytes);
        return new StrokeCollection(ms);
    }
}
