using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using System;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class RuralGovs
{
    [Inject]
    protected IRuralGovsClient RuralGovsClient { get; set; } = default!;

    protected EntityServerTableContext<RuralGovDto, Guid, UpdateRuralGovRequest> Context { get; set; } = default!;
    protected override void OnInitialized()
    {

        base.OnInitialized();
        Context = new EntityServerTableContext<RuralGovDto, Guid, UpdateRuralGovRequest>(
            entityName: L["Rural government"],
            entityNamePlural: L["Rural governments"],
            entityResource: FSHResource.RuralGovs,
            fields: new List<EntityField<RuralGovDto>>()
                {
                new EntityField<RuralGovDto>(rural=>rural.Id,L["Id"],"Id"),
                new EntityField<RuralGovDto>(rural=>rural.Name,L["Name"],"Name"),
                new EntityField<RuralGovDto>(rural=>rural.Coordinate,L["Coordinate"],"Coordinate"),
                new EntityField<RuralGovDto>(rural=>rural.Description,L["Description"],"Description"),
                },
            idFunc: ruralgov => ruralgov.Id,
            searchFunc: async filter => (await RuralGovsClient
                .SearchAsync(filter.Adapt<SearchRuralGovsRequest>()))
                .Adapt<PaginationResponse<RuralGovDto>>(),
            createFunc: async ruralgov => await RuralGovsClient.CreateAsync(ruralgov.Adapt<CreateRuralGovRequest>()),
            updateFunc: async (id, ruralgov) => await RuralGovsClient.UpdateAsync(id, ruralgov),
            deleteFunc: async id => await RuralGovsClient.DeleteAsync(id),
            exportAction: string.Empty);
    }
}
