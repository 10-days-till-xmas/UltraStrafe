using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using UnityEngine;

namespace UltraStrafe;

[BepInPlugin(UltraStrafePluginInfo.PLUGIN_GUID, UltraStrafePluginInfo.PLUGIN_NAME, UltraStrafePluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigEntry<float> sv_accelerate;
    internal static ConfigEntry<float> sv_maxspeed;
    internal static ConfigEntry<Byte> sv_maxfrictionlessframes;
    internal static ConfigEntry<bool> sv_acceltweak;

    internal void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {UltraStrafePluginInfo.PLUGIN_NAME} is loaded! :3");

        sv_accelerate = Config.Bind(
            "Cvars",
            "sv_accelerate", 
            80f, // TODO: try finding the best value for this
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
            "The maximum number of frames that friction is disabled for when landing. A value from 2-4 is recommended depending on your specs with lower framerates having a higher value");
        sv_acceltweak = Config.Bind(
            "Cvars",
            "sv_acceltweak",
            false,
            "Enable this to make the acceleration value scaled based off of an equation");

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
