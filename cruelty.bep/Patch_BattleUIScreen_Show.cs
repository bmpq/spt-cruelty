using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.cruelty.bep
{
    internal class Patch_EftBattleUIScreen_Show : ModulePatch
    {
        public static event Action<GamePlayerOwner> OnShow;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EftBattleUIScreen), nameof(EftBattleUIScreen.Show), [typeof(GamePlayerOwner)]);
        }

        [PatchPostfix]
        private static void PatchPostfix(EftBattleUIScreen __instance, GamePlayerOwner owner)
        {
            OnShow?.Invoke(owner);
        }
    }
}
