using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.player
{
    internal class PlayerGrapplingAudioManager : IDisposable
    {
        private static readonly FieldInfo Field_Player__soundBySurface = AccessTools.Field(typeof(Player), "_soundBySurface");

        private static PlayerGrapplingAudioManager instance;

        private readonly Player _player;
        private readonly SurfaceSet wetSurfaceSet;

        public bool shouldMuteMovementSounds { get; set; }

        public PlayerGrapplingAudioManager(Player player)
        {
            instance = this;
            _player = player;

            var allSurfaces = Field_Player__soundBySurface.GetValue(player) as Dictionary<BaseBallistic.ESurfaceSound, SurfaceSet>;
            wetSurfaceSet = allSurfaces[BaseBallistic.ESurfaceSound.Puddle];
        }

        public void PlayHitAudio(Vector3 point)
        {
            if (!Singleton<BetterAudio>.Instantiated || !CameraManager.Exist || CameraManager.Instance.Camera == null)
                return;

            Singleton<BetterAudio>.Instance.PlayAtPoint(point, wetSurfaceSet.LandingSoundBank, CameraManager.Instance.Distance(point));
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
