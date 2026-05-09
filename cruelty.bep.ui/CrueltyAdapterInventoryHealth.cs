using System;
using UnityEngine;

using EFT.UI;
using EFT.UI.Health;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterInventoryHealth : IDisposable
    {
        readonly GameObject _eftSilhouette;

        readonly GameObject _character;

        public CrueltyAdapterInventoryHealth(CommonUI commonUI, GameObject prefab)
        {
            InventoryScreenHealthPanel healthPanel = commonUI.InventoryScreen.GetComponentInChildren<InventoryScreenHealthPanel>(true);

            _eftSilhouette = healthPanel.transform.Find("Silhouette").gameObject;
            _eftSilhouette.SetActive(false);

            _character = GameObject.Instantiate(prefab, healthPanel.transform);
            _character.transform.SetAsFirstSibling();
        }

        public void Dispose()
        {
            _eftSilhouette.SetActive(true);

            if (_character != null)
                GameObject.Destroy(_character.gameObject);
        }
    }
}
