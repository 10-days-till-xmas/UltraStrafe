using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using System;

namespace UltraStrafe;

[BepInPlugin(UltraStrafePluginInfo.PLUGIN_GUID, UltraStrafePluginInfo.PLUGIN_NAME, UltraStrafePluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigEntry<float> sv_accelerate;
    internal static ConfigEntry<float> sv_maxspeed;
    internal static ConfigEntry<Byte> sv_maxfrictionlessframes;
    internal static ConfigEntry<bool> sv_acceltweak;
    internal static ConfigEntry<float> sv_switchspeed;
    internal static ConfigEntry<bool> sv_autobhop;

    internal void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {UltraStrafePluginInfo.PLUGIN_NAME} is loaded! :3");

        sv_accelerate = Config.Bind(
            "Cvars",
            "sv_accelerate", 
            70f, // TODO: try finding the best value for this
            "The (base) acceleration value for the strafe movement, affecting the turn radius");
        sv_maxspeed = Config.Bind(
            "Cvars",
            "sv_maxspeed",
            320000f,
            "The maximum speed value for the strafe movement");
        sv_maxfrictionlessframes = Config.Bind<Byte>(
            "Cvars",
            "sv_maxfrictionlessframes",
            2,
            "The maximum number of frames that friction is disabled for when landing. A value from 2-4 is recommended depending on your specs with lower framerates having a higher value.");
        sv_acceltweak = Config.Bind(
            "Cvars",
            "sv_acceltweak",
            true,
            "Enable this to make sv_accelerate increase based on your speed");
        sv_switchspeed = Config.Bind(
            "Cvars",
            "sv_switchspeed",
            16.50f,
            "The speed at which it switches from ultrakill physics (low speeds) to quake (high speeds). I don't recommend changing unless you know what you're doing.");
        sv_autobhop = Config.Bind(
            "Cvars",
            "sv_autobhop",
            false,
            "Enable this to make the game automatically jump while holding jump, without any need to release the jump key");

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
