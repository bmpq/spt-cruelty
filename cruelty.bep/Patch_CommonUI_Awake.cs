using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.cruelty.bep
{
    internal class Patch_CommonUI_Awake : ModulePatch
    {
        public static Action<CommonUI> OnAwake;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CommonUI), nameof(CommonUI.Awake));
        }

        [PatchPostfix]
        private static void PatchPostfix(CommonUI __instance)
        {
            OnAwake?.Invoke(__instance);
        }
    }
}
