using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PugNetPaint;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


        // Default settings
        if (MyCanvas != null)
        {
            MyCanvas.DefaultDrawingAttributes.Color = Colors.Blue;
            MyCanvas.DefaultDrawingAttributes.Width = 2;
            MyCanvas.DefaultDrawingAttributes.Height = 2;
        }
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string colorName)
        {
            Color color = Colors.Black;
            switch (colorName)
            {
                case "Blue": color = Colors.Blue; break;
                case "Green": color = Colors.Green; break;
                case "Red": color = Colors.Red; break;
                case "Orange": color = Colors.Orange; break;
            }
            MyCanvas.DefaultDrawingAttributes.Color = color;
            MyCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }
    }

    private void Size_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MyCanvas != null)
        {
            MyCanvas.DefaultDrawingAttributes.Width = e.NewValue;
            MyCanvas.DefaultDrawingAttributes.Height = e.NewValue;
        }
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (MyCanvas.Strokes.Count > 0)
        {
            MyCanvas.Strokes.RemoveAt(MyCanvas.Strokes.Count - 1);
        }
    }

    private void Eraser_Click(object sender, RoutedEventArgs e)
    {
        MyCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to clear the canvas? This cannot be undone.", "Clear Canvas", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            MyCanvas.Strokes.Clear();
            MyCanvas.Background = Brushes.Transparent;
        }
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.ContextMenu != null)
        {
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.IsOpen = true;
        }
    }

    private void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        SaveWithWarning("isf");
    }

    private void ExportPng_Click(object sender, RoutedEventArgs e)
    {
        SaveWithWarning("png");
    }

    private void ExportJpg_Click(object sender, RoutedEventArgs e)
    {
        SaveWithWarning("jpg");
    }

    private void SaveWithWarning(string format)
    {
        if (format != "isf")
        {
            string msg = "WARNING: Exporting as an image (" + format.ToUpper() + ") will flatten everything.\n" +
                         "You will NOT be able to undo strokes or move them if you open this file later.\n\n" +
                         "Use 'Save Project (.isf)' if you want to keep your layers editable.\n\n" +
                         "Continue?";

            if (MessageBox.Show(msg, "Export Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (format == "isf") saveFileDialog.Filter = "Ink Serialized Format (*.isf)|*.isf";
        else if (format == "png") saveFileDialog.Filter = "PNG Image (*.png)|*.png";
        else if (format == "jpg") saveFileDialog.Filter = "JPEG Image (*.jpg)|*.jpg";

        if (saveFileDialog.ShowDialog() == true)
        {
            using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
            {
                if (format == "isf")
                {
                    MyCanvas.Strokes.Save(fs);
                }
                else
                {
                    // Render the canvas to a bitmap
                    Rect bounds = VisualTreeHelper.GetDescendantBounds(MyCanvas);
                    double dpi = 96d;
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);

                    DrawingVisual dv = new DrawingVisual();
                    using (DrawingContext dc = dv.RenderOpen())
                    {
                        VisualBrush vb = new VisualBrush(MyCanvas);
                        dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                    }
                    rtb.Render(dv);

                    BitmapEncoder encoder = null;
                    if (format == "png") encoder = new PngBitmapEncoder();
                    else encoder = new JpegBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(fs);
                }
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Ink Serialized Format (*.isf)|*.isf";
        if (saveFileDialog.ShowDialog() == true)
        {
            using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
            {
                MyCanvas.Strokes.Save(fs);
            }
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Ink Serialized Format (*.isf)|*.isf";

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                string file = openFileDialog.FileName;
                if (file.EndsWith(".isf"))
                {
                    // ISF format preserves strokes (editable)
                    using (FileStream fs = new FileStream(file, FileMode.Open))
                    {
                        StrokeCollection strokes = new StrokeCollection(fs);
                        MyCanvas.Strokes = strokes;
                    }
                }
                else
                {
                    // Standard image formats
                    ImageBrush img = new ImageBrush();
                    img.ImageSource = new BitmapImage(new Uri(file, UriKind.Absolute));
                    img.Stretch = Stretch.Uniform;
                    MyCanvas.Background = img;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening file: " + ex.Message);
            }
        }
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(MyCanvas, "PugNetPaint Drawing");
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            Undo_Click(sender, new RoutedEventArgs());
        }
    }
}