using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Enums.JsonOptions;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Json.OptionsCollection;
using Soenneker.Extensions.String;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Json.Abstract;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Utils.Runtime;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Soenneker.Utils.Json;

///<inheritdoc cref="IJsonUtil"/>
public sealed class JsonUtil : IJsonUtil
{
    private static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly IFileUtil _fileUtil;

    public JsonUtil(IFileUtil fileUtil)
    {
        _fileUtil = fileUtil;
    }

    [Pure]
    private static JsonSerializerOptions GetOptionsOrWeb(JsonOptionType? optionType) =>
        optionType is null ? JsonOptionsCollection.WebOptions : JsonOptionsCollection.GetOptionsFromType(optionType);

    /// <summary>
    /// Uses WebOptions as default
    /// </summary>
    [Pure]
    public static T? Deserialize<T>(string str, JsonLibraryType? libraryType = null)
    {
        if (string.IsNullOrEmpty(str))
            return default;

        return libraryType is null || libraryType == JsonLibraryType.SystemTextJson
            ? JsonSerializer.Deserialize<T>(str, JsonOptionsCollection.WebOptions)
            : JsonConvert.DeserializeObject<T>(str, JsonOptionsCollection.Newtonsoft);
    }

    /// <summary>
    /// Uses WebOptions as default
    /// </summary>
    [Pure]
    public static T? Deserialize<T>(Stream stream, JsonLibraryType? libraryType = null)
    {
        return libraryType is null || libraryType == JsonLibraryType.SystemTextJson
            ? JsonSerializer.Deserialize<T>(stream, JsonOptionsCollection.WebOptions)
            : DeserializeViaNewtonsoft<T>(stream, JsonOptionsCollection.Newtonsoft);
    }

    /// <summary>
    /// Uses WebOptions as default. Only uses System.Text.Json
    /// </summary>
    [Pure]
    public static T? Deserialize<T>(ReadOnlySpan<byte> utf8Json)
    {
        if (utf8Json.Length == 0)
            return default;

        return JsonSerializer.Deserialize<T>(utf8Json, JsonOptionsCollection.WebOptions);
    }

