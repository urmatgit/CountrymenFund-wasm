using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;

namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

public class ImportRequestDto
{
    public FileUploadRequest fileUploadRequest { get; set; }
    public Guid Value { get; set; }

}
