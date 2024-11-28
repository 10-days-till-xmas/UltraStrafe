using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UltraStrafe
{
    [HarmonyPatch(typeof(NewMovement))]
    public class MovementPatcher
    {

        [HarmonyPatch(typeof(NewMovement), "Move")]
        [HarmonyPrefix]
        static bool MovePrefix(NewMovement __instance)
        {
            QuakeMovement QuakeMovement = new(__instance);
            QuakeMovement.NewMove();
            return false;
        }

        [HarmonyPatch(typeof(NewMovement), "Update")]
        [HarmonyPrefix]
        static bool UpdatePrefix(NewMovement __instance)
        {
            JumpBuffer.JumpBufferCheck(__instance);
            return true;
        }
    }
}
