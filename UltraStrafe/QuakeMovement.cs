using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

using static UltraStrafe.Plugin;
namespace UltraStrafe;

internal static class QuakeMovement
{
    private static float wishspeed;
    private static int frictionlessFrames = 0;

    // Field References
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> movementDirectionRef =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("movementDirection");
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> movementDirection2Ref =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("movementDirection2");
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> airDirectionRef =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("airDirection");

    public static float sv_accelerate => ConfigManager.sv_accelerate.Value;
    public static float sv_maxspeed => ConfigManager.sv_maxspeed.Value;
    public static int sv_maxfrictionlessframes => ConfigManager.sv_maxfrictionlessframes.Value;
    public static float sv_switchspeed => ConfigManager.sv_switchspeed.Value;
    public static bool sv_acceltweak => ConfigManager.sv_acceltweak.Value;

    private static float NewAcceleration(float speed)
    {
        // TODO: try changing the parameters of this function to get the best result
        // TODO: use Jace.NET to parse a string and return a delegate
        float factor = Mathf.Clamp(Mathf.Log(speed - 8.5f, 2f) - 2f, 1f, 10f);
        return sv_accelerate * factor;
    }

    private static void QuakeAirMove(NewMovement __instance)
    {
        float speed = new Vector2(__instance.rb.velocity.x, __instance.rb.velocity.z).magnitude;

        Vector3 wishvel = new(
            movementDirectionRef(__instance).x,
            0,
            movementDirectionRef(__instance).z);

        wishspeed = wishvel.magnitude;

        if (wishspeed > sv_maxspeed && sv_maxspeed != 0)
        {
            wishvel *= sv_maxspeed / wishspeed;
            wishspeed = sv_maxspeed;
        }

        var currentspeed = Vector3.Dot(__instance.rb.velocity, wishvel);
        var addspeed = wishspeed - currentspeed; // the maximum amount of speed we need to add

        if (addspeed <= 0)
            return;

        var accelspeed = Time.deltaTime * wishspeed * (sv_acceltweak ? NewAcceleration(speed) : sv_accelerate);
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        __instance.rb.velocity += accelspeed * wishvel;
    }

    private static void NewAirMove(NewMovement __instance)
    {
        __instance.rb.useGravity = true;

        if (QuakeAirMoveInjection(__instance)) return;

        DefaultUltrakillAirMove(__instance);
    }

    private static bool QuakeAirMoveInjection(NewMovement __instance)
    {
        var speed = new Vector2(__instance.rb.velocity.x, __instance.rb.velocity.z).magnitude;

        if (!(speed > sv_switchspeed)) return false;
        // quake air move
        QuakeAirMove(__instance);
        return true;
    }

    private static void DefaultUltrakillAirMove(NewMovement __instance)
    {
        // TODO: use transpilation so this block isn't needed

        // default ultrakill air movement (cleaned up and commented for my better understanding)
        float slowModeNum = __instance.slowMode ? 1.25f : 2.75f;
        // movementDirection is raw player input, normalised to 1
        movementDirection2Ref(__instance) = new Vector3(
            movementDirectionRef(__instance).x * __instance.walkSpeed * Time.deltaTime * slowModeNum,
            __instance.rb.velocity.y,
            movementDirectionRef(__instance).z * __instance.walkSpeed * Time.deltaTime * slowModeNum);

        //movementDirection2 is the actual movement vector, with the y component being the same as the rb velocity

        airDirectionRef(__instance).y = 0f; // the force to be applied to the movement vector

        if (__instance.rb.velocity.x * movementDirection2Ref(__instance).x < Mathf.Pow(movementDirection2Ref(__instance).x, 2))
        {
            airDirectionRef(__instance).x = movementDirection2Ref(__instance).x;
        }
        else
        {
            airDirectionRef(__instance).x = 0f;
        }

        if (__instance.rb.velocity.z * movementDirection2Ref(__instance).z < Mathf.Pow(movementDirection2Ref(__instance).z, 2))
        {
            airDirectionRef(__instance).z = movementDirection2Ref(__instance).z;
        }
        else
        {
            airDirectionRef(__instance).z = 0f;
        }
        __instance.rb.AddForce(airDirectionRef(__instance).normalized * __instance.airAcceleration);
    }

