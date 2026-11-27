using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColourClashNet.Pipeline
{
    /// <summary>
    /// Saves and loads <see cref="PipelineDefinition"/> files as JSON.
    /// <para/>
    /// The <c>Version</c> property is read first: a file written by a newer program is refused with a clear message
    /// instead of being half loaded. A file without <c>Version</c> is read as version 1.
    /// Step and options types are stored in the <c>$type</c> properties, enums are written by name.
    /// </summary>
    public static class PipelineSerializer
    {
        // Same settings as the transformation options (they are nested in the transform steps)
        static readonly JsonSerializerOptions jsonOptions = ColourClashNet.Color.Transformation.TransformOptionsSerializer.CreateJsonOptions();

        /// <summary>
        /// Serializes a pipeline to JSON.
        /// </summary>
        public static string Serialize(PipelineDefinition pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            return JsonSerializer.Serialize(pipeline, jsonOptions);
        }

        /// <summary>
        /// Deserializes a pipeline, after checking its version.
        /// </summary>
        /// <exception cref="JsonException">Invalid JSON, unknown step or options type, or unsupported version.</exception>
        public static PipelineDefinition Deserialize(string json)
        {
            int version = ReadVersion(json);
            if (version > PipelineDefinition.CurrentVersion)
                throw new JsonException($"pipeline version {version} requires a newer program (this one reads versions 1 to {PipelineDefinition.CurrentVersion})");
            var pipeline = JsonSerializer.Deserialize<PipelineDefinition>(json, jsonOptions)
                ?? throw new JsonException("empty pipeline");
            // Future versions: convert older files here, step by step, before returning them
            return pipeline with { Version = Math.Max(1, pipeline.Version) };
        }

        /// <summary>
        /// Saves a pipeline to a file.
        /// </summary>
        public static void Save(PipelineDefinition pipeline, string fileName)
            => File.WriteAllText(fileName, Serialize(pipeline));

        /// <summary>
        /// Loads a pipeline from a file.
        /// </summary>
        /// <exception cref="JsonException">Invalid file content.</exception>
        public static PipelineDefinition Load(string fileName)
            => Deserialize(File.ReadAllText(fileName));

        /// <summary>
        /// Reads the <c>Version</c> property (any letter case) without deserializing the whole file.
        /// </summary>
        /// <returns>The version, or 1 when it is not present.</returns>
        static int ReadVersion(string json)
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException("a pipeline file must contain a JSON object");
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (string.Equals(property.Name, nameof(PipelineDefinition.Version), StringComparison.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out int version))
                        throw new JsonException($"{nameof(PipelineDefinition.Version)} must be an integer");
                    return version;
                }
            }
            return 1;
        }
    }
}
