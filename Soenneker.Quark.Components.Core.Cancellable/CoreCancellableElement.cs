using Microsoft.AspNetCore.Components;

namespace Soenneker.Quark;

///<inheritdoc cref="ICoreCancellableElement"/>
public abstract class CoreCancellableElement : CoreCancellableComponent, ICoreCancellableElement
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
