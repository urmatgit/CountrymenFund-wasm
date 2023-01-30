using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public class NativeAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<NativeAutocomplete> L { get; set; } = default!;
    [Inject]
    private INativesClient NativesClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<NativeDto> _natives = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Native"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchNatives;
        ToStringFunc = GetNativeName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one native to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => NativesClient.GetAsync(_value), Snackbar) is { } native)
        {
            _natives.Add(native);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchNatives(string value)
    {
        var filter = new SearchNativesRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name","surname","middlename" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => NativesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfNativeDto response)
        {
            _natives = response.Data.ToList();
        }

        return _natives.Select(x => x.Id);
    }

    private string GetNativeName(Guid id)
    {
        var finded = _natives.Find(b => b.Id == id);
        if (finded is not null)
            return $"{finded.Name} {finded.Surname} {finded.MiddleName}";
        else return string.Empty;
    }
}