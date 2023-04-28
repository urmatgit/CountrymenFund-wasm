using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using MudBlazor;
using System.Diagnostics;
using MediatR;

namespace FSH.BlazorWebAssembly.Client.Pages;

public partial class NewsPost
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IHomePageClient HomePageClient { get; set; } = default!;

    public IEnumerable<Claim>? Claims { get; set; }
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    private PaginationResponseOfNewsPostDto NewsPostList;


    [Parameter] public int PageIndex { get; set; } = 1;

    [Parameter] public int PageSize { get; set; } = 10;


    private bool _loading = true;




    private MudPagination _mudPagination;

    protected override async Task OnInitializedAsync()
    {

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
            NewsPostList = await ApiHelper.ExecuteCallGuardedAsync(
                () => HomePageClient.SearchAsync(Tenant, request),
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
    }
}
