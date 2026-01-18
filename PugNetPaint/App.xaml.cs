using System.Configuration;
using System.Data;
using System.Windows;

namespace PugNetPaint;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("Crash: " + e.Exception.ToString());
        e.Handled = true;
    }
}

