using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using static MudBlazor.CategoryTypes;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class TotalByNatives
{

    [Inject]
    protected IYearsClient YearsClient { get; set; } = default!;
    [Inject]
    protected ITotalsClient TotalsClient { get; set; } = default!;
    protected EntityServerTableContext<TotalByNative, Guid, TotalByNative> Context { get; set; } = default!;

    private EntityTableWithGrouping<TotalByNative, Guid, TotalByNative> _table = default!;

    protected override async void OnInitialized()
    {
        Context = new(
            entityName: L["Contribution"],
            entityNamePlural: L["Contributions"],
            entityResource: FSHResource.Contributions,
            fields: new()
            {

                new(prod => prod.Fio, L["FIO"], "Fio"),
                new(prod => prod.Village, L["Village"], "Village"),
                new(prod => prod.Year, L["Year"], "Year"),
                new(prod => prod.AllSumm, L["Total"], "AllSumm", Type : typeof(decimal?)),
                new(prod => prod.January, L["January"], "January",Type: typeof(decimal?)),
                new(prod => prod.February, L["February"], "February", Type: typeof(decimal?)),
                new(prod => prod.March, L["March"], "March",Type: typeof(decimal?)),
                new(prod => prod.April, L["April"], "April",Type: typeof(decimal?)),
                new(prod => prod.May, L["May"], "May",Type: typeof(decimal?)),
                new(prod => prod.June, L["June"], "June",Type: typeof(decimal?)),
                new(prod => prod.July, L["July"], "July", Type : typeof(decimal?)),
                new(prod => prod.August, L["August"], "August", Type : typeof(decimal?)),
                new(prod => prod.September, L["September"], "September", Type : typeof(decimal?)),
                new(prod => prod.October, L["October"], "October", Type : typeof(decimal?)),
                new(prod => prod.November, L["November"], "November", Type : typeof(decimal?)),
                new(prod => prod.December, L["December"], "December", Type : typeof(decimal?)),
                
               // new(prod =>L.GetString(prod.Month.ToString()), L["Month"], "Month"),
                //new(prod => prod.Summa, L["Summa"], "Summa")
            },
            enableAdvancedSearch: false,
            rowStyle: r => r.Style,
            searchFunc: async filter =>
            {
                var contributionFilter = filter.Adapt<GetTotalReportByNativesRequest>();

                contributionFilter.YearId = SearchYearId == default ? null : SearchYearId;
                contributionFilter.RuralGovId = SearchRuralGovId == default ? null : SearchRuralGovId;
                var result = await TotalsClient.GetTotalByNativeAsync(contributionFilter);

                return result.Adapt<PaginationResponse<TotalByNative>>();
            },
            GroupSeletor: (x) => x.RuralGovName,
            groupSumSeletor: (x) => x.AllSumm,

            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportTotalByNativesRequest>();

                exportFilter.YearId = SearchYearId == default ? null : SearchYearId;


                return await TotalsClient.ExportAsync(exportFilter);
            }

            );


    }

    private Guid _ruralGovId;
    private Guid SearchRuralGovId
    {
        get => _ruralGovId;
        set
        {
            _ruralGovId = value;
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
}
