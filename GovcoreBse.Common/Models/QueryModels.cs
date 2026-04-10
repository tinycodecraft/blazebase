using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static GovcoreBse.Common.Interfaces;


namespace GovcoreBse.Common.Models;


public class UrlModel : IUrlModel
{
    public string UrlTitle { get; set; }

    public int MaxCount { get; set; }

    public string BaseUrl { get; set; }

    public string BaseUrlByName { get; set; }
    public IUrl[] Urls { get; set; }

    public int InitStart { get; set; }

    public IUrlModel ExtractModel(int init, int len)
    {

        return new UrlModel
        {
            Urls = this.Urls,
            BaseUrl = this.BaseUrl,
            InitStart = init,
            MaxCount = len,
            UrlTitle = this.UrlTitle
        };
    }

}

public class UrlItem : IUrl
{
    public long Size { get; set; }
    public bool CanLoad { get; set; }
    public string Caption { get; set; }

    public string Url { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }

    public long Thumb { get; set; }
}

//using in react mantine table
public class SortProps
{
    public bool Desc { get; set; }

    public string Id { get; set; }

}
//using in react mantine table
public class FilterProps
{
    public string Id { get; set; }  
    public dynamic Value { get; set; }


}
//using in react mantine table
public class DescProps
{
    public string value { get; set; } = "";
    public string label { get; set; } = "";
}

//using in react mantine table
public class MantineTableProps
{
    public string Type { get; set; } = "";

    //record start index
    public int Start { get; set; }

    //page size
    public int Size { get; set; }

    public FilterProps[] Filtering { get; set; } = [];

    public string GlobalFilter { get; set; }= "";

    public SortProps[] Sorting { get; set; } = [];

    public bool? WithDisabled { get; set; }

    public string? SelectedIds { get; set; }
}

