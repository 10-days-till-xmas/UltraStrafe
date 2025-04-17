using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace UltraStrafe;

[HarmonyPatch(typeof(NewMovement))]
public class MovementPatcher
{

    [HarmonyTranspiler]
    [HarmonyPatch("Move")]
    private static IEnumerable<CodeInstruction> MoveTranspiler(IEnumerable<CodeInstruction> instructions, 
        ILGenerator generator)
    {
        return QuakeMovement.MoveTranspiler(instructions, generator);
    }
    

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void UpdatePrefix(NewMovement __instance)
    {
        JumpBuffer.JumpBufferCheck(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Jump")]
    private static void JumpPrefix(NewMovement __instance)
    {
        __instance.jumpPower = ConfigManager.sv_jumppower.Value;
    }
}