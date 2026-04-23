using Microsoft.AspNetCore.Components;

namespace Soenneker.Quark;

///<inheritdoc cref="ICoreElement"/>
public abstract class CoreElement : CoreComponent, ICoreElement
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
