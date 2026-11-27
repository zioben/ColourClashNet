using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColourClashNet.Color
{
    /// <summary>
    /// JSON converter of a packed color: written as <c>"#RRGGBB"</c>, read from <c>"#RRGGBB"</c>, <c>"0xRRGGBB"</c>,
    /// <c>"RRGGBB"</c> or a plain number (older files).
    /// <para/>
    /// Values with <see cref="ColorInfo"/> flags (negative) are written as numbers, so nothing is lost.
    /// </summary>
    public sealed class ColorJsonConverter : JsonConverter<int>
    {
        /// <inheritdoc/>
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => ReadColor(ref reader);

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => WriteColor(writer, value);

        /// <summary>
        /// Reads a color written as a string or as a number.
        /// </summary>
        /// <exception cref="JsonException">Invalid color text.</exception>
        internal static int ReadColor(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"a color must be \"#RRGGBB\" or a number, found {reader.TokenType}");
            var text = reader.GetString()?.Trim() ?? string.Empty;
            var hex = text.StartsWith('#') ? text[1..]
                : text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? text[2..]
                : text;
            hex = hex.Replace("_", string.Empty);
            if (hex.Length == 0 || hex.Length > 8 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                throw new JsonException($"invalid color \"{text}\": use \"#RRGGBB\"");
            return value;
        }

        /// <summary>
        /// Writes a color as <c>"#RRGGBB"</c>, or as a number when it carries flags.
        /// </summary>
        internal static void WriteColor(Utf8JsonWriter writer, int value)
        {
            if (value >= 0 && value <= 0xFFFFFF)
                writer.WriteStringValue($"#{value:X6}");
            else
                writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// JSON converter of a list of packed colors (palettes, background colors): an array of <c>"#RRGGBB"</c> strings.
    /// </summary>
    public sealed class ColorListJsonConverter : JsonConverter<IReadOnlyList<int>>
    {
        /// <inheritdoc/>
        public override IReadOnlyList<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("a color list must be an array, e.g. [ \"#FF00FF\", \"#000000\" ]");
            var colors = new List<int>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                colors.Add(ColorJsonConverter.ReadColor(ref reader));
            return colors;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IReadOnlyList<int> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var color in value)
                ColorJsonConverter.WriteColor(writer, color);
            writer.WriteEndArray();
        }
    }
}
