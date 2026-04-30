using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.FilePond.Dtos;

namespace Soenneker.Blazor.FilePond;

internal sealed record ServerProcessRegistration<T>(Func<T, CancellationToken, ValueTask<T>> LoadHandler, Func<T, CancellationToken, ValueTask<T>> RemoveHandler, CancellationToken CancellationToken) where T : class;
