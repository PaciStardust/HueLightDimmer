using System.Text.Json.Serialization;

namespace HueLightDimmer.Models
{
    internal class HueLightStateRequest : HueLightingData
    {
        [JsonPropertyName("transitiontime"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ushort? TransitionTime { get; set; }

        private short? _brightnessIncrease;
        [JsonPropertyName("bri_inc"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public short? BrightnessIncrease
        {
            get => _brightnessIncrease;
            set => _brightnessIncrease = value is null ? null : Math.Max((short)-254, Math.Min((short)254, value.Value));
        }

        private short? _saturationIncrease;
        [JsonPropertyName("sat_inc"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public short? SaturationIncrease
        {
            get => _saturationIncrease;
            set => _saturationIncrease = value is null ? null : Math.Max((short)-254, Math.Min((short)254, value.Value));
        }

        [JsonPropertyName("hue_inc"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? HueIncrease { get; set; }

        private int? _colorTemperatureIncrease;
        [JsonPropertyName("ct_inc"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ColorTemperatureIncrease
        {
            get => _colorTemperatureIncrease;
            set => _colorTemperatureIncrease = value is null ? null : Math.Max(-65534, Math.Min(65534, value.Value));
        }

        internal HueLightStateRequest() { }

        internal HueLightStateRequest(HueLightingData baseClass)
        {
            On = baseClass.On;
            Hue = baseClass.Hue;
            Saturation = baseClass.Saturation;
            Brightness = baseClass.Brightness;
        }
    }
}
