namespace GovcoreBse.Models;

public class LayoutStateModel
{
    public bool SideBarOpen { get; set; }

    public string ReferrerUrl { get; set; }
    public string CurrentUrl { get; set; }
    public event Action? OnLayoutChange;
    
    public void UpdateState(string  referrerUrl,string currentUrl)
    {
        ReferrerUrl = referrerUrl;
        CurrentUrl = currentUrl;

        OnLayoutChange?.Invoke();
    }
}
