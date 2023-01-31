using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

public class EntityTableGroupingContext<TEntity, TId, TRequest>
    : EntityTableContext<TEntity, TId, TRequest>
{
    public TableGroupDefinition<TEntity> _groupDefinition = default!;
    
    public EntityTableGroupingContext(
        List<EntityField<TEntity>> fields,
        Func<TEntity, TId>? idFunc = null,
        Func<Task<TRequest>>? getDefaultsFunc = null,
        Func<TRequest, Task>? createFunc = null,
        Func<TId, Task<TRequest>>? getDetailsFunc = null,
        Func<TId, TRequest, Task>? updateFunc = null,
        Func<TId, Task>? deleteFunc = null,
        string? entityName = null,
        string? entityNamePlural = null,
        string? entityResource = null,
        string? searchAction = null,
        string? createAction = null,
        string? updateAction = null,
        string? deleteAction = null,
        string? exportAction = null,
        Func<Task>? editFormInitializedFunc = null,
        Func<bool>? hasExtraActionsFunc = null,
        Func<TEntity, bool>? canUpdateEntityFunc = null,
        Func<TEntity, bool>? canDeleteEntityFunc = null,
        
        Func<TEntity, object>? GroupSeletor=null)
        : base(
            fields,
            idFunc,
            getDefaultsFunc,
            createFunc,
            getDetailsFunc,
            updateFunc,
            deleteFunc,
            entityName,
            entityNamePlural,
            entityResource,
            searchAction,
            createAction,
            updateAction,
            deleteAction,
            exportAction,
            editFormInitializedFunc,
            hasExtraActionsFunc,
            canUpdateEntityFunc,
            canDeleteEntityFunc)
    {
     
          _groupDefinition = new()
        {
            GroupName = "",
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = true,
            Selector = GroupSeletor
        };
}
}
