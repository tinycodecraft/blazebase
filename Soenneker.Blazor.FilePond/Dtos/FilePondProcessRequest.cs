using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FN=GovcoreBse.Common.Adapt.Interfaces;

namespace Soenneker.Blazor.FilePond.Dtos;

public class FilePondLoadRequest : FN.IFilePondLoadRequest
{
    public string[] InUrls { get; set; } = Array.Empty<string>();
    public string[] OutUrls { get; set; } = Array.Empty<string>();
    public string Source { get; set; } = string.Empty;
}