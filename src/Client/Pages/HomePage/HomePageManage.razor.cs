using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Components.Fund;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient.Fund;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.BlazorWebAssembly.Client.Pages.NewsPosts;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Authorization;

using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Reflection;
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
    private NewsPostList _newsPost;
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
    private List<NewsPostDto> TextBlocks = new List<NewsPostDto>();
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    private bool _canEditHomePage;

    private int selectedIndex = 1;
    private int CurrentBlockImageIndex = 0;

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
            _arrows = mainpageModel.ShowArrows;
            _bullets = mainpageModel.ShowBullets;
            _enableSwipeGesture = mainpageModel.EnableSwapGesture;
              
            if (mainpageModel.AutoCycleTime !=0)
                slideTimeSec =mainpageModel.AutoCycleTime;
            if (mainpageModel.Sliders != null && mainpageModel.Sliders.Count > 0)
            {
                mudCarouselItems = mainpageModel.Sliders.Select(x => x.Adapt<SlideViewModel>()).ToList();
            }
           // TextBlocks = mainpageModel.TextBlocs.ToList();
            StateHasChanged();
        }
    }
    private async Task DeleteCurrentImageFromBlock(TextBlockDto textBlockDto)
    {
        var image = textBlockDto.Images.ElementAt(CurrentBlockImageIndex);
        if (image is not null)
        {
            textBlockDto.Images.Remove(image);
            CurrentBlockImageIndex = textBlockDto.Images.Count > 0? textBlockDto.Images.Count-1 : 0;
        }
    }
    private async Task UploadFilesTextBlock(InputFileChangeEventArgs e,string id)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }
            var txtBlock = TextBlocks.FirstOrDefault(s => s.Id == Guid.Parse(id));
            //if (txtBlock != null)
            //{
            //    BlockImageDto blockImage =  new BlockImageDto();
            //    if (txtBlock.Images == null) txtBlock.ImagesDto = new List<BlockImageDto>();
            //    txtBlock.ImagesDto.Add(blockImage);
            //    CurrentBlockImageIndex = txtBlock.ImagesDto.Count-1;
                        
            //    blockImage.ImageExtension = extension;
            //    var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            //    byte[]? buffer = new byte[imageFile.Size];
            //    await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);

            //    blockImage.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            //    blockImage.Name = e.File.Name;
            //    StateHasChanged();
            //}
        }
    }
    private async Task EditTextBlockAsync(NewsPostDto textBlockDto)
    {
        
        var model = await AddOrEditTextBlockAsync(textBlockDto, false);
        if (model is not null)
        {
            

        }
    }
  
    private async Task DeleteTextBlockAsync(NewsPostDto textBlockDto)
    {
        string deleteContent = LS["You're sure you want to delete {0} ?"];
        
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), string.Format(deleteContent, textBlockDto.Title) }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(LS["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            TextBlocks.Remove(textBlockDto);
            await Task.Delay(1);
            
        }
        
    }
    private async Task AddTextBlockAsync()
    {
        var textblock = new NewsPostDto()
        {
            Id= Guid.NewGuid(),
        };
        var model =await  AddOrEditTextBlockAsync(textblock, true);
        if (model is not null)
        {
            TextBlocks.Add(model);

        }
    }
    private async Task<NewsPostDto> AddOrEditTextBlockAsync (NewsPostDto model, bool isCreate)
    {
        string title = LS["Update"];
        if (isCreate)
        {
            title = LS["Create"];
        }

        var result = await DialogService.ShowModalAsync<AddOrUpdateTextBlock> (new()
        {
            { nameof(AddOrUpdateTextBlock.IsCreate), isCreate },
            { nameof(AddOrUpdateTextBlock.TextBlockModel), model },
            { nameof(AddOrUpdateTextBlock.Caption), title }

        });

        if (!result.Cancelled)
        {
            return model;
        }
        return null;
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
            if (mudCarouselItems.Count>0)
                _carousel.MoveTo(System.Math.Max(index, mudCarouselItems.Count - 1));
        }
    }

    private async Task SubmitAsync()
    {
        BusySubmitting = true;
        mainpageModel.Sliders.Clear();
        mainpageModel.AutoCycle = _autocycle;
        mainpageModel.AutoCycleTime = slideTimeSec;
        mainpageModel.ShowArrows = _arrows;
        mainpageModel.ShowBullets = _bullets;
        mainpageModel.EnableSwapGesture = _enableSwipeGesture;
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
            mainpageModel.Sliders.Add(item.Adapt<SliderDto>());
        }
        mainpageModel.TextBlocs.Clear();
        //TextBlocks.ForEach(x => 
        //{
        //    if (x.Images != null)
        //    {
        //        foreach (BlockImageDto image in x.Images?.Where(x=>!string.IsNullOrEmpty(x.ImageInBytes)))
        //        {
                    
        //                image.Image = new FileUploadRequest()
        //                {
        //                    Name = image.Name,
        //                    Data = image.ImageInBytes,
        //                    Extension = image.ImageExtension
        //                };
                    
        //            // image.ImageInBytes = null;
        //        }
        //    }
        //});
       //mainpageModel.TextBlocs = TextBlocks;
        
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
