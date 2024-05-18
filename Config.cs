using System.Text.Json;
using HueLightDimmer.Models;

namespace HueLightDimmer
{
    internal class Config
    {
        public string HueUser { get; set; } = string.Empty;
        public string HueIP { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public bool UseGroup { get; set; } = false;
        public List<string> TargetNames { get; set; } = [];
        public HueModificationOption RunningBrightness { get; set; } = new();
        public HueModificationOption RunningHue { get; set; } = new();
        public HueModificationOption RunningSaturation { get; set; } = new();
        public HueLightingState RunningOn { get; set; } = HueLightingState.None;
        public int TransitionTime { get; set; } = 0;
        public bool RevertOnStop { get; set; } = false;
        public int UpdateRate { get; set; } = 60;

        #region Creation
        internal static async Task<Config> FromConfigFile()
        {
            var directoryPath = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(directoryPath, "config.json");
            Config? config = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    Logger.Debug($"Reading config from {fullPath}");
                    var configText = await File.ReadAllTextAsync(fullPath);
                    config = JsonSerializer.Deserialize<Config>(configText);
                    Logger.Info($"Read config from {fullPath}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unable to read config from {fullPath}");
                }
            }

            if (config is null)
            {
                Logger.Debug($"Saving config at {fullPath}");
                config = await CreateConfig();
                try
                {
                    var options = new JsonSerializerOptions() { WriteIndented = true };
                    var configText = JsonSerializer.Serialize(config, options);
                    await File.WriteAllTextAsync(fullPath, configText);
                    Logger.Info($"Saved config at {fullPath}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unable to save config at {fullPath}");
                }
            }

            return config!;
        }

        internal static async Task<Config> CreateConfig()
        {
            var config = new Config
            {
                HueUser = Utils.AskQuestion("What is your hue user?"),
                HueIP = Utils.AskQuestion("What is the hue bridge IP?"),

                ProcessName = Utils.AskQuestion("What is the name of the process lights should be changed for?"),

                UpdateRate = Utils.AskQuestionInt("In which interval (in seconds) should the process be checked? (Recommended = 60)", 1, 600),

                RunningOn = AskQuestionHueLightingState("Should the lights be changed when the process is running?"),
                RunningSaturation = AskQuestionModificationOption("saturation", 0, 254, -254, 254),
                RunningBrightness = AskQuestionModificationOption("brightness", 1, 254, -254, 254),
                RunningHue = AskQuestionModificationOption("hue", 0, 65535, -65534, 65534),

                TransitionTime = Utils.AskQuestionInt("How long should the states take to transition? (1 = 100ms)", ushort.MinValue, ushort.MaxValue),

                RevertOnStop = Utils.AskQuestionBool("Should the lights be reverted when the process is turned off?"),

                UseGroup = Utils.AskQuestionBool("Would you like to control a group instead of a single light?")
            };

            var hueObjects = await HueCommunicationHelper.GetHueObjectsAsync(config);
            DisplayObjectsAsync(hueObjects);
            config.TargetNames = Utils.AskQuestion("Which objects should be affected? (comma to separate)").Split(',').Select(x => x.Trim()).ToList();

            return config;
        }

        #endregion
        #region Utils

        private static HueModificationOption AskQuestionModificationOption(string stateName, int minInclusiveSet, int maxInclusiveSet, int minInclusiveModify, int maxInclusiveModify)
        {
            HueModificationType? type = null;
            while (type is null)
            {
                var res = Utils.AskQuestion($"How should the {stateName} state be changed when the process is running? [n = none, m = modify, s = set]");
                if (res.Length > 0)
                {
                    type = res.ToLower()[0] switch
                    {
                        'n' => HueModificationType.None,
                        'm' => HueModificationType.Modify,
                        's' => HueModificationType.Set,
                        _ => null
                    };
                }
                if (type is null)
                {
                    Console.WriteLine("Invalid state");
                }
            }

            int value = type.Value switch
            {
                HueModificationType.Modify => Utils.AskQuestionInt($"How much should the {stateName} state be modified when the process is running?", minInclusiveModify, maxInclusiveModify),
                HueModificationType.Set => Utils.AskQuestionInt($"To what should the {stateName} state be set when the process is running?", minInclusiveSet, maxInclusiveSet),
                _ => 0
            };
            
            return new() { ModificationType = type.Value, ModificationValue = value };
        }

        private static void DisplayObjectsAsync(Dictionary<string, HueObjectBase> hueObjects)
        {
            var text = string.Join(", ", hueObjects.Select(x => $"{x.Key}({x.Value.Name})"));
            Console.WriteLine("All objects: " + text);
        }

        private static HueLightingState AskQuestionHueLightingState(string question)
        {
            HueLightingState? state = null;
            while (state is null)
            {
                var res = Utils.AskQuestion($"{question} [n = none, i = inactive, a = active]");
                if (res.Length > 0)
                {
                    state = res.ToLower()[0] switch
                    {
                        'n' => HueLightingState.None,
                        'i' => HueLightingState.Inactive,
                        'a' => HueLightingState.Active,
                        _ => null
                    };
                }
                if (state is null)
                {
                    Console.WriteLine("Invalid state");
                }
            }
            return state.Value;
        }
        #endregion
    }

    internal enum HueLightingState
    {
        None,
        Inactive,
        Active
    }
}
