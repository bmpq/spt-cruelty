using System;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

using EFT;
using EFT.UI;
using EFT.HealthSystem;

using tarkin.cruelty.shared.Health;

namespace tarkin.cruelty.bep
{
    public class CrueltyAdapterHealth : IDisposable, IUnityUpdateReceiver
    {
        static readonly FieldInfo Field_BattleUIScreen_player_0 = AccessTools.Field(typeof(EftBattleUIScreen), "player_0");

        readonly Transform _eftCharacterHealthPanel;
        readonly Transform _eftBodyParts;

        readonly Transform _eftEffectsPanel;
        readonly Vector2 _effectsPanelOriginalPos;

        readonly CrueltyHealth _crueltyHealth;

        Player _player;

        public CrueltyAdapterHealth(CommonUI commonUI, AssetBundle bundle)
        {
            _eftCharacterHealthPanel = commonUI.EftBattleUIScreen.transform.Find("CharacterHealthPanel");

            _eftBodyParts = _eftCharacterHealthPanel.Find("BodyParts");
            _eftEffectsPanel = _eftCharacterHealthPanel.Find("EffectsPanel");

            _effectsPanelOriginalPos = _eftEffectsPanel.RectTransform().anchoredPosition;
            _eftEffectsPanel.RectTransform().anchoredPosition = new Vector2(570f, -250f);

            _eftBodyParts.gameObject.SetActive(false);

            GameObject prefab = bundle.LoadAsset<GameObject>("Packages/com.tarkin.cruelty.shared/Health/CrueltyHealth.prefab");
            _crueltyHealth = GameObject.Instantiate(prefab, _eftCharacterHealthPanel).GetComponent<CrueltyHealth>();

            Patch_EftBattleUIScreen_Show.OnShow += OnPlayerChange;
            _player = Field_BattleUIScreen_player_0.GetValue(commonUI.EftBattleUIScreen) as Player;
        }

        void OnPlayerChange(GamePlayerOwner owner)
        {
            _player = owner.Player;
        }

        public void Update()
        {
            if (_player == null)
                return;

            ValueStruct currentHealth = _player.ActiveHealthController.GetBodyPartHealth(EBodyPart.Common, rounded: true);
            _crueltyHealth.SetHealth(currentHealth.Current, currentHealth.Maximum);
        }

        public void Dispose()
        {
            Patch_EftBattleUIScreen_Show.OnShow -= OnPlayerChange;

            _eftBodyParts.gameObject.SetActive(true);
            _eftEffectsPanel.RectTransform().anchoredPosition = _effectsPanelOriginalPos;

            GameObject.Destroy(_crueltyHealth.gameObject);
        }
    }
}