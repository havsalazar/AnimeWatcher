﻿using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using AnimeWatcher.Contracts.Services;
using AnimeWatcher.Contracts.ViewModels;
using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using AnimeWatcher.Core.Services;
using AnimeWatcher.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel;

namespace AnimeWatcher.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly SearchAnimeService _searchAnimeService = new();
    private readonly AppUpdateService _appUpdateService = new();

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string versionMessage = "";

    [ObservableProperty]
    private string versionExtensions = "";
    [ObservableProperty]
    private string _appName = "";

    [ObservableProperty]
    private int selectedThemeIndex;
    public ICommand SwitchThemeCommand
    {
        get;
    }

    [ObservableProperty]
    private Provider selectedProvider;
    public ObservableCollection<Provider> Providers { get; } = new ObservableCollection<Provider>();

    public SettingsViewModel(
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService
    )
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _localSettingsService = localSettingsService;
        _versionDescription = GetVersionDescription();
        _appName = GetAppName();
        GetExtensionVersion();

        SwitchThemeCommand = new RelayCommand<ComboBox>(
            async (cb) =>
            {
                var selected = ElementTheme.Dark;
                switch (cb.SelectedIndex)
                {
                    case 0:
                        selected = ElementTheme.Light;
                        break;
                    case 1:
                        selected = ElementTheme.Dark;
                        break;
                    case 2:
                        selected = ElementTheme.Default;
                        break;
                    default:
                        selected = ElementTheme.Default;
                        break;
                }
                await _themeSelectorService.SetThemeAsync(selected);
            }
        );
    }

    private void GetExtensionVersion()
    {
        var currv = _appUpdateService.GetExtensionVer().ToString();
        VersionExtensions = $"Extensions version : {currv}";
    }
    private string GetAppName()
    {
        return $"{"AppDisplayName".GetLocalized()}";
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(
                packageVersion.Major,
                packageVersion.Minor,
                packageVersion.Build,
                packageVersion.Revision
            );
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"Version - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public async void OnNavigatedTo(object parameter)
    {
        await GetProviders();
        var provdef = await _localSettingsService.ReadSettingAsync<int>("ProviderId");

        if (provdef != 0)
        {
            var tmp = Providers.FirstOrDefault(p => p.Id == provdef);
            SelectedProvider = tmp != null ? tmp : new();
        }
        var currentTheme = _themeSelectorService.Theme;

        switch (currentTheme)
        {
            case ElementTheme.Light:
                SelectedThemeIndex = 0;
                break;
            case ElementTheme.Dark:
                SelectedThemeIndex = 1;
                break;
            case ElementTheme.Default:
                SelectedThemeIndex = 2;
                break;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task GetProviders()
    {
        var provs = _searchAnimeService.GetProviders();
        foreach (var item in provs)
        {
            Providers.Add(item);
        }
        SelectedProvider = provs[0];
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ChangedProvider()
    {
        if (SelectedProvider != null)
            await _localSettingsService.SaveSettingAsync<int>("ProviderId", SelectedProvider.Id);
    }

    [RelayCommand]
    private async void CheckUpdates()
    {
        try
        {
            VersionMessage = "Searching for updates";

            var result = await _appUpdateService.CheckMainUpdates();
            var verEval = result.Item1;
            var version = result.Item2;
            if (verEval > 0)
            {
                VersionMessage = "The running version is higher than the main version; it is not recommended to update in debug mode.";
            }
            else if (verEval < 0)
            {
                VersionMessage = "Update Available";
            }
            else
            {
                VersionMessage = "Same version";
            }

            var tmpNotes = await _appUpdateService.GetReleaseNotes();
            var patchNotes = (string)JObject.Parse(tmpNotes)["body"];

            onPatchNotes(this, (patchNotes, version.ToString()));


        } catch (Exception e)
        {
            VersionMessage = e.ToString();
        }
    }
    [RelayCommand]
    private async void CheckPatchNotes()
    {
        var vt = Assembly.GetExecutingAssembly().GetName().Version!;
        var version = $"{vt.Major}.{vt.Minor}.{vt.Build}";


        //var tmpNotes = await _appUpdateService.GetReleaseNotes(version);
        var tmpNotes = await _appUpdateService.GetReleaseNotes("0.2");


        var patchNotes = (string)JObject.Parse(tmpNotes)["body"];

        onPatchNotes(this, (patchNotes, version.ToString()));

    }

    public event EventHandler<(string Notes, string version)> onPatchNotes;

}
