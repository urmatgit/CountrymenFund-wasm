using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;

namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

/// <summary>
/// Initialization Context for the EntityTable Component.
/// Use this one if you want to use Server Paging, Sorting and Filtering.
/// </summary>
public class EntityServerTableContext<TEntity, TId, TRequest>
    : EntityTableGroupingContext<TEntity, TId, TRequest>
{
    /// <summary>
    /// A function that loads the specified page from the api with the specified search criteria
    /// and returns a PaginatedResult of TEntity.
    /// </summary>
    public Func<PaginationFilter, Task<PaginationResponse<TEntity>>> SearchFunc { get; }

    /// <summary>
    /// A function that exports the specified data from the API.
    /// </summary>
    public Func<BaseFilter, Task<FileResponse>>? ExportFunc { get; }

    /// <summary>
    /// A function that import the specified data from the API.
    /// </summary>
    public Func<ImportRequestDto, Task>? ImportFunc { get; }

    public bool EnableAdvancedSearch { get; }

    public EntityServerTableContext(
        List<EntityField<TEntity>> fields,
        Func<PaginationFilter, Task<PaginationResponse<TEntity>>> searchFunc,
        Func<BaseFilter, Task<FileResponse>>? exportFunc = null,
          Func<ImportRequestDto, Task>? importFunc = null,
        bool enableAdvancedSearch = false,
        Func<TEntity, TId>? idFunc = null,
        Func<TEntity, string>? rowStyle = null,
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
        string? importAction=null,
        Func<Task>? editFormInitializedFunc = null,
          Func<Task>? importFormInitializedFunc = null,
        Func<bool>? hasExtraActionsFunc = null,
        Func<TEntity, bool>? canUpdateEntityFunc = null,
        Func<TEntity, bool>? canDeleteEntityFunc = null,
        
        Func<TEntity, object>? GroupSeletor = null,
        Func<TEntity, decimal>? groupSumSeletor = null)
        : base(
            fields,
            idFunc,
            rowStyle,
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
            importAction,
            editFormInitializedFunc,
            importFormInitializedFunc,
            hasExtraActionsFunc,
            canUpdateEntityFunc,
            canDeleteEntityFunc,
            
            GroupSeletor,
            groupSumSeletor)
    {
        SearchFunc = searchFunc;
        ExportFunc = exportFunc;
        ImportFunc = importFunc;
        EnableAdvancedSearch = enableAdvancedSearch;
    }
}