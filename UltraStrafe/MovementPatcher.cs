using HarmonyLib;

namespace UltraStrafe
{
    [HarmonyPatch(typeof(NewMovement))]
    public class MovementPatcher
    {

        [HarmonyPatch(typeof(NewMovement), "Move")]
        [HarmonyPrefix]
        static bool MovePrefix(NewMovement __instance)
        {
        QuakeMovement.NewMove(__instance);
            return false;
        }

        [HarmonyPatch(typeof(NewMovement), "Update")]
        [HarmonyPrefix]
        static bool UpdatePrefix(NewMovement __instance)
        {
            JumpBuffer.JumpBufferCheck(__instance);
            return true;
        }

        [HarmonyPatch(typeof(NewMovement), "Jump")]
        [HarmonyPrefix]
        static bool JumpPrefix(NewMovement __instance)
        {
            __instance.jumpPower = ConfigManager.sv_jumppower.Value;
            return true;
        }
    }
}
