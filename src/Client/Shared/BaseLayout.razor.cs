using FSH.BlazorWebAssembly.Client.Infrastructure.Preferences;
using FSH.BlazorWebAssembly.Client.Infrastructure.Theme;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;

namespace FSH.BlazorWebAssembly.Client.Shared;

public partial class BaseLayout
{

    [Inject]
   private NavigationManager MyNavigationManager { get; set; }=default!;
    private ClientPreference? _themePreference;
    private MudTheme _currentTheme = new LightTheme();
    private bool _themeDrawerOpen;
    private bool _rightToLeft;
    private bool isLoginPagge { get; set; }=false;
    void CheckLoginPage()
    {
        var pageUrl = MyNavigationManager.Uri;
        if (!string.IsNullOrEmpty(pageUrl))
        {
            
            pageUrl = pageUrl.ToLower()!.Replace(MyNavigationManager.BaseUri, "");
            if (pageUrl!.StartsWith("login") && isLoginPagge==false)
            {
                isLoginPagge = true;
                StateHasChanged();
            }
        }
    }
    protected override async Task OnInitializedAsync()
    {
        _themePreference = await ClientPreferences.GetPreference() as ClientPreference;
        if (_themePreference == null) _themePreference = new ClientPreference();
        SetCurrentTheme(_themePreference);
     //  CheckLoginPage();
        //Snackbar.Add("Like this boilerplate? ", Severity.Normal, config =>
        //{
        //    config.BackgroundBlurred = true;
        //    config.Icon = Icons.Custom.Brands.Telegram;
        //    config.Action = "Star us on Github!";
        //    config.ActionColor = Color.Primary;
        //    config.Onclick = snackbar =>
        //    {
        //        Navigation.NavigateTo("https://github.com/fullstackhero/blazor-wasm-boilerplate");
        //        return Task.CompletedTask;
        //    };
        //});
    }
    
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        CheckLoginPage();
        
        return base.OnAfterRenderAsync(firstRender);
    }
    private async Task ThemePreferenceChanged(ClientPreference themePreference)
    {
        SetCurrentTheme(themePreference);
        
        await ClientPreferences.SetPreference(themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _currentTheme = themePreference.IsDarkMode ? new DarkTheme() : new LightTheme();
        _currentTheme.Palette.Primary = themePreference.PrimaryColor;
        _currentTheme.Palette.Secondary = themePreference.SecondaryColor;
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _rightToLeft = themePreference.IsRTL;
    }
}