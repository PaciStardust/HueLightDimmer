using System.Text.Json.Serialization;

namespace HueLightDimmer.Models
{
    internal class HueLightingData
    {
        [JsonPropertyName("on"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? On { get; set; }

        private byte? _brightness;
        [JsonPropertyName("bri"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public byte? Brightness
        {
            get => _brightness;
            set => _brightness = value is null ? null : Math.Max((byte)1, Math.Min((byte)254, value.Value));
        }

        [JsonPropertyName("hue"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ushort? Hue { get; set; }

        private byte? _saturation;
        [JsonPropertyName("sat"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public byte? Saturation
        {
            get => _saturation;
            set => _saturation = value is null ? null : Math.Min((byte)254, value.Value);
        }
    }
}
