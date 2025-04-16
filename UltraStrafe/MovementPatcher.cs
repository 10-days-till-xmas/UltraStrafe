using HarmonyLib;

namespace UltraStrafe;

[HarmonyPatch(typeof(NewMovement))]
public class MovementPatcher
{

    [HarmonyPatch( "Move")]
    [HarmonyPrefix]
    private static bool MovePrefix(NewMovement __instance)
    {
        QuakeMovement.NewMove(__instance);
        return false;
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