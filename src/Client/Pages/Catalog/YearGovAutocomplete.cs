using Blazored.LocalStorage;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public class YearGovAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<YearGovAutocomplete> L { get; set; } = default!;
    [Inject]
    private IStringLocalizer<SharedResource> SL { get; set; } = default!;
    [Inject]
    private IYearsClient YearsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Parameter]
    public string? DefaultText { get; set; }=default!;
    private List<YearDto> _ruralGovs = new();


    protected override async void OnInitialized()
    {
       await  SearchRuralGovs("");
        var currentYearId = GetCurrentYearId();
        if (currentYearId== null)
            this.SelectOption(currentYearId.Value);
        // if (_ruralGovs!=null && _ruralGovs.Count>0)
        //     this.SelectOption(_ruralGovs.FirstOrDefault()!.Id);
        // Console.WriteLine($"Initialized val {this.Text}");
        // this.ForceRender(true);
        base.OnInitialized();
    }
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = SL["Year"];
        Variant = Variant.Filled;
        Dense = true;
        
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchRuralGovs;
        ToStringFunc = GetRuralGovName;
        Clearable = true;
        
        return base.SetParametersAsync(parameters);
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
       
        if (firstRender && _value!=default
             && await ApiHelper.ExecuteCallGuardedAsync(()=>YearsClient.GetAsync(_value),Snackbar) is { } ruralGov)
        {
            _ruralGovs.Add(ruralGov);
            ForceRender(true);
        }
        
    }
    private string GetRuralGovName(Guid arg)
    => _ruralGovs.Find(b => b.Id == arg)?.Year!.ToString() ?? string.Empty;

    private async Task<IEnumerable<Guid>> SearchRuralGovs(string arg)
    {
        var filter = new SearchYearsRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "year" }, Keyword = arg }
        };
        if (await ApiHelper.ExecuteCallGuardedAsync(
            ()=>YearsClient.SearchAsync(filter),Snackbar) is PaginationResponseOfYearDto response)
        {
            _ruralGovs = response.Data.ToList();
        }
        return _ruralGovs.Select(r => r.Id);
    }
    public Guid? GetCurrentYearId()
    {
        if (_ruralGovs is null || _ruralGovs.Count()== 0) return null;
        return _ruralGovs.Find(y => y.Year == DateTime.Now.Year)?.Id;
            
    }
}
