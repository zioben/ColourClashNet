using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Saves and loads <see cref="TransformOptions"/> presets as JSON.
    /// <para/>
    /// The record type is stored in the <c>$type</c> property, enums are written by name.
    /// </summary>
    public static class TransformOptionsSerializer
    {
        static readonly JsonSerializerOptions jsonOptions = CreateJsonOptions();

        /// <summary>
        /// JSON settings shared by the options and the pipeline files.
        /// <para/>
        /// Indented, enums by name, readable characters (no <c>\u0027</c> escapes), and only the settings are written:
        /// read-only computed properties (<c>TransformType</c>, <c>Kind</c> ...) are left out.
        /// Reading accepts comments, trailing commas, any letter case and <c>$type</c> in any position (hand written files).
        /// </summary>
        public static JsonSerializerOptions CreateJsonOptions() => new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            AllowOutOfOrderMetadataProperties = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { SkipComputedProperties } },
        };

        /// <summary>
        /// Removes the read-only properties of the project records: they are computed, not settings.
        /// </summary>
        static void SkipComputedProperties(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object || typeInfo.Type.Namespace?.StartsWith("ColourClashNet") != true)
                return;
            for (int i = typeInfo.Properties.Count - 1; i >= 0; i--)
            {
                if (typeInfo.Properties[i].Set == null)
                    typeInfo.Properties.RemoveAt(i);
            }
        }

        /// <summary>
        /// Serializes options (any derived record) to JSON.
        /// </summary>
        public static string Serialize(TransformOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            return JsonSerializer.Serialize(options, jsonOptions);
        }

        /// <summary>
        /// Deserializes options written by <see cref="Serialize"/>.
        /// </summary>
        /// <exception cref="JsonException">Invalid JSON or unknown <c>$type</c>.</exception>
        public static TransformOptions Deserialize(string json)
            => JsonSerializer.Deserialize<TransformOptions>(json, jsonOptions)
               ?? throw new JsonException("Empty transformation options");

        /// <summary>
        /// Saves options to a file.
        /// </summary>
        public static void Save(TransformOptions options, string fileName)
            => File.WriteAllText(fileName, Serialize(options));

        /// <summary>
        /// Loads options from a file.
        /// </summary>
        public static TransformOptions Load(string fileName)
            => Deserialize(File.ReadAllText(fileName));
    }
}
