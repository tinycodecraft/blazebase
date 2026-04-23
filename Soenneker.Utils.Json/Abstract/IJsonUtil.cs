using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Json.Abstract;

/// <summary>
/// Utility for formatting JSON and saving formatted output to a file.
/// </summary>
public interface IJsonUtil
{
    /// <summary>
    /// Reads a JSON file, formats it, and writes the result to the destination path.
    /// </summary>
    /// <param name="sourcePath">Path to the source JSON file.</param>
    /// <param name="destinationPath">Path where the formatted JSON output is written.</param>
    /// <param name="forceWindowsLineEndings">When <c>true</c>, outputs CRLF line endings on non-Windows systems.</param>
    /// <param name="log">Whether to log the file operations.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask WritePretty(string sourcePath, string destinationPath, bool forceWindowsLineEndings, bool log = true,
        CancellationToken cancellationToken = default);
}