using Microsoft.AspNetCore.Components;

namespace Soenneker.Quark;

public interface ICoreElement
{
    RenderFragment? ChildContent { get; set; }
}
