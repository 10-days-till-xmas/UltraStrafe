using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;

namespace UltraStrafe
{
    [BepInPlugin(UltraStrafePluginInfo.PLUGIN_GUID, UltraStrafePluginInfo.PLUGIN_NAME, UltraStrafePluginInfo.PLUGIN_VERSION)]
    internal class ConfigManager : BaseUnityPlugin
    {
        public Dictionary<string, object> Cvars = new()
        {
            { "sv_accelerate", sv_accelerate},
            { "sv_maxspeed", sv_maxspeed},
            { "sv_maxfrictionlessframes", sv_maxfrictionlessframes},
            { "sv_acceltweak", sv_acceltweak},
            { "sv_switchspeed", sv_switchspeed},
            { "sv_autobhop", sv_autobhop}
        };

        internal static ConfigEntry<float> sv_accelerate;
        internal static ConfigEntry<float> sv_maxspeed;
        internal static ConfigEntry<Byte> sv_maxfrictionlessframes;
        internal static ConfigEntry<bool> sv_acceltweak;
        internal static ConfigEntry<float> sv_switchspeed;
        internal static ConfigEntry<bool> sv_autobhop;

        public void Reload()
        {
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
        }


        public bool Set<T>(string key, T value)
        {
            if (typeof(T) != Cvars[key].GetType()) return false;
            {
                Plugin.Logger.LogError($"Failed to set cvar {key} to {value}: type mismatch");
                return false;
            }
            try
            {
                Cvars[key] = value;
                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Failed to set cvar {key} to {value}: {e}");
                return false;
            }
        }
        public bool Get<T>(string key)
        {
            try
            {
                Plugin.Logger.LogInfo($"{key} = {Cvars[key]}");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Failed to get cvar {key}: {e}");
                return false;
            }
        }
    }
}
