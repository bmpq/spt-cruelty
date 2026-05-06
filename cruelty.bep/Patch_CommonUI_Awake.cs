using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.cruelty.bep
{
    internal partial class Plugin // partial class so only Plugin has access to the patch event
    {
        private class Patch_CommonUI_Awake : ModulePatch
        {
            public static event Action<CommonUI> OnAwake;

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
}
