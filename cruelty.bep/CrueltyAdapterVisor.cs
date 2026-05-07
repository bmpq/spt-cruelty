using System;
using UnityEngine;

using EFT;
using EFT.UI;

namespace tarkin.cruelty.bep
{
    internal class CrueltyAdapterVisor : IDisposable
    {
        readonly Texture2D _textureBiosuit;

        public CrueltyAdapterVisor(AssetBundle bundle)
        {
            _textureBiosuit = bundle.LoadAsset<Texture2D>("Packages/com.tarkin.cruelty.shared/Border/CR_terrorsuit.png");

            if (_textureBiosuit == null)
            {
                Plugin.Logger.LogError("missing biosuit texture");
                return;
            }

            CameraClass.Instance.OnCameraChanged += OnCameraChanged;
            if (CameraClass.Instance.Camera != null)
                OnCameraChanged();
        }

        private void OnCameraChanged()
        {
            if (CameraClass.Instance.Camera.TryGetComponent<VisorEffect>(out var visorEffect))
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
            CameraClass.Instance.OnCameraChanged -= OnCameraChanged;
        }
    }
}
