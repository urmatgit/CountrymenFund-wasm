using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.HomePage;

public class DropItem 
{

    public Slide Value { get; set; }
    public string Name { get
        {
            return (Value != null ? $"{Value.Title} {Value.ImagePath}":"");
        } }
    public string Selector { get; set; }
    public DropItem()
    {
        Value = new Slide();
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

            foreach(var sld in model.CarouselModel.Slides)
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
    private  void goSliderAdd()
    {
        CurrentSlider = new();
        CurrentSlider.Selector = "1";
        StateHasChanged();
    }
    private void ConfirtSliderChange(MouseEventArgs arg)
    {
        var tmpItems = _items.ToList();
        if (!tmpItems.Any(i => i.Name == CurrentSlider.Name)){
            tmpItems.Add(CurrentSlider);
            
        }
        _items = tmpItems;
        //StateHasChanged();
        _MudDropContainer.Refresh();
    }
    private void selectedValueChanged(object selectValue)
    {
        var dropItem = (DropItem)selectValue;
        if (dropItem != null)
        {
            CurrentSlider = dropItem;
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
        _updateHomePageRequest.CarouselModel.Slides.Clear();
        foreach (var item in _items)
        {
            _updateHomePageRequest.CarouselModel.Slides.Add(item.Value);
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
