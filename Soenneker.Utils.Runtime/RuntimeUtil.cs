using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.String;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Atomics.ValueNullableBools;


#if WINDOWS
using Microsoft.Win32;
#endif

namespace Soenneker.Utils.Runtime;

/// <summary>
/// A collection of helpful runtime-based environment and platform detection utilities.
/// </summary>
public static class RuntimeUtil
{
    /// <summary>
    /// Determines whether the current operating system is Windows.
    /// </summary>
    /// <returns>true if the current operating system is Windows; otherwise, false.</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWindows() => OperatingSystem.IsWindows();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMacOs() => OperatingSystem.IsMacOS();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLinux() => OperatingSystem.IsLinux();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAndroid() => OperatingSystem.IsAndroid();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBrowser() => OperatingSystem.IsBrowser();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIos() => OperatingSystem.IsIOS();

    private static ValueAtomicNullableBool _isGitHubAction = new();
    private static ValueAtomicNullableBool _isAzureFunction = new();
    private static ValueAtomicNullableBool _isAzureAppService = new();

    /// <summary>
    /// Gets a value indicating whether the current process is running within a GitHub Actions environment.
    /// </summary>
    /// <remarks>This property checks for the presence of environment variables commonly set by GitHub
    /// Actions, such as "GITHUB_ACTIONS" or "CI". Returns <see langword="true"/> if the process is detected to be
    /// running in a GitHub Actions workflow; otherwise, <see langword="false"/>.</remarks>
    public static bool IsGitHubAction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            bool? cached = _isGitHubAction.Value;
            if (cached is not null)
                return cached.Value;

            string? actionStr = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") ?? Environment.GetEnvironmentVariable("CI");

            bool value = actionStr is not null && actionStr.EqualsIgnoreCase("true");

            _isGitHubAction.TrySet(value);
            return value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current process is running within an Azure Functions environment.
    /// </summary>
    /// <remarks>This property determines the environment by checking for the presence of the
    /// "FUNCTIONS_WORKER_RUNTIME" environment variable. It is useful for conditionally enabling features or behaviors
    /// specific to Azure Functions.</remarks>
    public static bool IsAzureFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            bool? cached = _isAzureFunction.Value;
            if (cached is not null)
                return cached.Value;

            bool value = Environment.GetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME")
                                    .HasContent();

            _isAzureFunction.TrySet(value);
            return value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current process is running within an Azure App Service environment.
    /// </summary>
    /// <remarks>This property determines the hosting environment by checking for specific environment
    /// variables set by Azure App Service. It can be used to conditionally enable or disable features that are specific
    /// to Azure App Service deployments.</remarks>
    public static bool IsAzureAppService
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            bool? cached = _isAzureAppService.Value;
            if (cached is not null)
                return cached.Value;

            bool value = (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")).HasContent();

            _isAzureAppService.TrySet(value);
            return value;
        }
    }

    /// <summary>
    /// Async singleton that determines whether the current process is running inside a container.
    /// The result is cached after the first evaluation.
    /// </summary>
    private static readonly AsyncSingleton<bool> _isContainer = new(DetectIsContainer);

    /// <summary>
    /// Determines whether the current process is running inside a container
    /// (for example Docker or Kubernetes).
    /// </summary>
    [Pure]
    public static ValueTask<bool> IsContainer(CancellationToken cancellationToken = default)
    {
        return _isContainer.Get(cancellationToken);
    }

    private static async ValueTask<bool> DetectIsContainer(CancellationToken cancellationToken = default)
    {
        // Fast-path environment variable hints
        string? inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ??
                              Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINERS");

        if (inContainer is not null && inContainer.EqualsIgnoreCase("true"))
            return true;

        if (OperatingSystem.IsLinux())
        {
            // Docker-specific marker file
            if (File.Exists("/.dockerenv"))
                return true;

            const string cgroupPath = "/proc/1/cgroup";

            if (!File.Exists(cgroupPath))
                return false;

            // Stream line-by-line to avoid allocating the entire file contents
            await using var fs = new FileStream(cgroupPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            using var reader = new StreamReader(fs);

            while (true)
            {
                string? line = await reader.ReadLineAsync(cancellationToken)
                                           .NoSync();
                if (line is null)
                    break;

                if (line.Contains("docker", StringComparison.OrdinalIgnoreCase) || line.Contains("kubepods", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("containerd", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

#if WINDOWS
        if (!OperatingSystem.IsWindows())
            return false;

        // Most Windows containers run as ContainerAdministrator under "User Manager"
        if (Environment.UserName == "ContainerAdministrator" && Environment.UserDomainName == "User Manager")
            return true;

        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control");
            object? val = key?.GetValue("ContainerType");

            if (val is 2)
                return true;
        }
        catch
        {
            // Ignore registry read failures
        }

        return false;
#endif

        // Non-Linux builds that don't include the WINDOWS block (or other OSes)
        return false;
    }
}