using System.Text.Json.Serialization;

namespace HueLightDimmer.Models
{
    internal class HueLight : HueObjectBase
    {
        [JsonPropertyName("state")]
        public HueLightingData State { get; set; } = new();

        internal override HueLightingData GetLightingData() => State;
    }
}
