using System;
using System.IO;
using System.Text.Json;

namespace PugNetPaint;

/// <summary>
/// This class stores all your settings and saves/loads them to a JSON file.
/// Think of it as the app's BRAIN that remembers your preferences!
/// 
/// It uses a singleton pattern (fancy word for: there's only ONE instance of this class
/// that everyone shares, like a communal fridge but for settings).
/// </summary>
public class AppSettings
{
    // ============================================================
    // SINGLETON INSTANCE
    // ============================================================
    // This is THE ONE settings object. Everyone uses this same one.
    // It's like having one TV remote for the whole house.
    private static AppSettings? _instance;
    private static readonly object _lock = new(); // Thread safety lock (prevents race conditions)

    /// <summary>
    /// Gets the singleton instance of AppSettings.
    /// If it doesn't exist yet, it creates one and loads from disk.
    /// </summary>
    public static AppSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= Load();
                }
            }
            return _instance;
        }
    }

    // ============================================================
    // SETTINGS PROPERTIES
    // ============================================================

    /// <summary>
    /// Beta Mode Toggle!
    /// When this is ON (true), you unlock experimental features.
    /// When OFF (false), you're a regular user.
    /// Default: OFF because experimental stuff might be janky lol
    /// </summary>
    public bool BetaModeEnabled { get; set; } = false;

    /// <summary>
    /// Experimental PNG/JPEG Export with History!
    /// This only works when BetaModeEnabled is true.
    /// When enabled, PNG/JPEG exports will include your stroke data
    /// embedded in the image metadata, so you can restore it later!
    /// 
    /// It's like giving your images a MEMORY. Absolutely insane tech.
    /// </summary>
    public bool ExperimentalHistoryExportEnabled { get; set; } = false;

    // ============================================================
    // FILE PATH STUFF
    // ============================================================
    // This is where we save the settings file
    // %APPDATA%\PugNetPaint\settings.json
    // (AppData is a hidden folder on Windows where apps store their stuff)

    private static string SettingsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PugNetPaint"
    );

    private static string SettingsFilePath => Path.Combine(SettingsDirectory, "settings.json");

    // ============================================================
    // SAVE METHOD
    // ============================================================
    /// <summary>
    /// Saves the current settings to the JSON file.
    /// Call this after you change any setting!
    /// </summary>
    public void Save()
    {
        try
        {
            // Make sure the directory exists (creates it if not)
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            // Serialize to JSON with nice formatting (indented so humans can read it)
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);

            // Write to file
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            // If something goes wrong, we just silently fail
            // (we don't want the app to crash just because settings couldn't save)
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    // ============================================================
    // LOAD METHOD
    // ============================================================
    /// <summary>
    /// Loads settings from the JSON file.
    /// If the file doesn't exist or is corrupted, returns default settings.
    /// </summary>
    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                return loaded ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            // If loading fails, just use defaults
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return new AppSettings();
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Checks if the experimental history export feature is available.
    /// It's only available when BOTH beta mode AND the feature toggle are enabled.
    /// </summary>
    public bool IsExperimentalHistoryExportAvailable =>
        BetaModeEnabled && ExperimentalHistoryExportEnabled;
}
