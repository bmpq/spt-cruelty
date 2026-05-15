using System;
using UnityEngine;
using UnityEngine.UI;

using EFT;
using EFT.UI;
using EFT.HealthSystem;

using tarkin.cruelty.shared.Health;
using Comfort.Common;
using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;

namespace tarkin.cruelty.bep.ui
{
    public class CrueltyAdapterHealth : IDisposable, IUnityUpdateReceiver, IPlayerOwnerDependent
    {

        readonly Transform _eftCharacterHealthPanel;
        readonly Transform _eftBodyParts;
        readonly Transform _eftBodyPartsBackground;

        readonly Transform _eftEffectsPanel;
        readonly Vector2 _effectsPanelOriginalPos;

        readonly CrueltyHealth _crueltyHealth;
        readonly Image _imageFlash;
        readonly AudioClip _clipEat;

        GamePlayerOwner _playerOwner;

        public CrueltyAdapterHealth(CommonUI commonUI, GameObject prefabHealth, AudioClip clipEat, GamePlayerOwner playerOwner)
        {
            _eftCharacterHealthPanel = commonUI.EftBattleUIScreen.transform.Find("CharacterHealthPanel");

            _eftBodyParts = _eftCharacterHealthPanel.Find("BodyParts");
            _eftBodyPartsBackground = _eftCharacterHealthPanel.Find("Background");
            _eftEffectsPanel = _eftCharacterHealthPanel.Find("EffectsPanel");

            _effectsPanelOriginalPos = _eftEffectsPanel.RectTransform().anchoredPosition;
            _eftEffectsPanel.RectTransform().anchoredPosition = new Vector2(570f, -250f);

            _eftBodyParts.gameObject.SetActive(false);
            _eftBodyPartsBackground.gameObject.SetActive(false);

            _crueltyHealth = GameObject.Instantiate(prefabHealth, _eftCharacterHealthPanel).GetComponent<CrueltyHealth>();

            _imageFlash = new GameObject("Cruelty Flash", typeof(RectTransform)).AddComponent<Image>();
            _imageFlash.transform.parent = commonUI.EftBattleUIScreen.transform.parent;
            _imageFlash.transform.localScale = Vector3.one;
            _imageFlash.rectTransform.anchorMin = Vector2.zero;
            _imageFlash.rectTransform.anchorMax = Vector2.one;
            _imageFlash.rectTransform.offsetMin = Vector2.zero;
            _imageFlash.rectTransform.offsetMax = Vector2.zero;
            _imageFlash.color = Color.clear;
            _imageFlash.transform.SetAsFirstSibling();

            _clipEat = clipEat;

            _playerOwner = playerOwner;

            Patch_ActiveHealthController_ChangeEnergy.OnPostfix += OnChangeEnergy;
        }

        private void OnChangeEnergy(float diff)
        {
            if (diff > 0)
                Singleton<GUISounds>.Instance.PlaySound(_clipEat);
        }

        public void ChangePlayerOwner(GamePlayerOwner owner)
        {
            _playerOwner = owner;
        }

        public void Update()
        {
            if (_playerOwner == null || _playerOwner.Player == null || _playerOwner.Player.ActiveHealthController == null)
                return;

            ValueStruct currentHealth = _playerOwner.Player.ActiveHealthController.GetBodyPartHealth(EBodyPart.Common, rounded: true);
            _crueltyHealth.SetHealth(currentHealth.Current, currentHealth.Maximum);
        }

        public void Dispose()
        {
            Patch_ActiveHealthController_ChangeEnergy.OnPostfix -= OnChangeEnergy;

            _eftBodyParts.gameObject.SetActive(true);
            _eftBodyPartsBackground.gameObject.SetActive(true);
            _eftEffectsPanel.RectTransform().anchoredPosition = _effectsPanelOriginalPos;

            GameObject.Destroy(_crueltyHealth.gameObject);

            GameObject.Destroy(_imageFlash.gameObject);
        }

        private class Patch_ActiveHealthController_ChangeEnergy : ModulePatch
        {
            public static event Action<float> OnPostfix;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.ChangeEnergy));
            }

            [PatchPostfix]
            private static void PatchPostfix(ActiveHealthController __instance)
            {
                if (!__instance.Player.IsYourPlayer)
                    return;

                float diff = __instance.HealthValue_0.LastDiff; // _energy
                if (!diff.IsZero())
                    OnPostfix?.Invoke(diff);
            }
        }
    }
}