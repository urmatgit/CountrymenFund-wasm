using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using MudBlazor;
using FSH.WebApi.Shared.Multitenancy;
using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Shared;

namespace FSH.BlazorWebAssembly.Client.Pages;

public partial class Index
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IHomePageClient HomePageClient { get; set; } = default!;

    public IEnumerable<Claim>? Claims { get; set; }

    private MudCarousel<SliderDto> _carousel;
    private MudGrid _mugGrid;
    private bool _arrows = true;
    private bool _bullets = true;
    private bool _enableSwipeGesture = true;
    private bool _autocycle = true;
    private Transition _transition = Transition.Slide;
    private string _height = "height: 200px;";
    private TimeSpan _autocycleTime = TimeSpan.FromSeconds(5);
    private List<SliderDto> mudCarouselItems= new List<SliderDto>();
    private List<TextBlockDto> TextBlocks = new List<TextBlockDto>();
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        Claims = authState.User.Claims;
        var mainpageModel = await ApiHelper.ExecuteCallGuardedAsync(
            () => HomePageClient.GetAsync(Tenant),
            Snackbar);
        
        //var mainpageModel = await HomePageClient.GetAsync(Tenant);
        if (mainpageModel is not null)
        {
            _arrows = mainpageModel.ShowArrows;
            _bullets=mainpageModel.ShowBullets;
            _enableSwipeGesture = mainpageModel.EnableSwapGesture;
            _autocycle = mainpageModel.AutoCycle;
            _height = $"height: {mainpageModel.Height}px;";
            if (mainpageModel.AutoCycleTime !=0)
                _autocycleTime =  TimeSpan.FromSeconds(mainpageModel.AutoCycleTime);
            if (mainpageModel.Sliders != null && mainpageModel.Sliders.Count > 0)
            {
                mudCarouselItems = mainpageModel.Sliders.ToList();
            }
            TextBlocks=mainpageModel.TextBlocs.ToList();
            StateHasChanged();
        }
        
    }
}
