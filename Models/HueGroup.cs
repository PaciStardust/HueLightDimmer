using System.Text.Json.Serialization;

namespace HueLightDimmer.Models
{
    internal class HueGroup : HueObjectBase
    {
        [JsonPropertyName("action")]
        public HueLightingData Action { get; set; } = new();

        internal override HueLightingData GetLightingData() => Action;
    }
}
