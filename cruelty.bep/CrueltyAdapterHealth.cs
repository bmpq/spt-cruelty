using System;
using UnityEngine;

using EFT.UI;
using tarkin.cruelty.shared.Health;

namespace tarkin.cruelty.bep
{
    public class CrueltyAdapterHealth : IDisposable
    {
        readonly Transform _eftCharacterHealthPanel;
        readonly Transform _eftBodyParts;

        readonly Transform _eftEffectsPanel;
        readonly Vector2 _effectsPanelOriginalPos;

        readonly CrueltyHealth _crueltyHealth;

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
        }

        public void Dispose()
        {
            _eftBodyParts.gameObject.SetActive(true);
            _eftEffectsPanel.RectTransform().anchoredPosition = _effectsPanelOriginalPos;

            GameObject.Destroy(_crueltyHealth.gameObject);
        }
    }
}