using System.Text.Json.Serialization;

namespace HueLightDimmer.Models
{
    internal abstract class HueObjectBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        internal abstract HueLightingData GetLightingData();
    }
}
