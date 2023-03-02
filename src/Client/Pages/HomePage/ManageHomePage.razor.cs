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
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.HomePage;

public class DropItem: Slide
{

    public string Name { get
        {
            return $"{Title} {ImagePath}";
        }
    }
    public string Selector { get; set; }
}

public partial class ManageHomePage
{
    private  UpdateHomePageRequest _updateHomePageRequest = new();
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
                var item=sld.Adapt<DropItem>();
                item.Selector = "1";
                _items.Add(item);
            }
        }
        var state = await AuthState;
        _canEditHomePage = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.HomePage);
        StateHasChanged();
    }
    private void ItemUpdated(MudItemDropInfo<DropItem> dropItem)
    {

        dropItem.Item.Selector = dropItem.DropzoneIdentifier;
    }
    private async Task SubmitAsync()
    {
        BusySubmitting = true;
        
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
