using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using MudBlazor;
using System.Diagnostics;
using MediatR;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Pages.HomePage;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.NewsPosts;

public partial class NewsPostList
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected INewsPostClient NewsPostClient { get; set; } = default!;

    public IEnumerable<Claim>? Claims { get; set; }
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    private PaginationResponseOfNewsPostDto NewsPostItems;

    [Parameter] public bool  ManageMode { get; set; } = false;

    [Parameter] public int PageIndex { get; set; } = 1;

    [Parameter] public int PageSize { get; set; } = 10;


    private bool _loading = true;
    private bool _canEditNewsPostPage;
    private bool _canDeleteNewsPostPage;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private MudPagination _mudPagination;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        Claims = authState.User.Claims;

        _canEditNewsPostPage = await AuthService.HasPermissionAsync(authState.User, FSHAction.Update, FSHResource.NewsPost);
        _canDeleteNewsPostPage = await AuthService.HasPermissionAsync(authState.User, FSHAction.Delete, FSHResource.NewsPost);
        await LoadPosts();

    }
    private async Task LoadPosts()
    {
        _loading = true;
        try
        {
            SearchNewsPostRequest request = new SearchNewsPostRequest();
            request.PageSize = this.PageSize;
            request.PageNumber = PageIndex;
            NewsPostItems = await ApiHelper.ExecuteCallGuardedAsync(
                () => NewsPostClient.SearchAsync(Tenant, request),
                Snackbar);

        }
        finally
        {
            _loading = false;
        }
    }
    private async void ToPage(int page)
    {
        PageIndex = page;
        await LoadPosts();
        
        StateHasChanged();
    }

    private async Task editNewsPost(NewsPostDto newsPostDto)
    {
        await AddOrEditNewsPostAsync(newsPostDto.Adapt<UpdateNewsPostRequest>(), false);
    }
    private async Task AddOrEditNewsPostAsync(UpdateNewsPostRequest model, bool isCreate)
    {
        string title = LS["Update"];
        if (isCreate)
        {
            title = LS["Create"];
        }

        var result = await DialogService.ShowModalAsync<AddOrUpdateNewsPost>(new()
        {
            { nameof(AddOrUpdateNewsPost.IsCreate), isCreate },
            { nameof(AddOrUpdateNewsPost.NewsPostModel), model },
            { nameof(AddOrUpdateNewsPost.Caption), title }

        });

        if (!result.Cancelled)
        {
            
            var retuslt = await ApiHelper.ExecuteCallGuardedAsync(
              () => NewsPostClient.UpdateAsync(model.Id,model),
              Snackbar);
            await LoadPosts();
        }
        
    }
}
