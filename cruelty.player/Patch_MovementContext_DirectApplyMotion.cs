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
            originalSpeedLimit = __instance.CharacterController.SpeedLimit;

            var grapplingComp = ____player.GetComponent<PlayerGrapplingController>();

            if (grapplingComp == null || !grapplingComp.IsGrappling) return;

            Vector3 target = grapplingComp.GrappleTarget;
            Vector3 direction = (target - __instance.TransformPosition).normalized;

            float grappleSpeed = 30f;

            motion = direction * grappleSpeed * deltaTime;

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
