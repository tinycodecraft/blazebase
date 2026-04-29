using GovcoreBse.Common;
using GovcoreBse.Common.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GovcoreBse.Common.Interfaces;

namespace GovcoreBse.Control;


/// <inheritdoc cref="ICoreComponent"/>
public abstract class CoreComponent : ComponentBase, ICoreComponent
{
    protected ValueAtomicBool Disposed;

    [Parameter]
    public virtual string? Id { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? Attributes { get; set; }

    protected virtual void OnDispose()
    {
    }

    protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;

    public virtual void Dispose()
    {
        if (!Disposed.TrySetTrue())
            return;

        OnDispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (!Disposed.TrySetTrue())
            return;

        await OnDisposeAsync()
            .NoSync();

        OnDispose();
    }
}