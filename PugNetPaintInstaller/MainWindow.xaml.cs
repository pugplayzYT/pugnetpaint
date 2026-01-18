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
            try
            {
                var fullPath = Path.GetFullPath(InstallPathTxt.Text);
                if (string.IsNullOrWhiteSpace(fullPath))
                    throw new ArgumentException("Path is empty");
            }
            catch
            {
                MessageBox.Show("Please enter a valid installation path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _installPath = InstallPathTxt.Text;
            _step++;
            PageLocation.Visibility = Visibility.Collapsed;
            PageInstalling.Visibility = Visibility.Visible;
            NextBtn.IsEnabled = false;
            CancelBtn.IsEnabled = false;

            bool success = await Task.Run(() => InstallApp());

            if (success)
            {
                _step++;
                PageInstalling.Visibility = Visibility.Collapsed;
                PageFinish.Visibility = Visibility.Visible;
                NextBtn.Content = "Finish";
                NextBtn.IsEnabled = true;
                CancelBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Revert to allow retry or exit
                _step--;
                PageInstalling.Visibility = Visibility.Collapsed;
                PageLocation.Visibility = Visibility.Visible;
                NextBtn.IsEnabled = true;
                CancelBtn.IsEnabled = true;
                MessageBox.Show("Installation failed. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (_step == 3) // Finish
        {
            if (LaunchChk.IsChecked == true)
            {
                var exePath = Path.Combine(_installPath, "PugNetPaint.exe");
                if (File.Exists(exePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                }
            }
            Application.Current.Shutdown();
        }
    }

    private bool InstallApp()
    {
        string tempZip = null;
        try
        {
            Dispatcher.Invoke(() => StatusTxt.Text = "Checking for existing instances...");

            // Kill existing process if running to allow update
            var existingProcs = System.Diagnostics.Process.GetProcessesByName("PugNetPaint");
            foreach (var p in existingProcs)
            {
                try { p.Kill(); p.WaitForExit(1000); } catch { }
                p.Dispose();
            }

            Dispatcher.Invoke(() => StatusTxt.Text = "Creating directories...");

            // Validate absolute path
            if (!Path.IsPathRooted(_installPath))
            {
                throw new ArgumentException("Path must be absolute.");
            }

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

                // Secure temp file creation
                tempZip = Path.Combine(Path.GetTempPath(), $"pugnetpaint_{Path.GetRandomFileName()}.zip");

                using (var fileStream = new FileStream(tempZip, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }

                Dispatcher.Invoke(() => InstallProgress.Value = 50);

                // Extract with overwrite (true)
                ZipFile.ExtractToDirectory(tempZip, _installPath, true);
            }

            Dispatcher.Invoke(() =>
            {
                InstallProgress.Value = 80;
                StatusTxt.Text = "Creating Shortcuts...";
            });

            // Sanitize path for PowerShell to prevent injection
            // Escape single quotes by doubling them
            string safeInstallPath = _installPath.Replace("'", "''");
            string safeExePath = Path.Combine(safeInstallPath, "PugNetPaint.exe");

            // Create Shortcut via PowerShell
            // FIXED: Using PowerShell's Join-Path for variables instead of C# Path.Combine on string literals
            string psScript = $@"
                $WshShell = New-Object -comObject WScript.Shell
                
                # Desktop Shortcut
                $Shortcut = $WshShell.CreateShortcut('$HOME\Desktop\PugNetPaint.lnk')
                $Shortcut.TargetPath = '{safeExePath}'
                $Shortcut.IconLocation = '{safeExePath},0'
                $Shortcut.Save()

                # Start Menu Shortcut
                $StartMenuPath = [Environment]::GetFolderPath('StartMenu')
                $ProgramPath = Join-Path $StartMenuPath 'Programs'
                $LnkPath = Join-Path $ProgramPath 'PugNetPaint.lnk'
                $ShortcutSM = $WshShell.CreateShortcut($LnkPath)
                $ShortcutSM.TargetPath = '{safeExePath}'
                $ShortcutSM.IconLocation = '{safeExePath},0'
                $ShortcutSM.Save()
            ";

            var psInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var psProc = System.Diagnostics.Process.Start(psInfo))
            {
                psProc?.WaitForExit();
            }

            Dispatcher.Invoke(() => InstallProgress.Value = 100);
            return true;
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error: " + ex.Message));
            return false;
        }
        finally
        {
            // Pickup trash
            if (tempZip != null && File.Exists(tempZip))
            {
                try { File.Delete(tempZip); } catch { }
            }
        }
    }
}