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
using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using System.Reflection;

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
        if (!Navigation.Uri.Equals(Navigation.BaseUri))
        {
            _canEditNewsPostPage = await AuthService.HasPermissionAsync(authState.User, FSHAction.Update, FSHResource.NewsPost);
            _canDeleteNewsPostPage = await AuthService.HasPermissionAsync(authState.User, FSHAction.Delete, FSHResource.NewsPost);
        }
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

    private async Task AddTextBlockAsync()
    {
        var newspost = new UpdateNewsPostRequest();

        await AddOrEditNewsPostAsync(newspost, true);

    }
    private async Task editNewsPost(NewsPostDto newsPostDto)
    {
        await AddOrEditNewsPostAsync(newsPostDto.Adapt<UpdateNewsPostRequest>(), false);
    }
    private async Task deleteNewsPost(NewsPostDto newsPostDto)
    {
        string deleteContent = LS["You're sure you want to delete {0} ?"];

        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), string.Format(deleteContent, newsPostDto.Title) }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(LS["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            //NewsPostItems.Data.Remove(newsPostDto);
            var retuslt = await ApiHelper.ExecuteCallGuardedAsync(
            () => NewsPostClient.DeleteAsync(newsPostDto.Id ),
             Snackbar);
            await LoadPosts();
            //await Task.Delay(1);

        }
    }
    private async Task AddOrEditNewsPostAsync(UpdateNewsPostRequest model, bool isCreate)
    {
        string title = LS["Update"];
        if (isCreate)
        {
            title = LS["Create"];
        }
        Func<UpdateNewsPostRequest, Task> saveFunc;
        if (!isCreate)
        {
            
            saveFunc= model =>NewsPostClient.UpdateAsync(model.Id, model);
              
        }
        else
        {

            saveFunc = model => NewsPostClient.CreateAsync(model.Adapt<CreateNewsPostRequest>());
            
        }
        var result = await DialogService.ShowModalAsync<AddOrUpdateNewsPost>(new()
        {
            { nameof(AddOrUpdateNewsPost.IsCreate), isCreate },
            { nameof(AddOrUpdateNewsPost.NewsPostModel), model },
            { nameof(AddOrUpdateNewsPost.Caption), title },
            { nameof(AddOrUpdateNewsPost.SaveFunc), saveFunc }


        });

        if (!result.Cancelled)
        {
            
            await LoadPosts();
        }
        
    }
}
