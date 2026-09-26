using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace tarkin.cruelty.grappendix
{
    internal class Patch_ActiveHealthController_ApplyDamage : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.ApplyDamage));
        }

        [PatchPrefix]
        private static void PatchPrefix(ActiveHealthController __instance, ref float damage, DamageInfo damageInfo)
        {
            if (!Plugin.FallDamageImmunity.Value)
                return;

            if (!__instance.Player.IsYourPlayer)
                return;

            if (damageInfo.DamageType == EDamageType.Fall)
                damage *= 0;
        }
    }
}
