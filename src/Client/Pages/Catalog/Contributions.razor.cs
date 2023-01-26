using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Contributions
{
    [Inject]
    protected IStringLocalizer<Contributions> L { get; set; }
    [Inject]
    protected IContributionsClient ContributionsClient { get; set; } = default!;
    [Inject]
    protected IYearsClient YearsClient { get; set; } = default!;
    [Inject]
    protected INativesClient NativesClient { get; set; } = default!;

    protected EntityServerTableContext<ContributionDto, Guid, UpdateContributionRequest> Context { get; set; } = default!;

    private EntityTable<ContributionDto, Guid, UpdateContributionRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Contribution"],
            entityNamePlural: L["Contributions"],
            entityResource: FSHResource.Contributions,
            fields: new()
            {
                new(prod => prod.Id, L["Id"], "Id"),
                new(prod => prod.Year, L["Year"], "Year"),
                new(prod => prod.NativeFIO, L["FIO"], "NativeFIO"),
                new(prod =>SH.GetString(prod.Month.ToString()), L["Month"], "Month"),
                new(prod => prod.Summa, L["Summa"], "Summa"),
                new(prod => prod.Date, L["Date"], "Date"),
                new(prod => prod.Description, L["Description"], "Description"),
                new(prod => prod.Rate, L["Rate"], "Rate")
            },
            enableAdvancedSearch: true,
            idFunc: prod => prod.Id,
            searchFunc: async filter =>
            {
                var contributionFilter = filter.Adapt<SearchContributionsRequest>();

                contributionFilter.YearId = SearchYearId == default ? null : SearchYearId;
                

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
            //exportFunc: async filter =>
            //{
            //    var exportFilter = filter.Adapt<ExportContributionsRequest>();

            //    exportFilter.YearId = SearchYearId == default ? null : SearchYearId;
             

            //    return await ContributionsClient.ExportAsync(exportFilter);
            //},
            deleteFunc: async id => await ContributionsClient.DeleteAsync(id));

    // Advanced Search

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
