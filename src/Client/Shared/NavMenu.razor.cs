﻿using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FSH.BlazorWebAssembly.Client.Shared;

public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private string? _hangfireUrl;
    private bool _canViewHangfire;
    private bool _canViewDashboard;
    private bool _canViewRoles;
    private bool _canViewUsers;
    private bool _canViewProducts;
    private bool _canViewBrands;
    private bool _canViewTenants;
    private bool _canViewRuralGovs;
    private bool _canViewNatives;
    private bool _canViewYears;
    private bool _canViewContributions;
    private bool _canEditHomePage;
    private bool _canViewFinSupports;
    private bool _canViewLogs;
    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewTenants || _canEditHomePage;

    protected override async Task OnParametersSetAsync()
    {
        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";
        var user = (await AuthState).User;
        _canViewHangfire = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Hangfire);
        _canViewDashboard = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Dashboard);
        _canViewRoles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Users);
        _canViewProducts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Products);
        _canViewBrands = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Brands);
        _canViewTenants = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tenants);
        _canViewRuralGovs = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.RuralGovs);
        _canViewNatives = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Natives);
        _canViewYears = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Years);
        _canViewContributions = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Contributions);
        _canEditHomePage=await AuthService.HasPermissionAsync(user,FSHAction.Update,FSHResource.HomePage);
        _canViewFinSupports = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.FinSupport);
        _canViewLogs=await AuthService.HasPermissionAsync(user,FSHAction.View,FSHResource.Logs);
    }
}