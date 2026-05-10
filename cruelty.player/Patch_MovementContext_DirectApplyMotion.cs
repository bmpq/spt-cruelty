using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.player
{
    internal class Patch_MovementContext_DirectApplyMotion : ModulePatch
    {
        static float originalSpeedLimit;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.DirectApplyMotion));
        }

        [PatchPrefix]
        static void Prefix(MovementContext __instance, ref Vector3 motion, float deltaTime, Player ____player)
        {
            var grapplingComp = ____player.GetComponent<PlayerGrapplingController>();

            if (grapplingComp == null || !grapplingComp.IsGrappling) return;

            Vector3 currentPos = __instance.TransformPosition;
            Vector3 toTarget = grapplingComp.GrappleTarget - currentPos;
            float currentDistance = toTarget.magnitude;
            Vector3 directionToTarget = toTarget.normalized;

            Vector3 currentVelocity = grapplingComp.GrappleMomentum;

            currentVelocity += Physics.gravity * deltaTime;

            float reelSpeed = 25f;
            currentVelocity += directionToTarget * reelSpeed * deltaTime;

            if (currentDistance > grapplingComp.RopeLength)
            {
                float stiffness = 23f;
                float damping = 4f;

                // F = -k * x
                float stretch = currentDistance - grapplingComp.RopeLength;
                Vector3 springForce = directionToTarget * (stretch * stiffness);

                Vector3 dampingForce = -currentVelocity * damping;

                currentVelocity += (springForce + dampingForce) * deltaTime;
            }

            float drag = (1f - (0.5f * deltaTime));
            currentVelocity *= drag;

            grapplingComp.GrappleMomentum = currentVelocity;

            motion = currentVelocity * deltaTime;

            originalSpeedLimit = __instance.CharacterController.SpeedLimit;
            __instance.CharacterController.SpeedLimit = -1f;
        }

        [PatchPostfix]
        static void Postfix(MovementContext __instance, Player ____player)
        {
            var grapplingComp = ____player.GetComponent<PlayerGrapplingController>();
            if (grapplingComp == null || !grapplingComp.IsGrappling) return;

            __instance.CharacterController.SpeedLimit = originalSpeedLimit;
        }
    }
}
