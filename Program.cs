using System.Diagnostics;
using HueLightDimmer.Models;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

namespace HueLightDimmer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = await Config.FromConfigFile();

            if (args.Contains("a"))
            {
                HideConsole();
            }
            else
            {
                AskForAutostart();
            }

            bool running = false;
            var savedLightData = new Dictionary<string, HueLightingData>();
            while (true)
            {
                var processFound = Process.GetProcesses().Any(x => x.ProcessName.Contains(config.ProcessName, StringComparison.OrdinalIgnoreCase));
                if (!processFound && running)
                {
                    if (config.RevertOnStop)
                    {
                        try
                        {
                            await HueCommunicationHelper.ApplyCustomLightingAsync(config, savedLightData);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to revert light changes");
                        }
                    }
                    running = false;
                }
                else if (processFound && !running)
                {
                    try
                    {
                        var objectData = await HueCommunicationHelper.GetHueObjectsAsync(config);
                        savedLightData = objectData.ToDictionary(k => k.Key, v => v.Value.GetLightingData());
                        await HueCommunicationHelper.ApplyProcessRunningLightingAsync(config);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to apply light changes");
                    }
                    running = true;
                }
#if DEBUG
                Logger.Debug("Application is running: " + running);
#endif
                await Task.Delay(1_000 * config.UpdateRate);
            }
        }

        #region
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void HideConsole()
        {
            try
            {
                var cWin = GetConsoleWindow();
                ShowWindow(cWin, 0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to hide console window");
            }
        }

        private static void AskForAutostart()
        {
            try
            {
                Logger.Debug("Grabbing registy keys to check for autostart...");
                var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                var current = registryKey!.OpenSubKey("HueLightDimmer");
                Logger.Info("Grabbed registy keys");

                if (registryKey!.GetValue("HueLightDimmer") is null)
                {
                    var res = Utils.AskQuestionBool("Program is currently not starting automatically, enable this feature?");
                    if (res)
                    {
                        Logger.Debug("Seting application to start automatically");
                        registryKey!.SetValue("HueLightDimmer", $"{Assembly.GetExecutingAssembly().Location} /a");
                        Logger.Info("Set application to not start automatically");
                    }
                }
                else
                {
                    var res = Utils.AskQuestionBool("Program is currently starting automatically, disable this feature?");
                    if (res)
                    {
                        Logger.Debug("Setting application to not start automatically");
                        registryKey.DeleteValue("HueLightDimmer");
                        Logger.Info("Set application to start automatically, to undo this simply manually start this program or remove the autostart via taskmanager");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to work with registy to check autostartup");
            }
        }
        #endregion
    }
}
