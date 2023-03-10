using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient.Fund;
public class SlideViewModel: SlideDto
{
    //public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}