    /// <summary>
    /// Uses WebOptions as default. Only uses System.Text.Json. Avoids string allocation. Wraps in a Try catch to log.
    /// </summary>
    [Pure]
    public static async ValueTask<T?> Deserialize<T>(HttpResponseMessage response, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                                             .NoSync();
            return await JsonSerializer.DeserializeAsync<T>(contentStream, JsonOptionsCollection.WebOptions, cancellationToken)
                                       .NoSync();
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Failed to deserialize response content");
            return default;
        }
    }

    /// <summary>
    /// Uses WebOptions as default. Only uses System.Text.Json. Avoids string allocation. Wraps in a Try catch to log.
    /// </summary>
    [Pure]
    public static async ValueTask<T?> Deserialize<T>(Stream stream, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptionsCollection.WebOptions, cancellationToken)
                                       .NoSync();
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Failed to deserialize response content");
            return default;
        }
    }

    /// <summary>
    /// Uses WebOptions as default
    /// </summary>
    [Pure]
    public static object? Deserialize(string str, Type type, JsonLibraryType? libraryType = null)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        return libraryType is null || libraryType == JsonLibraryType.SystemTextJson
            ? JsonSerializer.Deserialize(str, type, JsonOptionsCollection.WebOptions)
            : JsonConvert.DeserializeObject(str, type, JsonOptionsCollection.Newtonsoft);
    }

    /// <summary>
    /// Uses WebOptions as default
    /// </summary>
    [Pure]
    public static object? Deserialize(Stream stream, Type type, JsonLibraryType? libraryType = null)
    {
        return libraryType is null || libraryType == JsonLibraryType.SystemTextJson
            ? JsonSerializer.Deserialize(stream, type, JsonOptionsCollection.WebOptions)
            : DeserializeViaNewtonsoft(stream, type, JsonOptionsCollection.Newtonsoft);
    }

    /// <summary>
    /// Accepts a nullable object... if null returns null. If optionType is left null, will use WebOptions.
    /// </summary>
    [Pure]
    public static string? Serialize(object? obj, JsonOptionType? optionType = null, JsonLibraryType? libraryType = null)
    {
        if (obj is null)
            return null;

        if (libraryType is not null && libraryType != JsonLibraryType.SystemTextJson)
            return JsonConvert.SerializeObject(obj, JsonOptionsCollection.Newtonsoft);

        JsonSerializerOptions options = GetOptionsOrWeb(optionType);
        return JsonSerializer.Serialize(obj, options);
    }

    [Pure]
    public static JsonElement? SerializeToElement(object? obj, JsonOptionType? optionType = null)
    {
        if (obj is null)
            return null;

        JsonSerializerOptions options = GetOptionsOrWeb(optionType);
        return JsonSerializer.SerializeToElement(obj, options);
    }

    /// <summary>
    /// Serializes the object into the given stream (System.Text.Json by default; can use Newtonsoft if specified)
    /// </summary>
    public static Task SerializeToStream(Stream stream, object? obj, JsonOptionType? optionType = null, JsonLibraryType? libraryType = null,
        CancellationToken cancellationToken = default)
    {
        if (libraryType is not null && libraryType != JsonLibraryType.SystemTextJson)
        {
            // Newtonsoft has no async writer; this is sync on the caller thread.
            SerializeViaNewtonsoft(obj!, stream, JsonOptionsCollection.Newtonsoft);
            return Task.CompletedTask;
        }

        JsonSerializerOptions options = GetOptionsOrWeb(optionType);
        return JsonSerializer.SerializeAsync(stream, obj, options, cancellationToken);
    }

    /// <summary>
    /// Serializes an object to a UTF-8 encoded byte array using System.Text.Json.
    /// </summary>
    public static byte[] SerializeToUtf8Bytes(object obj, JsonOptionType? optionType = null)
    {
        JsonSerializerOptions options = GetOptionsOrWeb(optionType);
        return JsonSerializer.SerializeToUtf8Bytes(obj, options);
    }

    public static async ValueTask<T?> DeserializeFromFile<T>(string path, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        await using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, useAsync: true);
        return await Deserialize<T>(fileStream, logger, cancellationToken)
            .NoSync();
    }

    public static async ValueTask SerializeToFile(object? obj, string path, JsonOptionType? optionType = null, JsonLibraryType? libraryType = null,
        CancellationToken cancellationToken = default)
    {
        if (obj is null)
            return;

        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);
        await SerializeToStream(fileStream, obj, optionType, libraryType, cancellationToken)
            .NoSync();
    }

    /// <summary>
    /// True "Try" parse: returns false on invalid JSON. Supports optional source-gen metadata.
    /// </summary>
    public static bool TryDeserialize<T>(ReadOnlySpan<byte> utf8Json, out T? value, JsonTypeInfo<T>? typeInfo = null)
    {
        if (utf8Json.Length == 0)
        {
            value = default;
            return false;
        }

        try
        {
            value = typeInfo is null
                ? JsonSerializer.Deserialize<T>(utf8Json, JsonOptionsCollection.WebOptions)
                : JsonSerializer.Deserialize(utf8Json, typeInfo);

            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    public static bool IsJsonValid(string str, ILogger? logger = null)
    {
        if (str.IsNullOrEmpty())
        {
            logger?.LogWarning("JSON is invalid");
            return false;
        }

        try
        {
            using JsonDocument _ = JsonDocument.Parse(str);
            return true;
        }
        catch
        {
            logger?.LogWarning("JSON is invalid");
            return false;
        }
    }

    private static void SerializeViaNewtonsoft(object value, Stream stream, JsonSerializerSettings? settings)
    {
        using var writer = new StreamWriter(stream, _utf8NoBom, bufferSize: 16 * 1024, leaveOpen: true);
        using var jsonWriter = new JsonTextWriter(writer) { CloseOutput = false };
        var serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
        serializer.Serialize(jsonWriter, value);
        jsonWriter.Flush();
    }

    private static T? DeserializeViaNewtonsoft<T>(Stream stream, JsonSerializerSettings? settings)
    {
        using var reader = new StreamReader(stream, _utf8NoBom, detectEncodingFromByteOrderMarks: true, bufferSize: 16 * 1024, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader) { CloseInput = false };
        var serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
        return serializer.Deserialize<T>(jsonReader);
    }

    private static object? DeserializeViaNewtonsoft(Stream stream, Type type, JsonSerializerSettings? settings)
    {
        using var reader = new StreamReader(stream, _utf8NoBom, detectEncodingFromByteOrderMarks: true, bufferSize: 16 * 1024, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader) { CloseInput = false };
        var serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
        return serializer.Deserialize(jsonReader, type);
    }

    public static string Format(string json, bool forceWindowsLineEndings)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        string result = JsonSerializer.Serialize(doc, JsonOptionsCollection.PrettySafeOptions);

        if (!forceWindowsLineEndings || RuntimeUtil.IsWindows())
            return result;

        // avoids alloc if already \r\n
        return result.IndexOf('\r') >= 0 ? result : result.Replace("\n", "\r\n");
    }

    public async ValueTask WritePretty(string sourcePath, string destinationPath, bool forceWindowsLineEndings, bool log = true,
        CancellationToken cancellationToken = default)
    {
        await using FileStream readStream = _fileUtil.OpenRead(sourcePath, log);

        using JsonDocument doc = await JsonDocument.ParseAsync(readStream, cancellationToken: cancellationToken)
                                                   .NoSync();

        if (forceWindowsLineEndings && !RuntimeUtil.IsWindows())
        {
            string formatted = JsonSerializer.Serialize(doc.RootElement, JsonOptionsCollection.PrettySafeOptions);

            formatted = formatted.Replace("\r\n", "\n")
                                 .Replace("\r", "\n")
                                 .Replace("\n", "\r\n");

            await _fileUtil.Write(destinationPath, formatted, log, cancellationToken)
                           .NoSync();
            return;
        }

        await using FileStream writeStream = _fileUtil.OpenWrite(destinationPath, log);

        await using var writer = new Utf8JsonWriter(writeStream, new JsonWriterOptions
        {
            Indented = true
        });

        doc.RootElement.WriteTo(writer);

        await writer.FlushAsync(cancellationToken)
                    .NoSync();
    }
}