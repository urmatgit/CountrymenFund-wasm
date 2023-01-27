using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
public partial class YearDto
{
    public YearDto()
    {
        this.Year = DateTime.Now.Year;
    }
}
