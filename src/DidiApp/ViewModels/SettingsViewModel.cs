using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace DidiApp.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private bool _isBoysMode;
    private string _selectedLanguage = "English";

    public SettingsViewModel()
    {
        Languages = new List<string> { "English", "Русский", "Latviešu", "Deutsch" };
    }

    public List<string> Languages { get; }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set { if (SetProperty(ref _selectedLanguage, value)) ChangeLanguage(value); }
    }

    public bool IsDarkMode
    {
        get => Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;
        set
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
                RaisePropertyChanged();
                ApplyThemeColors(); 
            }
        }
    }

    public bool IsBoysMode
    {
        get => _isBoysMode;
        set { if (SetProperty(ref _isBoysMode, value)) ApplyThemeColors(); }
    }

    private void ApplyThemeColors()
    {
        if (Application.Current == null) return;
        var res = Application.Current.Resources;
        bool isDark = IsDarkMode;
        
        if (_isBoysMode)
        {
            
            res["AccentBrush"] = SolidColorBrush.Parse("#2563EB"); 
            res["AccentHoverBrush"] = SolidColorBrush.Parse("#1D4ED8");
            res["SoftBtnBrush"] = isDark ? SolidColorBrush.Parse("#1E3A8A") : SolidColorBrush.Parse("#DBEAFE");
            res["BgBrush"] = isDark ? SolidColorBrush.Parse("#0F172A") : SolidColorBrush.Parse("#F0F8FF");
            res["CardBrush"] = isDark ? SolidColorBrush.Parse("#1E293B") : SolidColorBrush.Parse("#FFFFFF");
            res["Card2Brush"] = isDark ? SolidColorBrush.Parse("#334155") : SolidColorBrush.Parse("#EFF6FF");
        }
        else
        {
            // GIRLY MODE (Pink / Blush)
            res["AccentBrush"] = SolidColorBrush.Parse("#FFEC4899");
            res["AccentHoverBrush"] = SolidColorBrush.Parse("#FFDB2777");
            res["SoftBtnBrush"] = isDark ? SolidColorBrush.Parse("#831843") : SolidColorBrush.Parse("#FFFCE7F3");
            res["BgBrush"] = isDark ? SolidColorBrush.Parse("#121212") : SolidColorBrush.Parse("#FFFDF7FB");
            res["CardBrush"] = isDark ? SolidColorBrush.Parse("#1E1E1E") : SolidColorBrush.Parse("#FFFFFFFF");
            res["Card2Brush"] = isDark ? SolidColorBrush.Parse("#252525") : SolidColorBrush.Parse("#FFF8EAF2");
        }
    }

    private void ChangeLanguage(string lang)
    {
        if (Application.Current == null) return;
        string code = lang switch { "Русский" => "ru", "Latviešu" => "lv", "Deutsch" => "de", _ => "en" };
        try
        {
            var uri = new Uri($"avares://DidiApp/Assets/Lang/{code}.axaml");
            var newDict = (ResourceDictionary)AvaloniaXamlLoader.Load(uri);
            var merged = Application.Current.Resources.MergedDictionaries;
            var oldDict = merged.OfType<ResourceDictionary>().FirstOrDefault(d => d.ContainsKey("AppTitle"));
            if (oldDict != null) merged.Remove(oldDict);
            merged.Add(newDict);
        }
        catch
        {
            
            //ignore
        }
    }
}