    public static IEnumerable<CodeInstruction> MoveTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
    {
        var codeMatcher = new CodeMatcher(instructions, ilGenerator);
         var frictionlessFramesField = AccessTools.Field(typeof(QuakeMovement), nameof(frictionlessFrames));

        TranspileAirMovement(ref codeMatcher, frictionlessFramesField);
        Log("hell yea");
        TranspileGroundMovement(ref codeMatcher, frictionlessFramesField);

        return codeMatcher.InstructionEnumeration();
    }

    private static void TranspileAirMovement(ref CodeMatcher codeMatcher, FieldInfo frictionlessFramesField)
    {
        /* // rb.useGravity = true;
         * IL_01e8: ret
         * IL_01e9: ldarg.0
         * IL_01ea: ldfld class [UnityEngine.PhysicsModule]UnityEngine.Rigidbody NewMovement::rb
         * IL_01ef: ldc.i4.1
         * IL_01f0: callvirt instance void [UnityEngine.PhysicsModule]UnityEngine.Rigidbody::set_useGravity(bool)
         */
        codeMatcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ret),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(NewMovement), "rb")),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(Rigidbody), "useGravity"))
            )
            .ThrowIfInvalid("Couldn't find `rb.useGravity = true`")
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0) /*{ labels = codeMatcher.Labels }*/,
                new CodeInstruction(OpCodes.Call, ((Delegate)NewAirMove).Method),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(QuakeMovement), nameof(sv_maxfrictionlessframes))),
                new CodeInstruction(OpCodes.Stsfld, frictionlessFramesField),
                new CodeInstruction(OpCodes.Ret)
            )
            .RemoveInstructions(codeMatcher.Remaining);
    }

    private static void TranspileGroundMovement(ref CodeMatcher codeMatcher, FieldInfo frictionlessFramesField)
    {

        /* // if (gc.onGround && friction > 0f && !jumping)
         * IL_0060: ldarg.0
         * IL_0061: ldfld class GroundCheck NewMovement::gc
         * IL_0066: ldfld bool GroundCheck::onGround
         * IL_006b: brfalse IL_01e9
         *
         * IL_0070: ldarg.0
         * IL_0071: ldfld float32 NewMovement::friction
         * IL_0076: ldc.r4 0.0
         * IL_007b: ble.un IL_01e9
         *
         * IL_0080: ldarg.0
         * IL_0081: ldfld bool NewMovement::jumping
         * IL_0086: brtrue IL_01e9
         */
        codeMatcher
            .Start()
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(NewMovement), "gc")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GroundCheck), "onGround")),
                new CodeMatch(OpCodes.Brfalse),

                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(NewMovement), "friction")),
                new CodeMatch(OpCodes.Ldc_R4, 0f),
                new CodeMatch(OpCodes.Ble_Un),

                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(NewMovement), "jumping")),
                new CodeMatch(OpCodes.Brtrue)
            )
            .ThrowIfInvalid("Could not find `if (gc.onGround && friction > 0f && !jumping)`")
            .Advance(1)
            .CreateLabel(out var groundMoveLabel)

            .Insert( // if (frictionlessFrames > 0) then decrement it and return
                new CodeInstruction(OpCodes.Ldsfld, frictionlessFramesField),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Cgt_Un),
                new CodeInstruction(OpCodes.Brfalse_S, groundMoveLabel),
                new CodeInstruction(OpCodes.Ldsfld, frictionlessFramesField),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Stsfld, frictionlessFramesField),
                new CodeInstruction(OpCodes.Ret)
            );
    }
}