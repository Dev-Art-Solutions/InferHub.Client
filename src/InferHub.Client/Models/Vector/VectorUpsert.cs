using System.Text.Json;
using System.Text.Json.Serialization;

namespace InferHub.Client.Models.Vector;

/// <summary>
/// Body for <c>POST /api/vector/{collection}/upsert</c>. Provide either a raw
/// <see cref="Vector"/> or a <see cref="Text"/> (embedded on a node using <see cref="Model"/>).
/// Use <see cref="FromVector(string, float[])"/> or <see cref="FromText(string, string, string?)"/>
/// for the common shapes; set <see cref="Payload"/> / <see cref="Metadata"/> afterwards as needed.
/// </summary>
public sealed class VectorUpsert
{
    /// <summary>Caller-assigned record id (required). An existing id is overwritten.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Raw vector to store. Leave null to have the coordinator embed <see cref="Text"/>.</summary>
    [JsonPropertyName("vector")]
    public float[]? Vector { get; set; }

    /// <summary>Opaque payload stored and echoed back on reads and matches.</summary>
    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    /// <summary>Flat string→string metadata used for query filtering.</summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; set; }

    /// <summary>Text to embed on a node when <see cref="Vector"/> is not supplied.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>Embedding model tag used when embedding <see cref="Text"/>.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Any additional fields to pass through.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    /// <summary>Build an upsert that stores a pre-computed vector.</summary>
    public static VectorUpsert FromVector(string id, float[] vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        return new VectorUpsert { Id = id, Vector = vector };
    }

    /// <summary>Build an upsert that stores text, embedded on a node (optionally with an explicit model).</summary>
    public static VectorUpsert FromText(string id, string text, string? model = null)
    {
        return new VectorUpsert { Id = id, Text = text, Model = model };
    }

    /// <summary>Attach a strongly-typed payload, serialized to JSON. Returns <c>this</c> for chaining.</summary>
    public VectorUpsert WithPayload<T>(T payload, JsonSerializerOptions? options = null)
    {
        Payload = JsonSerializer.SerializeToElement(payload, options);
        return this;
    }

    /// <summary>Attach metadata. Returns <c>this</c> for chaining.</summary>
    public VectorUpsert WithMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Metadata = metadata;
        return this;
    }
}
