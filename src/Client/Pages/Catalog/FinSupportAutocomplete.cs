using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Net.NetworkInformation;
using static MudBlazor.Icons.Custom;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public class FinSupportAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<FinSupportAutocomplete> L { get; set; } = default!;
    [Inject]
    private IStringLocalizer<SharedResource> LS { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IFinSupportsClient FinSupportsClient { get; set; } = default!;
    private List<FinSupportDto> _finSupports = new List<FinSupportDto>();
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = LS["Financial support"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        SearchFunc = SearchBrands;
        ToStringFunc = GetBrandName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }
    // when the value parameter is set, we have to load that one brand to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => FinSupportsClient.GetAsync(_value), Snackbar) is { } brand)
        {
            _finSupports.Add(brand);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchBrands(string value)
    {
        var filter = new SearchFinSupportsRequest
        {
            IsCompleted= false,
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => FinSupportsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfFinSupportDto response)
        {
            _finSupports = response.Data.ToList();
        }

        return _finSupports.Select(x => x.Id);
    }

    private string GetBrandName(Guid id) =>
        _finSupports.Find(b => b.Id == id)?.Name ?? string.Empty;
}
