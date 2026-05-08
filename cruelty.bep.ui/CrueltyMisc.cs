using System;
using UnityEngine;

using EFT.UI;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyMisc : IDisposable
    {
        GameObject _activateSoftware;

        Transform _eftStancePanel;
        Vector2 _eftStancePanelOriginalPosition;
        Transform _eftAmmoPanel;
        Vector2 _eftAmmoPanelOriginalPosition;
        Transform _eftQuickAccessPanel;
        Vector2 _eftQuickAccessPanelOriginalPosition;

        public CrueltyMisc(CommonUI commonUI, GameObject prefabActivateSoftware)
        {
            _eftStancePanel = commonUI.EftBattleUIScreen.transform.Find("BattleStancePanel");
            _eftStancePanelOriginalPosition = _eftStancePanel.RectTransform().anchoredPosition;
            _eftStancePanel.RectTransform().anchoredPosition = new Vector2(-200, 0);

            _eftAmmoPanel = commonUI.EftBattleUIScreen.transform.Find("AmmoPanel");
            _eftAmmoPanelOriginalPosition = _eftAmmoPanel.RectTransform().anchoredPosition;
            _eftAmmoPanel.RectTransform().anchoredPosition = new Vector2(0, -200);

            _eftQuickAccessPanel = commonUI.EftBattleUIScreen.transform.Find("QuickAccessPanel");
            _eftQuickAccessPanelOriginalPosition = _eftQuickAccessPanel.RectTransform().anchoredPosition;
            _eftQuickAccessPanel.RectTransform().anchoredPosition = new Vector2(0, 200);

            _activateSoftware = GameObject.Instantiate(prefabActivateSoftware, commonUI.EftBattleUIScreen.transform.parent);
            _activateSoftware.SetActive(UnityEngine.Random.value > 0.992f);
        }

        public void Dispose()
        {
            _eftStancePanel.RectTransform().anchoredPosition = _eftStancePanelOriginalPosition;
            _eftAmmoPanel.RectTransform().anchoredPosition = _eftAmmoPanelOriginalPosition;
            _eftQuickAccessPanel.RectTransform().anchoredPosition = _eftQuickAccessPanelOriginalPosition;

            GameObject.Destroy(_activateSoftware.gameObject);
        }
    }
}
