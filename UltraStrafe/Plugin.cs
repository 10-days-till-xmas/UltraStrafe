using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;

namespace UltraStrafe;

[BepInPlugin(UltraStrafePluginInfo.PLUGIN_GUID, UltraStrafePluginInfo.PLUGIN_NAME, UltraStrafePluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigManager ConfigManager;

    internal void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {UltraStrafePluginInfo.PLUGIN_NAME} is loaded! :3");
        ConfigManager = new ConfigManager();
        ConfigManager.Reload();
        DoPatching();
    }

    private void DoPatching()
    {
        var harmony = new Harmony(UltraStrafePluginInfo.PLUGIN_GUID);
        HarmonyFileLog.Enabled = true;
        harmony.PatchAll();
        Logger.LogInfo("Patches applied!");
        // TODO: add a console patch so you can change configs in-game
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameStateManager), "CanSubmitScores", MethodType.Getter)]
    static void ScoresSubmission(ref bool __result) 
    {
        // prevent scores from being submitted since this mod is technically a cheat
        __result = false;
        // remove if using for a level plugin
    }
}
