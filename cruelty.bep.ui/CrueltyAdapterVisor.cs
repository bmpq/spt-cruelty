using System;
using UnityEngine;

using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterVisor : IDisposable
    {
        readonly Texture2D _textureBiosuit;

        public CrueltyAdapterVisor(Texture2D textureBiosuit)
        {
            _textureBiosuit = textureBiosuit;

            if (_textureBiosuit == null)
            {
                Plugin.Logger.LogError("missing biosuit texture");
                return;
            }

            Patch_CameraManager_SetCamera.OnPostfix += OnCameraChanged;
            if (CameraManager.Exist)
                OnCameraChanged(CameraManager.Instance);
        }

        void OnCameraChanged(CameraManager cameraManager)
        {
            if (cameraManager.Camera == null)
                return;

            if (cameraManager.Camera.TryGetComponent<VisorEffect>(out var visorEffect))
            {
                visorEffect.Scratches = _textureBiosuit;
                visorEffect.ScratcesIntensity = 1f;

                foreach (var mask in visorEffect.VisorMasks)
                {
                    mask.Texture = null;
                }

                visorEffect.OnEnable();
            }
        }

        public void Dispose()
        {

        }

        private class Patch_CameraManager_SetCamera : ModulePatch
        {
            public static event Action<CameraManager> OnPostfix;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(CameraManager), nameof(CameraManager.SetCamera));
            }

            [PatchPostfix]
            private static void PatchPostfix(CameraManager __instance, Camera camera)
            {
                OnPostfix?.Invoke(__instance);
            }
        }
    }
}
