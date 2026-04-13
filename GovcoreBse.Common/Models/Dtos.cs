using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common.Models;

public class UserDto
{

    public int Id { get; set; }


    public string UserId { get; set; } = null!;


    public string UserName { get; set; } = null!;


    public string? Person { get; set; }


    public string EncPassword { get; set; } = null!;

    public bool Disabled { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsReset { get; set; }


    public DateTime? loginedAt { get; set; }


    public DateTime updatedAt { get; set; }


    public string updatedBy { get; set; } = null!;


    public DateTime? createdAt { get; set; }


    public int level { get; set; }


    public string? post { get; set; }


    public string? tel { get; set; }


    public string? email { get; set; }

    public string? Division { get; set; }

    public string? AdminScope { get; set; }
}

public class WeatherForecastDto
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; set; } = string.Empty;
}

public class FileItemDto : IN.IDocItem
{
    public long Id { get; set; }
    public string DocType { get; set; }
    public string UploadFilePath { get; set; }
    public string RelativePath { get; set; }
}