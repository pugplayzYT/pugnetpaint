using System.Windows;

namespace PugNetPaint;

/// <summary>
/// This is the code-behind for the Settings Window.
/// 
/// Code-behind = the C# logic that makes the XAML (UI) actually DO stuff.
/// Think of XAML as the "looks" and this file as the "brains" of the settings window.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Constructor - this runs when the settings window opens.
    /// It sets up the initial state of all the toggles based on saved settings.
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent();

        // Load the current settings and update the UI to match
        LoadSettingsToUI();
    }

    /// <summary>
    /// Loads the saved settings and updates all the toggle switches to match.
    /// This makes sure when you open settings, the toggles show the CORRECT state!
    /// </summary>
    private void LoadSettingsToUI()
    {
        // Get the singleton settings instance (the one shared settings object)
        var settings = AppSettings.Instance;

        // Set the Beta Mode toggle to match the saved setting
        BetaModeToggle.IsChecked = settings.BetaModeEnabled;

        // Set the History Export toggle to match the saved setting
        HistoryExportToggle.IsChecked = settings.ExperimentalHistoryExportEnabled;

        // Show/hide the experimental section based on beta mode
        UpdateExperimentalSectionVisibility();
    }

    /// <summary>
    /// Shows or hides the Experimental Features section.
    /// This section is ONLY visible when Beta Mode is enabled!
    /// 
    /// It's like a secret room that only opens when you have the special key (beta mode).
    /// </summary>
    private void UpdateExperimentalSectionVisibility()
    {
        // If beta mode is ON, show the experimental section
        // If beta mode is OFF, hide it (Collapsed = hidden and takes no space)
        ExperimentalSection.Visibility = BetaModeToggle.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    /// <summary>
    /// Called when the Beta Mode toggle is clicked.
    /// Saves the new setting and updates the UI accordingly.
    /// </summary>
    private void BetaModeToggle_Click(object sender, RoutedEventArgs e)
    {
        // Update the setting
        AppSettings.Instance.BetaModeEnabled = BetaModeToggle.IsChecked == true;

        // If beta mode is turned OFF, also disable the experimental export feature
        // (can't use experimental features without beta mode!)
        if (!AppSettings.Instance.BetaModeEnabled)
        {
            AppSettings.Instance.ExperimentalHistoryExportEnabled = false;
            HistoryExportToggle.IsChecked = false;
        }

        // Save the changes to disk
        AppSettings.Instance.Save();

        // Update the visibility of the experimental section
        UpdateExperimentalSectionVisibility();
    }

    /// <summary>
    /// Called when the Experimental History Export toggle is clicked.
    /// Saves the new setting.
    /// </summary>
    private void HistoryExportToggle_Click(object sender, RoutedEventArgs e)
    {
        // Update the setting
        AppSettings.Instance.ExperimentalHistoryExportEnabled = HistoryExportToggle.IsChecked == true;

        // Save the changes to disk
        AppSettings.Instance.Save();
    }

    /// <summary>
    /// Called when the Close button is clicked.
    /// Simply closes the settings window.
    /// </summary>
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        // Close this window (settings are already saved on each toggle click!)
        Close();
    }
}
