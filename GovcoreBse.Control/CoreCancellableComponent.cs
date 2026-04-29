using GovcoreBse.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GovcoreBse.Common.Interfaces;

namespace GovcoreBse.Control;

///<inheritdoc cref="ICoreCancellableComponent"/>
public abstract class CoreCancellableComponent : CoreComponent, ICoreCancellableComponent
{
    public CancellationToken CancellationToken =>
        Disposed.Value
            ? CancellationToken.None
            : _cancellationTokenSource.TryGet()
                                      ?.Token ?? CancellationToken.None;

    private readonly AtomicResource<CancellationTokenSource> _cancellationTokenSource;

    protected CoreCancellableComponent() : this(CancellationToken.None)
    {
    }

    /// <summary>
    /// Optionally link to an external token so parent cancellation flows into this component.
    /// </summary>
    protected CoreCancellableComponent(CancellationToken linkedToken)
    {
        _cancellationTokenSource = new AtomicResource<CancellationTokenSource>(
            factory: () => linkedToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(linkedToken) : new CancellationTokenSource(),
            teardown: async cts =>
            {
                try
                {
                    await cts.CancelAsync();
                }
                catch
                {
                    /* ignore */
                }

                cts.Dispose();
            });
    }

    public Task Cancel()
    {
        CancellationTokenSource? cts = _cancellationTokenSource.TryGet();
        return cts is null ? Task.CompletedTask : cts.CancelAsync();
    }

    public ValueTask ResetCancellation() => _cancellationTokenSource.Reset();

    public override async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.DisposeAsync();

        await base.DisposeAsync();
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Dispose();

        base.Dispose();
    }
}