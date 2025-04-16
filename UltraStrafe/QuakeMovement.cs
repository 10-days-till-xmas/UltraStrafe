using HarmonyLib;
using UnityEngine;

namespace UltraStrafe;

internal static class QuakeMovement
{
    private static float wishspeed;
    private static int frictionlessFrames = 0;

    // Field References
    private static readonly AccessTools.FieldRef<NewMovement, bool> slideEndingRef =
        AccessTools.FieldRefAccess<NewMovement, bool>("slideEnding");
    private static readonly AccessTools.FieldRef<NewMovement, float> hurtInvincibilityRef =
        AccessTools.FieldRefAccess<NewMovement, float>("hurtInvincibility");
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> movementDirectionRef =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("movementDirection");
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> movementDirection2Ref =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("movementDirection2");
    private static readonly AccessTools.FieldRef<NewMovement, Vector3> airDirectionRef =
        AccessTools.FieldRefAccess<NewMovement, Vector3>("airDirection");
    private static readonly AccessTools.FieldRef<NewMovement, float> frictionRef =
        AccessTools.FieldRefAccess<NewMovement, float>("friction");

    public static float sv_accelerate = ConfigManager.sv_accelerate.Value;
    public static float sv_maxspeed = ConfigManager.sv_maxspeed.Value;
    public static int sv_maxfrictionlessframes = ConfigManager.sv_maxfrictionlessframes.Value;
    public static float sv_switchspeed = ConfigManager.sv_switchspeed.Value;
    public static bool sv_acceltweak = ConfigManager.sv_acceltweak.Value;
    private static void NewGroundMove(NewMovement __instance)
    {
        // note: this is the same as the original ground move. Try removing this and use a transpiler instead
        // TODO: test out if implementing the quake ground move would work better, along with its friction too
        float y = __instance.rb.velocity.y;
        if (__instance.slopeCheck.onGround && movementDirectionRef(__instance).x == 0f && movementDirectionRef(__instance).z == 0f)
        {
            y = 0f;
            __instance.rb.useGravity = false;
        }
        else
        {
            __instance.rb.useGravity = true;
        }

        float slowModeNum = __instance.slowMode ? 1.25f : 2.75f;

        if ((bool)__instance.groundProperties)
        {
            slowModeNum *= __instance.groundProperties.speedMultiplier;
        }
        float speedScale = __instance.walkSpeed * Time.deltaTime * slowModeNum;
        
        movementDirection2Ref(__instance) = new Vector3(movementDirectionRef(__instance).x * speedScale, y, movementDirectionRef(__instance).z * speedScale);
        Vector3 vector = __instance.pushForce;
        if ((bool)__instance.groundProperties && __instance.groundProperties.push)
        {
            Vector3 vector2 = __instance.groundProperties.pushForce;
            if (__instance.groundProperties.pushDirectionRelative)
            {
                vector2 = __instance.groundProperties.transform.rotation * vector2;
            }

            vector += vector2;
        }

        __instance.rb.velocity = Vector3.Lerp(
            __instance.rb.velocity,
            movementDirection2Ref(__instance) + vector,
            0.25f * frictionRef(__instance)
        );

    }

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

        var speed = new Vector2(__instance.rb.velocity.x, __instance.rb.velocity.z).magnitude;

        if (speed > sv_switchspeed)
        {
            // quake air move
            QuakeAirMove(__instance);
        }
        else
        {
            // TODO: use transpilation so this block isnt needed
            
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
    }

    public static void NewMove(NewMovement __instance)
    {
        slideEndingRef(__instance) = false;
        if (hurtInvincibilityRef(__instance) <= 0f && !__instance.levelOver)
        {
            __instance.gameObject.layer = 2;
            __instance.exploded = false;
        }

        if (__instance.gc.onGround && !__instance.jumping)
        {
            __instance.currentWallJumps = 0;
            __instance.rocketJumps = 0;
            __instance.hammerJumps = 0;
            __instance.rocketRides = 0;
        }

        //ground friction and acceleration
        if (__instance.gc.onGround && frictionRef(__instance) > 0f && !__instance.jumping)
        {
            if (frictionlessFrames == 0)
                NewGroundMove(__instance);
            else
                frictionlessFrames--;
        }
        else // air acceleration
        {
            NewAirMove(__instance);
            frictionlessFrames = sv_maxfrictionlessframes;
        }
    }
}