using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class FSContributions
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; }
    
    [Inject]
    protected IFSContributionsClient FSContributionsClient { get; set; } = default!;
    
    [Inject]
    protected IFinSupportsClient FinSupportsClient { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();
    protected EntityServerTableContext<FSContributionDto, Guid, UpdateFSContributionRequest> Context { get; set; } = default!;

    private EntityTableWithGrouping<FSContributionDto, Guid, UpdateFSContributionRequest> _table = default!;
    protected override void OnInitialized()
    {
        Context = new(
            entityName: SH["Financial support contribution"],
            entityNamePlural: SH["Financial support contribution"],
            entityResource: FSHResource.Contributions,
            fields: new()
            {
                new(prod => prod.Id,SH["Id"], "Id"),
                new(prod => prod.FinSuportName, SH["Financial supports"], "FinSuportName"),
                new(prod => prod.NativeFIO, SH["FIO"], "NativeFIO"),
                new(prod => prod.Summa, SH["Summa"], "Summa",Type: typeof(decimal?)),
                new(prod => prod.Date, SH["Date"], "Date"),
                new(prod => prod.Description, SH["Description"], "Description"),

            },
            enableAdvancedSearch: false,
            idFunc: prod => prod.Id,
            searchFunc: async filter =>
            {
                var contributionFilter = filter.Adapt<SearchFSContributionsRequest>();

                contributionFilter.FinSupportId = FinSupportId == default ? null : FinSupportId;
                contributionFilter.NativeId = SearchNativeId == default ? null : SearchNativeId;
                
                var result = await FSContributionsClient.SearchAsync(contributionFilter);
                return result.Adapt<PaginationResponse<FSContributionDto>>();
            },
            createFunc: async prod =>
            {


                await FSContributionsClient.CreateAsync(prod.Adapt<CreateFSContributionRequest>());

            },
            updateFunc: async (id, prod) =>
            {

                await FSContributionsClient.UpdateAsync(id, prod.Adapt<UpdateFSContributionRequest>());

            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportFSContributionsRequest>();

                exportFilter.FinSupportId = FinSupportId == default ? null : FinSupportId;


                exportFilter.NativeId = SearchNativeId == default ? null : SearchNativeId;
                ;
                return await FSContributionsClient.ExportAsync(exportFilter);
            },
            deleteFunc: async id => await FSContributionsClient.DeleteAsync(id)
            , getDefaultsFunc: async () =>
            {

                var yearDefault = await FSContributionsClient.GetDefaultAsync();
                Console.WriteLine($"Default year is {yearDefault.Id}");
                return new UpdateFSContributionRequest()
                {
                    
                    Date = yearDefault.Date,
                    
                };
            },

            GroupSeletor: (x) => x.FinSuportName,
            groupSumSeletor: (x) => x.Summa);

    }

    

   
     


    private Guid _finSupportId;
    private Guid FinSupportId
    {
        get => _finSupportId;
        set
        {
            _finSupportId = value;
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
    protected async Task Finded(bool finded)
    {

        var result = await JsRuntime.InvokeAsync<string>("ShowElement", "addNative", !finded);
        StateHasChanged();
    }
    private void goNativeAdd()
    {
        Navigation.NavigateTo($"/catalog/natives/{Guid.Empty}");
    }
}
