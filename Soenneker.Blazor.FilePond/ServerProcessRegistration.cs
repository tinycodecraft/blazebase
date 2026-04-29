using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.FilePond.Dtos;

namespace Soenneker.Blazor.FilePond;

internal sealed record ServerProcessRegistration<T>(Func<T, CancellationToken, ValueTask<string>> Handler, CancellationToken CancellationToken) where T : class;
