namespace GovcoreBse.Models;

public class LayoutStateModel
{
    public string CurrentCulture { get; set;  }
    public bool IsLogin { get; set;  }
    public bool SideBarOpen { get; set; }

    public string UserName { get; set; } = string.Empty;

    public bool IsAdmin { get; set;  }

    public string Post { get; set; } = string.Empty;

    
}
