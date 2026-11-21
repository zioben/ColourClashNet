using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Saves and loads <see cref="TransformOptions"/> presets as JSON.
    /// <para/>
    /// The record type is stored in the <c>$type</c> property, enums are written by name.
    /// </summary>
    public static class TransformOptionsSerializer
    {
        static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

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
