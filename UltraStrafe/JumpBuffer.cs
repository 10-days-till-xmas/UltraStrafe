using HarmonyLib;

namespace UltraStrafe;

internal static class JumpBuffer
{
    private static bool jumpBuffer = false;

    private static readonly AccessTools.FieldRef<NewMovement, WallCheck> wcRef =
        AccessTools.FieldRefAccess<NewMovement, WallCheck>("wc");
    private static readonly AccessTools.FieldRef<NewMovement, float> clingFadeRef =
        AccessTools.FieldRefAccess<NewMovement, float>("clingFade");
    private static readonly AccessTools.FieldRef<NewMovement, bool> jumpCooldownRef =
        AccessTools.FieldRefAccess<NewMovement, bool>("jumpCooldown");
    private static readonly AccessTools.FieldRef<NewMovement, bool> fallingRef =
        AccessTools.FieldRefAccess<NewMovement, bool>("falling");

    public static void JumpBufferCheck(NewMovement __instance)
    {
        if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
        {
            jumpBuffer = true;
        }

        if (GameStateManager.Instance.PlayerInputLocked
            || !jumpBuffer
            || !MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed
            || jumpCooldownRef(__instance)
            || (fallingRef(__instance) && !__instance.gc.canJump && !wcRef(__instance).CheckForEnemyCols())) return;
        if (__instance.gc.canJump || wcRef(__instance).CheckForEnemyCols())
        {
            __instance.currentWallJumps = 0;
            __instance.rocketJumps = 0;
            __instance.hammerJumps = 0;
            clingFadeRef(__instance) = 0f;
            __instance.rocketRides = 0;
        }
        jumpBuffer = ConfigManager.sv_autobhop.Value;
        __instance.Jump();
    }
}