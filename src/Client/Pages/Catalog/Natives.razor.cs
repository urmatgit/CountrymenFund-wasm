﻿using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Natives
{
    [Inject]
    protected INativesClient NativesClient { get; set; } = default!;
    [Inject]
    protected IRuralGovsClient RuralGovsClient { get; set; } = default!;

    protected EntityServerTableContext<NativeDto, Guid, NativeViewModel> Context { get; set; } = default!;

    private EntityTable<NativeDto, Guid, NativeViewModel> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Native"],
            entityNamePlural: L["Natives"],
            entityResource: FSHResource.Natives,
            fields: new()
            {
               // new(prod => prod.Id, L["Id"], "Id"),
                new(prod => prod.Name, L["Name"], "Name"),
                new(prod => prod.Name, L["Surname"], "Surname"),
                new(prod => prod.Name, L["MiddleName"], "MiddleName"),
                new(prod => prod.RuralGovName, L["RuralGov"], "RuralGovName"),
                new(prod => prod.Description, L["Description"], "Description"),
                new(prod => prod.Rate, L["Rate"], "Rate")
            },
            enableAdvancedSearch: true,
            idFunc: prod => prod.Id,
            searchFunc: async filter =>
            {
                var productFilter = filter.Adapt<SearchNativesRequest>();

                productFilter.RuralGovId = SearchRuralGovId == default ? null : SearchRuralGovId;
                productFilter.MinimumRate = SearchMinimumRate;
                productFilter.MaximumRate = SearchMaximumRate;

                var result = await NativesClient.SearchAsync(productFilter);
                return result.Adapt<PaginationResponse<NativeDto>>();
            },
            createFunc: async prod =>
            {
                if (!string.IsNullOrEmpty(prod.ImageInBytes))
                {
                    prod.Image = new FileUploadRequest() { Data = prod.ImageInBytes, Extension = prod.ImageExtension ?? string.Empty, Name = $"{prod.Name}_{Guid.NewGuid():N}" };
                }

                await NativesClient.CreateAsync(prod.Adapt<CreateNativeRequest>());
                prod.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, prod) =>
            {
                if (!string.IsNullOrEmpty(prod.ImageInBytes))
                {
                    prod.DeleteCurrentImage = true;
                    prod.Image = new FileUploadRequest() { Data = prod.ImageInBytes, Extension = prod.ImageExtension ?? string.Empty, Name = $"{prod.Name}_{Guid.NewGuid():N}" };
                }

                await NativesClient.UpdateAsync(id, prod.Adapt<UpdateNativeRequest>());
                prod.ImageInBytes = string.Empty;
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportNativesRequest>();

                exportFilter.RuralGovId = SearchRuralGovId == default ? null : SearchRuralGovId;
                exportFilter.MinimumRate = SearchMinimumRate;
                exportFilter.MaximumRate = SearchMaximumRate;

                return await NativesClient.ExportAsync(exportFilter);
            },
            deleteFunc: async id => await NativesClient.DeleteAsync(id));

    // Advanced Search

    private Guid _searchRuralGovId;
    private Guid SearchRuralGovId
    {
        get => _searchRuralGovId;
        set
        {
            _searchRuralGovId = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private decimal _searchMinimumRate;
    private decimal SearchMinimumRate
    {
        get => _searchMinimumRate;
        set
        {
            _searchMinimumRate = value;
            _ = _table.ReloadDataAsync();
        }
    }

    private decimal _searchMaximumRate = 9999;
    private decimal SearchMaximumRate
    {
        get => _searchMaximumRate;
        set
        {
            _searchMaximumRate = value;
            _ = _table.ReloadDataAsync();
        }
    }

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"
    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            Context.AddEditModal.RequestModel.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    public void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    public void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImage = true;
        Context.AddEditModal.ForceRender();
    }
}

public class NativeViewModel : UpdateNativeRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}