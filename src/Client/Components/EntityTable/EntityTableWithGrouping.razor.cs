using FSH.BlazorWebAssembly.Client.Components.Dialogs;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Auth;
using FSH.BlazorWebAssembly.Client.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

public partial class EntityTableWithGrouping<TEntity, TId, TRequest>
    where TRequest : new()
{
    [Parameter]
    [EditorRequired]
    public EntityTableGroupingContext<TEntity, TId, TRequest> Context { get; set; } = default!;

    

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public string? SearchString { get; set; }
    [Parameter]
    public EventCallback<string> SearchStringChanged { get; set; }

    [Parameter]
    public RenderFragment? AdvancedSearchContent { get; set; }

    [Parameter]
    public RenderFragment<TEntity>? ActionsContent { get; set; }
    [Parameter]
    public RenderFragment<TEntity>? ExtraActions { get; set; }
    [Parameter]
    public RenderFragment<TEntity>? ChildRowContent { get; set; }

    [Parameter]
    public RenderFragment<TRequest>? EditFormContent { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private bool _canSearch;
    private bool _canCreate;
    private bool _canUpdate;
    private bool _canDelete;
    private bool _canExport;
    private bool _canImport;

    private bool _advancedSearchExpanded = false;
    [Parameter]
    public bool AdvancedSearchExpanded
    {
        get => _advancedSearchExpanded;
        set
        {
            _advancedSearchExpanded = value;
        }
    }
    [Parameter]
    public bool VisibleSearchText { get; set; } = true;
    private MudTable<TEntity> _table = default!;
    private IEnumerable<TEntity>? _entityList;
    private int _totalItems;

    private string getZeroVisible(EntityField<TEntity> field, TEntity entity)
    {
        if (field == null || entity == null) return "";
        if (field.Type == typeof(decimal?) && Convert.ToDecimal(field.ValueFunc(entity)) == 0M)
        {
            return "";
        }
        return field!.ValueFunc(entity)!.ToString();

    }

    private string getStyle(EntityField<TEntity> field, TEntity entity)
    {

        if (field.Type == typeof(decimal?))
            return "text-align:right;" + (Context.RowStyleFunc != null ? Context.RowStyleFunc(entity) : "");
        else
            return "text-align:left;" + (Context.RowStyleFunc != null ? Context.RowStyleFunc(entity) : "");
    }
    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canSearch = await CanDoActionAsync(Context.SearchAction, state);
        _canCreate = await CanDoActionAsync(Context.CreateAction, state) && Context.CreateFunc != null; ;
        _canUpdate = await CanDoActionAsync(Context.UpdateAction, state) && Context.CreateFunc != null; ;
        _canDelete = await CanDoActionAsync(Context.DeleteAction, state) && Context.CreateFunc != null; ;
        _canExport = await CanDoActionAsync(Context.ExportAction, state);
        _canImport = Context.ImportAction != null && await CanDoActionAsync(Context.ImportAction, state);
        await LocalLoadDataAsync();
    }

    public Task ReloadDataAsync() =>
        Context.IsClientContext
            ? LocalLoadDataAsync()
            : ServerLoadDataAsync();

    private async Task<bool> CanDoActionAsync(string? action, AuthenticationState state) =>
        !string.IsNullOrWhiteSpace(action) &&
            ((bool.TryParse(action, out bool isTrue) && isTrue) || // check if action equals "True", then it's allowed
            (Context.EntityResource is { } resource && await AuthService.HasPermissionAsync(state.User, action, resource)));

    private bool HasActions => _canUpdate || _canDelete || (Context.HasExtraActionsFunc is not null && Context.HasExtraActionsFunc());
    private bool CanUpdateEntity(TEntity entity) => _canUpdate && (Context.CanUpdateEntityFunc is null || Context.CanUpdateEntityFunc(entity));
    private bool CanDeleteEntity(TEntity entity) => _canDelete && (Context.CanDeleteEntityFunc is null || Context.CanDeleteEntityFunc(entity));

    // Client side paging/filtering
    private bool LocalSearch(TEntity entity) =>
        Context.ClientContext?.SearchFunc is { } searchFunc
            ? searchFunc(SearchString, entity)
            : string.IsNullOrWhiteSpace(SearchString);

    private async Task LocalLoadDataAsync()
    {
        if (Loading || Context.ClientContext is null)
        {
            return;
        }

        Loading = true;

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Context.ClientContext.LoadDataFunc(), Snackbar)
            is List<TEntity> result)
        {
            _entityList = result;
        }

        Loading = false;
    }

    // Server Side paging/filtering

    private async Task OnSearchStringChanged(string? text = null)
    {
        await SearchStringChanged.InvokeAsync(SearchString);

        await ServerLoadDataAsync();
    }

    private async Task ServerLoadDataAsync()
    {
        if (Context.IsServerContext)
        {
            await _table.ReloadServerData();
        }
    }

    private Func<TableState, Task<TableData<TEntity>>>? ServerReloadFunc =>
        Context.IsServerContext ? ServerReload : null;

    private async Task<TableData<TEntity>> ServerReload(TableState state)
    {
        if (!Loading && Context.ServerContext is not null)
        {
            Loading = true;

            var filter = GetPaginationFilter(state);

            if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => Context.ServerContext.SearchFunc(filter), Snackbar)
                is { } result)
            {
                _totalItems = result.TotalCount;
                _entityList = result.Data;
            }

            Loading = false;
        }

        return new TableData<TEntity> { TotalItems = _totalItems, Items = _entityList };
    }

    private async Task ExportAsync()
    {
        if (!Loading && Context.ServerContext is not null)
        {
            if (Context.ServerContext.ExportFunc is not null)
            {
                Loading = true;

                var filter = GetBaseFilter();

                if (await ApiHelper.ExecuteCallGuardedAsync(
                        () => Context.ServerContext.ExportFunc(filter), Snackbar)
                    is { } result)
                {
                    using var streamRef = new DotNetStreamReference(result.Stream);
                    await JS.InvokeVoidAsync("downloadFileFromStream", $"{Context.EntityNamePlural}.xlsx", streamRef);
                }

                Loading = false;
            }
        }
    }

    private PaginationFilter GetPaginationFilter(TableState state)
    {
        string[]? orderings = null;
        if (!string.IsNullOrEmpty(state.SortLabel))
        {
            orderings = state.SortDirection == SortDirection.None
                ? new[] { $"{state.SortLabel}" }
                : new[] { $"{state.SortLabel} {state.SortDirection}" };
        }

        var filter = new PaginationFilter
        {
            PageSize = state.PageSize,
            PageNumber = state.Page + 1,
            Keyword = SearchString,
            OrderBy = orderings ?? Array.Empty<string>()
        };

        if (!Context.AllColumnsChecked)
        {
            filter.AdvancedSearch = new()
            {
                Fields = Context.SearchFields,
                Keyword = filter.Keyword
            };
            filter.Keyword = null;
        }

        return filter;
    }

    private BaseFilter GetBaseFilter()
    {
        var filter = new BaseFilter
        {
            Keyword = SearchString,
        };

        if (!Context.AllColumnsChecked)
        {
            filter.AdvancedSearch = new()
            {
                Fields = Context.SearchFields,
                Keyword = filter.Keyword
            };
            filter.Keyword = null;
        }

        return filter;
    }

    private async Task InvokeModal(TEntity? entity = default)
    {
        bool isCreate = entity is null;

        var parameters = new DialogParameters()
        {
            { nameof(AddEditModal<TRequest>.ChildContent), EditFormContent },
            { nameof(AddEditModal<TRequest>.OnInitializedFunc), Context.EditFormInitializedFunc },
            { nameof(AddEditModal<TRequest>.IsCreate), isCreate }
        };

        Func<TRequest, Task> saveFunc;
        TRequest requestModel;
        string title, successMessage;

        if (isCreate)
        {
            _ = Context.CreateFunc ?? throw new InvalidOperationException("CreateFunc can't be null!");

            saveFunc = Context.CreateFunc;

            requestModel =
                Context.GetDefaultsFunc is not null
                    && await ApiHelper.ExecuteCallGuardedAsync(
                            () => Context.GetDefaultsFunc(), Snackbar)
                        is { } defaultsResult
                ? defaultsResult
                : new TRequest();

            title = $"{L["Create"]} {Context.EntityName}";
            successMessage = $"{Context.EntityName} {L["Created"]}";
        }
        else
        {
            _ = Context.IdFunc ?? throw new InvalidOperationException("IdFunc can't be null!");
            _ = Context.UpdateFunc ?? throw new InvalidOperationException("UpdateFunc can't be null!");

            var id = Context.IdFunc(entity!);

            saveFunc = request => Context.UpdateFunc(id, request);

            requestModel =
                Context.GetDetailsFunc is not null
                    && await ApiHelper.ExecuteCallGuardedAsync(
                            () => Context.GetDetailsFunc(id!),
                            Snackbar)
                        is { } detailsResult
                ? detailsResult
                : entity!.Adapt<TRequest>();

            title = $"{L["Edit"]} {Context.EntityName}";
            successMessage = $"{Context.EntityName} {L["Updated"]}";
        }

        parameters.Add(nameof(AddEditModal<TRequest>.SaveFunc), saveFunc);
        parameters.Add(nameof(AddEditModal<TRequest>.RequestModel), requestModel);
        parameters.Add(nameof(AddEditModal<TRequest>.Title), title);
        parameters.Add(nameof(AddEditModal<TRequest>.SuccessMessage), successMessage);

        var dialog = DialogService.ShowModal<AddEditModal<TRequest>>(parameters);

        Context.SetAddEditModalRef(dialog);

        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            await ReloadDataAsync();
        }
    }

    private async Task Delete(TEntity entity)
    {
        _ = Context.IdFunc ?? throw new InvalidOperationException("IdFunc can't be null!");
        TId id = Context.IdFunc(entity);

        string deleteContent = L["You're sure you want to delete {0} with id '{1}'?"];
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), string.Format(deleteContent, Context.EntityName, id) }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(L["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            _ = Context.DeleteFunc ?? throw new InvalidOperationException("DeleteFunc can't be null!");

            await ApiHelper.ExecuteCallGuardedAsync(
                () => Context.DeleteFunc(id),
                Snackbar, successMessage: L["Deleted"]);

            await ReloadDataAsync();
        }
    }
    // developing
    private async Task ImportAsync(FileUploadRequest request)
    {
        Loading = true;
        try
        {
            if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => Context.ServerContext.ImportFunc(request), Snackbar)
                is { } result)
            {

            }
        }finally { Loading = false; }
        
    }

    private async Task InvokeImportModal()
    {
        var parameters = new DialogParameters
            {
                { nameof(ImportModal.ModelName), Context.EntityName },
                { nameof(ImportModal.OnInitializedFunc), Context.ImportFormInitializedFunc },
            };

        Func<FileUploadRequest, Task> importFunc = ImportAsync;

        parameters.Add(nameof(ImportModal.ImportFunc), importFunc);
        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };

        var dialog = DialogService.Show<ImportModal>(@L["Import"], parameters, options);

        Context.SetImportModalRef(dialog);

        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await ReloadDataAsync();
        }
    }
}