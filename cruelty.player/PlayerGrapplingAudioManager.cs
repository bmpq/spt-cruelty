using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace tarkin.cruelty.player
{
    internal class PlayerGrapplingAudioManager : IDisposable
    {
        private static PlayerGrapplingAudioManager instance;

        readonly Player _player;

        public bool shouldMuteMovementSounds { get; set; }

        public PlayerGrapplingAudioManager(Player player)
        {
            instance = this;
            _player = player;
        }

        public void Dispose()
        {
            if (instance == this)
                instance = null;
        }

        private static bool ShouldMuteMovementSounds(Player player)
        {
            if (instance == null || instance._player != player)
                return false;
            return instance.shouldMuteMovementSounds;
        }

        private class Patch_Player_StateChangedHandler : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
                => AccessTools.Method(typeof(Player), nameof(Player.method_55));
            [PatchPrefix]
            private static bool PatchPrefix(Player __instance)
                => !ShouldMuteMovementSounds(__instance);
        }

        private class Patch_Player_PlayTurnSound : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
                => AccessTools.Method(typeof(Player), nameof(Player.method_62));
            [PatchPrefix]
            static bool PatchPrefix(Player __instance)
                => !ShouldMuteMovementSounds(__instance);
        }
    }
}
