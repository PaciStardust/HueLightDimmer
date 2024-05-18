using System.Net.Http.Json;
using System.Text.Json;
using HueLightDimmer.Models;

namespace HueLightDimmer
{
    internal static class HueCommunicationHelper
    {
        private static readonly HttpClient _httpClient = new();

        #region Getting Data
        private static async Task<Dictionary<string, HueLight>> GetHueLightsAsync(Config config)
        {
            var lightpath = GetFullUrl(config, "lights");
            Logger.Debug("Grabbing hue lights");
            var res = await _httpClient.GetAsync(lightpath);
            if (!res.IsSuccessStatusCode)
            {
                await ThrowExeption("Unable to get hue lights", res.Content);
            }
            else
            {
                Logger.Info("Grabbed hue lights");
            }
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, HueLight>>(json) ?? [];
        }

        private static async Task<Dictionary<string, HueGroup>> GetHueGroupsAsync(Config config)
        {
            var grouppath = GetFullUrl(config, "groups");
            Logger.Debug("Grabbing hue groups");
            var res = await _httpClient.GetAsync(grouppath);
            if (!res.IsSuccessStatusCode)
            {
                await ThrowExeption("Unable to get hue groups", res.Content);
            }
            else
            {
                Logger.Info("Grabbed hue groups");
            }
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, HueGroup>>(json) ?? [];
        }

        internal static async Task<Dictionary<string, HueObjectBase>> GetHueObjectsAsync(Config config)
        {
            var objects = new Dictionary<string, HueObjectBase>();
            if (config.UseGroup)
            {
                var groups = await GetHueGroupsAsync(config);
                objects = groups.ToDictionary(x => x.Key, v => v.Value as HueObjectBase);
            }
            else
            {
                var lights = await GetHueLightsAsync(config);
                objects = lights.ToDictionary(x => x.Key, v => v.Value as HueObjectBase);
            }
            return objects;
        }
        #endregion

        #region Updating
        internal static async Task ApplyProcessRunningLightingAsync(Config config)
        {
            if (config.TargetNames.Count == 0)
            {
                return;
            }

            var lightingData = CreateHueLightingData(config);
            var content = JsonContent.Create(lightingData);

            var targetType = config.UseGroup ? "group" : "light";
            var targetAction = config.UseGroup ? "action" : "state";
            foreach (var light in config.TargetNames)
            {
                var targetPath = GetFullUrl(config, $"{targetType}s/{light}/{targetAction}");
                Logger.Debug($"Applying running lighting settings for {targetType} {light}");
                var res = await _httpClient.PutAsync(targetPath, content);
                if (!res.IsSuccessStatusCode)
                {
                    await ThrowExeption($"Unable to apply running lighting settings for {targetType} {light}", res.Content);
                }
                else
                {
                    Logger.Info($"Applied running lighting settings for {targetType} {light}");
                }
            }
        }

        private static HueLightStateRequest CreateHueLightingData(Config config)
        {
            var lightingData = new HueLightStateRequest();

            switch (config.RunningBrightness.ModificationType)
            {
                case HueModificationType.Set:
                    lightingData.Brightness = Convert.ToByte(config.RunningBrightness.ModificationValue);
                    break;

                case HueModificationType.Modify:
                    lightingData.BrightnessIncrease = Convert.ToInt16(config.RunningBrightness.ModificationValue);
                    break;
            }

            switch (config.RunningSaturation.ModificationType)
            {
                case HueModificationType.Set:
                    lightingData.Saturation = Convert.ToByte(config.RunningSaturation.ModificationValue);
                    break;

                case HueModificationType.Modify:
                    lightingData.SaturationIncrease = Convert.ToInt16(config.RunningSaturation.ModificationValue);
                    break;
            }

            switch (config.RunningHue.ModificationType)
            {
                case HueModificationType.Set:
                    lightingData.Hue = Convert.ToUInt16(config.RunningHue.ModificationValue);
                    break;

                case HueModificationType.Modify:
                    lightingData.HueIncrease = config.RunningHue.ModificationValue;
                    break;
            }

            switch (config.RunningOn)
            {
                case HueLightingState.Active:
                    lightingData.On = true;
                    break;

                case HueLightingState.Inactive:
                    lightingData.On = false;
                    break;
            }

            lightingData.TransitionTime = config.TransitionTime > 0 ? Convert.ToUInt16(config.TransitionTime) : null;

            return lightingData;
        }

        internal static async Task ApplyCustomLightingAsync(Config config, Dictionary<string, HueLightingData> lightingData)
        {
            var parsedLightingData = lightingData.Where(x => config.TargetNames.Contains(x.Key));
            if (!parsedLightingData.Any())
            {
                return;
            }

            var targetType = config.UseGroup ? "group" : "light";
            var targetAction = config.UseGroup ? "action" : "state";
            foreach (var (k, v) in parsedLightingData)
            {
                var targetPath = GetFullUrl(config, $"{targetType}s/{k}/{targetAction}");
                Logger.Debug($"Applying running lighting settings for {targetType} {k}");
                var lightStateRequest = new HueLightStateRequest(v)
                {
                    TransitionTime = config.TransitionTime > 0 ? Convert.ToUInt16(config.TransitionTime) : null
                };
                var res = await _httpClient.PutAsJsonAsync(targetPath, lightStateRequest);
                if (!res.IsSuccessStatusCode)
                {
                    await ThrowExeption($"Unable to apply custom lighting settings for {targetType} {k}", res.Content);
                }
                else
                {
                    Logger.Info($"Applied custom lighting settings for {targetType} {k}");
                }
            }
        }
        #endregion

        #region Utils
        private static async Task ThrowExeption(string message, HttpContent content)
        {
            var stringContent = await content.ReadAsStringAsync();
            throw new HttpRequestException($"{message}: {stringContent}");
        }

        private static string GetFullUrl(Config config, string path)
            => $"http://{config.HueIP}/api/{config.HueUser}/{path}";
        #endregion
    }
}
