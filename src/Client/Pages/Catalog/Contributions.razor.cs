using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using Mapster;
using MudBlazor;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Bson;
using Microsoft.JSInterop;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.WebUtilities;
using MediatR.Courier;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Contributions
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; }
    [Inject]
    protected IStringLocalizer<Contributions> L { get; set; }
    [Inject]
    protected IContributionsClient ContributionsClient { get; set; } = default!;
    [Inject]
    protected IYearsClient YearsClient { get; set; } = default!;
    [Inject]
    protected INativesClient NativesClient { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();

    
    
    
    
    protected async Task Finded(bool finded)
    {
        
        var result=await JsRuntime.InvokeAsync<string>("ShowElement", "addNative",!finded);
        StateHasChanged(); 
    }
    private  void goNativeAdd()
    {
         Navigation.NavigateTo($"/catalog/natives/{Guid.Empty}");
    }
    protected EntityServerTableContext<ContributionDto, Guid, UpdateContributionRequest> Context { get; set; } = default!;

    private EntityTableWithGrouping<ContributionDto, Guid, UpdateContributionRequest> _table = default!;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: L["Contribution"],
            entityNamePlural: L["Contributions"],
            entityResource: FSHResource.Contributions,
            fields: new()
            {
                new(prod => prod.Id, L["Id"], "Id"),
                new(prod => prod.RuralGovName, L["RuralGov"], "RuralGovName"),
                new(prod => prod.Year, L["Year"], "Year"),
                new(prod => prod.NativeFIO, L["FIO"], "NativeFIO"),
                new(prod =>SH.GetString(prod.Month.ToString()), L["Month"], "Month"),
                new(prod => prod.Summa, L["Summa"], "Summa",Type: typeof(decimal?)),
                new(prod => prod.Date, L["Date"], "Date"),
                new(prod => prod.Description, L["Description"], "Description")
                //new(prod => prod.Rate, L["Rate"], "Rate",Template: RateFieldTemplate)
            },
            enableAdvancedSearch: false,
            idFunc: prod => prod.Id,
            searchFunc: async filter =>
            {
                var contributionFilter = filter.Adapt<SearchContributionsRequest>();

                contributionFilter.YearId = SearchYearId == default ? null : SearchYearId;
                contributionFilter.NativeId = SearchNativeId == default ? null : SearchNativeId;
                contributionFilter.Month=SearchMonth== default ? null : SearchMonth;
                contributionFilter.DateStart = SearchDateRange == default ? null : SearchDateRange.Start;
                contributionFilter.DateEnd= SearchDateRange == default ? null : SearchDateRange.End;
                contributionFilter.RuralGovId = SearchRuralGovId == default ? null : SearchRuralGovId;
                var result = await ContributionsClient.SearchAsync(contributionFilter);
                return result.Adapt<PaginationResponse<ContributionDto>>();
            },
            createFunc: async prod =>
            {


                await ContributionsClient.CreateAsync(prod.Adapt<CreateContributionRequest>());

            },
            updateFunc: async (id, prod) =>
            {

                await ContributionsClient.UpdateAsync(id, prod.Adapt<UpdateContributionRequest>());

            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportContributionsRequest>();

                exportFilter.YearId = SearchYearId == default ? null : SearchYearId;

                
                exportFilter.NativeId = SearchNativeId == default ? null : SearchNativeId;
                exportFilter.Month = SearchMonth == default ? null : SearchMonth;
                exportFilter.DateStart = SearchDateRange == default ? null : SearchDateRange.Start;
                exportFilter.DateEnd = SearchDateRange == default ? null : SearchDateRange.End;
                exportFilter.RuralGovId = SearchRuralGovId == default ? null : SearchRuralGovId;
                return await ContributionsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportRuralGovReques() { ExcelFile = FileUploadRequest };
                await ContributionsClient.ImportAsync(request);
            },
            deleteFunc: async id => await ContributionsClient.DeleteAsync(id)
            , getDefaultsFunc: async () =>
            {
            
                var yearDefault = await ContributionsClient.GetDefaultAsync();
                Console.WriteLine($"Default year is {yearDefault.Id}");
                return new UpdateContributionRequest()
                {
                    Month=yearDefault.Month,
                    Date=yearDefault.Date,
                    YearId=yearDefault.YearId,
                };
            },
           
            GroupSeletor: (x)=>x.RuralGovName,
            groupSumSeletor:(x)=>x.Summa);
         
    }

    // Advanced Search
    private Guid _ruralGovId;
    private Guid SearchRuralGovId {
        get => _ruralGovId;
        set
        {
            _ruralGovId= value;
            _ = _table.ReloadDataAsync();
        }
    }

    private DateRange _dateRange = null; // new DateRange(DateTime.Now.Date, DateTime.Now.AddDays(5).Date);
    private MudDateRangePicker _searchPicker;
    private DateRange SearchDateRange
    {
        get => _dateRange;
        set
        {
            _dateRange = value;
            _ = _table.ReloadDataAsync();
        }
    }
    private Months _searchMonth;
    private Months SearchMonth
    {
        get => _searchMonth;
        set
        {
            _searchMonth= value;
            _ = _table.ReloadDataAsync();
        }
    }
        
    
    private Guid _searchYearId;
    private Guid SearchYearId
    {
        get => _searchYearId;
        set
        {
            _searchYearId = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private Guid _searchNativeId;
    private Guid SearchNativeId
    {
        get => _searchNativeId;
        set
        {
            _searchNativeId = value;
            _ = _table.ReloadDataAsync();
        }
    }


}
