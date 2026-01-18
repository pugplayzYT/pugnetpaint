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
        openFileDialog.Filter = "Ink Serialized Format (*.isf)|*.isf";
        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    StrokeCollection strokes = new StrokeCollection(fs);
                    MyCanvas.Strokes = strokes;
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
            Undo_Click(null, null);
        }
    }
}