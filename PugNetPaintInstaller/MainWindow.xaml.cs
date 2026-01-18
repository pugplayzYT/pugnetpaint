using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PugNetPaintInstaller;

public partial class MainWindow : Window
{
    private int _step = 0;
    private string _installPath;

    public MainWindow()
    {
        InitializeComponent();
        _installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PugNetPaint");
        InstallPathTxt.Text = _installPath;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_step == 0) // Welcome
        {
            _step++;
            PageWelcome.Visibility = Visibility.Collapsed;
            PageLocation.Visibility = Visibility.Visible;
        }
        else if (_step == 1) // Location
        {
            _installPath = InstallPathTxt.Text;
            _step++;
            PageLocation.Visibility = Visibility.Collapsed;
            PageInstalling.Visibility = Visibility.Visible;
            NextBtn.IsEnabled = false;
            CancelBtn.IsEnabled = false;

            await Task.Run(() => InstallApp());

            _step++;
            PageInstalling.Visibility = Visibility.Collapsed;
            PageFinish.Visibility = Visibility.Visible;
            NextBtn.Content = "Finish";
            NextBtn.IsEnabled = true;
            CancelBtn.Visibility = Visibility.Collapsed;
        }
        else if (_step == 3) // Finish
        {
            if (LaunchChk.IsChecked == true)
            {
                var exePath = Path.Combine(_installPath, "PugNetPaint.exe");
                if (File.Exists(exePath))
                {
                    System.Diagnostics.Process.Start(exePath);
                }
            }
            Close();
        }
    }

    private void InstallApp()
    {
        try
        {
            Dispatcher.Invoke(() => StatusTxt.Text = "Creating directories...");
            if (!Directory.Exists(_installPath))
                Directory.CreateDirectory(_installPath);

            Dispatcher.Invoke(() =>
            {
                InstallProgress.Value = 20;
                StatusTxt.Text = "Extracting files...";
            });

            // Extract Embedded Resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PugNetPaintInstaller.PugNetPaint.zip";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new Exception("Resource not found: " + resourceName);

                // Temp zip file
                string tempZip = Path.Combine(Path.GetTempPath(), "pugnetpaint_setup.zip");
                using (var fileStream = File.Create(tempZip))
                {
                    stream.CopyTo(fileStream);
                }

                Dispatcher.Invoke(() => InstallProgress.Value = 50);

                // Extract
                if (Directory.Exists(_installPath))
                {
                    // Clean install
                    /* Directory.Delete(_installPath, true); */
                    // Can't simple delete if running, whatever
                }

                ZipFile.ExtractToDirectory(tempZip, _installPath, true);
                File.Delete(tempZip);
            }

            Dispatcher.Invoke(() =>
            {
                InstallProgress.Value = 80;
                StatusTxt.Text = "Creating Shortcut...";
            });

            // Create Shortcut via PowerShell because COM is pain
            string psScript = $@"
                $WshShell = New-Object -comObject WScript.Shell
                $Shortcut = $WshShell.CreateShortcut('$HOME\Desktop\PugNetPaint.lnk')
                $Shortcut.TargetPath = '{Path.Combine(_installPath, "PugNetPaint.exe")}'
                $Shortcut.Save()
            ";

            var psInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(psInfo)?.WaitForExit();

            Dispatcher.Invoke(() => InstallProgress.Value = 100);
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error: " + ex.Message));
        }
    }
}