using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyKillFeed : IDisposable, IPlayerOwnerDependent
    {
        private GamePlayerOwner _playerOwner;

        public CrueltyKillFeed(GamePlayerOwner playerOwner)
        {
            Patch_Player_OnBeenKilledByAggressor.OnPostfix += OnBeenKilledByAggressor;

            ChangePlayerOwner(playerOwner);
        }

        public void ChangePlayerOwner(GamePlayerOwner playerOwner)
        {
            _playerOwner = playerOwner;
        }

        private void OnBeenKilledByAggressor(IPlayer aggressor, IPlayer victim)
        {
            if (_playerOwner.Player.Id != aggressor.Id)
                return;

            NotificationManager.DisplayMessageNotification($"{victim.Profile.Nickname} eliminated");
        }

        public void Dispose()
        {
            Patch_Player_OnBeenKilledByAggressor.OnPostfix -= OnBeenKilledByAggressor;
        }

        private class Patch_Player_OnBeenKilledByAggressor : ModulePatch
        {
            public delegate void OnKill(IPlayer aggressor, IPlayer victim);
            public static event OnKill OnPostfix;
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Player), nameof(Player.OnBeenKilledByAggressor));
            }

            [PatchPostfix]
            private static void PatchPostfix(Player __instance, IPlayer aggressor)
            {
                OnPostfix?.Invoke(aggressor, __instance);
            }
        }
    }
}
