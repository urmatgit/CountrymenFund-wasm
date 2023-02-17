using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography.X509Certificates;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class TotalByRuralGovs
{
    [Inject]
    protected IYearsClient YearsClient { get; set; } = default!;
    [Inject]
    protected ITotalsClient TotalsClient { get; set; } = default!;
    protected EntityServerTableContext<TotalWithMonths, Guid, TotalWithMonths> Context { get; set; } = default!;

    private EntityTable<TotalWithMonths, Guid, TotalWithMonths> _table = default!;
    protected override async void OnInitialized()
    {
        Context = new(
            entityName: L["Contribution"],
            entityNamePlural: L["Contributions"],
            entityResource: FSHResource.Contributions,
            fields: new()
            {

                new(prod => prod.RuralGovName, L["RuralGov"], "RuralGovName"),
                new(prod => prod.Year, L["Year"], "Year"),
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
            rowStyle: r=>r.Style,
            searchFunc: async filter =>
            {
                var contributionFilter = filter.Adapt<GetStateByRuralGovRequest>();

                contributionFilter.YearId = SearchYearId == default ? null : SearchYearId;

                var result = await TotalsClient.GetTotalRuralgovAsync(contributionFilter);

                return result.Adapt<PaginationResponse<TotalWithMonths>>();
            }
             

            //exportFunc: async filter =>
            //{
            //    var exportFilter = filter.Adapt<ExportContributionsRequest>();

            //    exportFilter.YearId = SearchYearId == default ? null : SearchYearId;


            //    return await ContributionsClient.ExportAsync(exportFilter);
            //},

            );
        

    }
    //protected override async Task OnAfterRenderAsync(bool firstRender)
    ////{
    ////    var year = await YearsClient.Get2Async(DateTime.Now.Year);
    ////    if (year is not null)
    ////        SearchYearId = year.Id;
        
    ////    Console.WriteLine($"Inin year id {SearchYearId}");
       
   // }
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
