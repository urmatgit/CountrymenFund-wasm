using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.HomePage;

public class DropItem 
{

    public SliderDto Value { get; set; }
    public string Name { get
        {
            return (Value != null ? $"{Value.Title} {Value.ImagePath}":"");
        } }
    public string Selector { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
    public DropItem()
    {
        Value = new SliderDto();
    }
}

public partial class ManageHomePage
{
    private MudDropContainer<DropItem> _MudDropContainer=default!;
    private MudList _MudList = default!;
    private  UpdateHomePageRequest _updateHomePageRequest = new();
    private DropItem CurrentSlider = new();
    private CustomValidation? _customValidation;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    public bool BusySubmitting { get; private set; }
    [Inject]
    protected IHomePageClient HomePageClient { get; set; } = default!;
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;
    private bool _canEditHomePage;

    private List<DropItem> _items = new List<DropItem>();
    protected override async Task OnInitializedAsync()
    {

        
        var model =await HomePageClient.GetAsync(Tenant);
        if (model != null)
        {
            _updateHomePageRequest=model.Adapt<UpdateHomePageRequest>();

            foreach(var sld in model.Sliders)
            {
                var item=new DropItem();
                item.Selector = "1";
                item.Value= sld;
                _items.Add(item);
            }
        }
        var state = await AuthState;
        _canEditHomePage = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.HomePage);
        StateHasChanged();
    }
    private void DeleteCurrentImage()
    {
        if (CurrentSlider is not null)
        {
            UpdateItems(CurrentSlider.Name,"");
            
            CurrentSlider.Value.ImagePath = "";
            CurrentSlider.ImageInBytes = string.Empty;

            
            StateHasChanged();
            _MudDropContainer.Refresh();
        }
    }
    private void UpdateItems(string name,string filepath)
    {
        var item = _items.FirstOrDefault(x => x.Name == name);
        if (item != null)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                item.ImageInBytes = string.Empty;
                item.Value.ImagePath=string.Empty;
            }
            else 
                item.Value.ImagePath = filepath;
        }
    }
    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            CurrentSlider.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            CurrentSlider.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            CurrentSlider.Value.ImagePath = e.File.Name;
            //UpdateItems(CurrentSlider.Name,e.File.Name);
            StateHasChanged();
            _MudDropContainer.Refresh();
        }
    }
    private  void goSliderAdd()
    {
        CurrentSlider = new();
        CurrentSlider.Selector = "1";
        StateHasChanged();
    }
    private void ConfirtSliderChange(MouseEventArgs arg)
    {
        //  var tmpItems = _items.ToList();
        var finded = _items.FirstOrDefault(i => i.Value.Title == CurrentSlider.Value.Title);
        if (finded is null){
            CurrentSlider.Selector = "1";
            
            _items.Add(CurrentSlider);
            
        }else
        {
            finded.Selector = "1";
            finded.Value= CurrentSlider.Value;
            finded.ImageInBytes = CurrentSlider.ImageInBytes;
            finded.ImageExtension = CurrentSlider.ImageExtension;
        }
        //_items = tmpItems;
        StateHasChanged();
        _MudDropContainer.Refresh();
        
    }
    private void selectedValueChanged(object selectValue)
    {
        var dropItem = (DropItem)selectValue;
        if (dropItem != null)
        {
            CurrentSlider =new DropItem()
            {
                Value=new SliderDto()
                {
                    Title=dropItem.Value.Title,
                    Description=dropItem.Value.Description,
                    ImagePath=dropItem.Value.ImagePath,
                    Transition = dropItem.Value.Transition
                },
                ImageInBytes=dropItem.ImageInBytes,
                Selector=dropItem.Selector,
                ImageExtension=dropItem.ImageExtension
            };
            
            StateHasChanged();
        }

    }
    private void ItemUpdated(MudItemDropInfo<DropItem> dropItem)
    {

        dropItem.Item.Selector = dropItem.DropzoneIdentifier;
        //CurrentSlider = dropItem.Item.Value;
        //StateHasChanged();
    }
    private async Task SubmitAsync()
    {
        BusySubmitting = true;
        _updateHomePageRequest.Sliders.Clear();
        foreach (var item in _items)
        {
            var sliderDto = item.Value;
            if (!string.IsNullOrEmpty(item.ImageInBytes))
            {
                sliderDto.Image = new FileUploadRequest()
                {
                    Data = item.ImageInBytes,
                    Extension = item.ImageExtension,
                    Name = item.Value.ImagePath
                };
            }
            _updateHomePageRequest.Sliders.Add(sliderDto);
        }
        var sucessMessage = await ApiHelper.ExecuteCallGuardedAsync(
            () => HomePageClient.PostAsync( _updateHomePageRequest),
            Snackbar,
            _customValidation);

        if (sucessMessage != null)
        {
            Snackbar.Add(LS["The home page updated!"], Severity.Info);
            //Navigation.NavigateTo("/login");
        }

        BusySubmitting = false;
    }
}
