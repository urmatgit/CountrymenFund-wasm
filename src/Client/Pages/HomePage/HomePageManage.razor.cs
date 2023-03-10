using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Components.Fund;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient.Fund;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System;
using System.Security.Claims;
using static MudBlazor.CategoryTypes;

namespace FSH.BlazorWebAssembly.Client.Pages.HomePage;

public partial class HomePageManage
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IHomePageClient HomePageClient { get; set; } = default!;

    public IEnumerable<Claim>? Claims { get; set; }


    private MudDropContainer<DropItem> _MudDropContainer = default!;
    private MudCarousel<SlideViewModel> _carousel;
    private MudGrid _mugGrid;
    private bool _arrows = true;
    private bool _bullets = true;
    private bool _enableSwipeGesture = true;
    private bool _autocycle = true;
    private int slideTimeSec = 5;
    private TimeSpan _autocycleTime
    {
        get
        {
            return TimeSpan.FromSeconds(slideTimeSec);
        }
    }
    private Transition _transition = Transition.Slide;
    private string _height = "height: 200px;";
    
    private List<SlideViewModel> mudCarouselItems = new List<SlideViewModel>();
    private List<TextBlockDto> TextBlocks = new List<TextBlockDto>();
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    private bool _canEditHomePage;

    private int selectedIndex = 1;
    
    public bool BusySubmitting { get; private set; }
    MainPageModelDto mainpageModel;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        Claims = authState.User.Claims;
        
        _canEditHomePage = await AuthService.HasPermissionAsync(authState.User, FSHAction.Update, FSHResource.HomePage);
          mainpageModel = await ApiHelper.ExecuteCallGuardedAsync(
            () => HomePageClient.GetAsync(Tenant),
            Snackbar);
        //var mainpageModel = await HomePageClient.GetAsync(Tenant);
        if (mainpageModel is not null)
        {
            _autocycle = mainpageModel.AutoCycle;
            _height = $"height: {mainpageModel.Height}px;";
            if (mainpageModel.AutoCycleTime is not null)
                slideTimeSec = Convert.ToInt32(mainpageModel.AutoCycleTime);
            if (mainpageModel.Slides != null && mainpageModel.Slides.Count > 0)
            {
                mudCarouselItems = mainpageModel.Slides.Select(x => x.Adapt<SlideViewModel>()).ToList();
            }
            TextBlocks = mainpageModel.TextBlocs.ToList();
            StateHasChanged();
        }
    }
    private async Task UpdateAsync()
    {
        int currentIndex = selectedIndex;
        var slide = mudCarouselItems[currentIndex];

        var model= await AddOrUpdateAsync(slide.Adapt<SlideViewModel>(), false);
        if (model is not null )
        {

            mudCarouselItems[currentIndex] = model;
        }
    }
    private async Task AddAsync() {
       var model=await  AddOrUpdateAsync(new SlideViewModel(),true);
        if (model is not null)
        {
            if (string.IsNullOrEmpty(model.Title) && model.Image is null)
            {
                throw new Exception("Параметры не заданы!!!");
            }
            mudCarouselItems.Add(model);
        }
    }
    private async Task<SlideViewModel> AddOrUpdateAsync(SlideViewModel model,bool isCreate)
    {
        string title = LS["Update"];
        if (isCreate)
        {
            title = LS["Create"];
        }

        var result = await DialogService.ShowModalAsync<SliderAddOrUpdateDialog>(new()
        {
            { nameof(SliderAddOrUpdateDialog.IsCreate), isCreate },
            { nameof(SliderAddOrUpdateDialog.sliderModel), model },
            { nameof(SliderAddOrUpdateDialog.Caption), title }

        });

        if (!result.Cancelled)
        {
            return model;
        }
        return null;
    }

    private async Task DeleteAsync(int index)
    {
        string deleteContent = LS["You're sure you want to delete {0} ?"];
        var slide = mudCarouselItems[index];
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), string.Format(deleteContent, slide.Title) }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(LS["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            mudCarouselItems.RemoveAt(index);
            await Task.Delay(1);
            _carousel.MoveTo(System.Math.Max(index, mudCarouselItems.Count - 1));
        }
    }

    private async Task SubmitAsync()
    {
        BusySubmitting = true;
        mainpageModel.Slides.Clear();
        mainpageModel.AutoCycle = _autocycle;
        mainpageModel.AutoCycleTime = _autocycleTime.ToString();
        foreach (var item in mudCarouselItems)
        {
            
            if (!string.IsNullOrEmpty(item.ImageInBytes) && item.Image==null)
            {
                item.Image = new FileUploadRequest()
                {
                    Data = item.ImageInBytes,
                    Extension = item.ImageExtension,
                    Name = item.ImagePath
                };
            }
            mainpageModel.Slides.Add(item.Adapt<SlideDto>());
        }
        var sucessMessage = await ApiHelper.ExecuteCallGuardedAsync(
            () => HomePageClient.PostAsync(mainpageModel.Adapt<UpdateHomePageRequest>()),
            Snackbar);

        if (sucessMessage != null)
        {
            Snackbar.Add(LS["The home page updated!"], Severity.Info);
            //Navigation.NavigateTo("/login");
        }

        BusySubmitting = false;
    }
}
