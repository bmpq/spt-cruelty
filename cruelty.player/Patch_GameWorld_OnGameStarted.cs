using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace tarkin.cruelty.player
{
    internal class Patch_GameWorld_OnGameStarted : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            Player mainPlayer = __instance.MainPlayer;
            mainPlayer.gameObject.AddComponent<PlayerGrapplingController>();
        }
    }
}
