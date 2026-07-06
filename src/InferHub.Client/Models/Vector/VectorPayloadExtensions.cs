using System.Text.Json;

namespace InferHub.Client.Models.Vector;

/// <summary>Helpers for reading the opaque <c>payload</c> carried by records and matches.</summary>
public static class VectorPayloadExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Deserialize a nullable payload into <typeparamref name="T"/>. Returns <c>default</c>
    /// when the payload is absent or JSON <c>null</c>.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <param name="payload">The payload element (e.g. <c>record.Payload</c>).</param>
    /// <param name="options">Serializer options; web defaults are used when null.</param>
    public static T? As<T>(this JsonElement? payload, JsonSerializerOptions? options = null)
    {
        if (payload is not { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } element)
        {
            return default;
        }

        return element.Deserialize<T>(options ?? DefaultOptions);
    }
}
