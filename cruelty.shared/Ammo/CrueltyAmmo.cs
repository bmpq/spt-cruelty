using System.Collections;
using TMPro;
using UnityEngine;

namespace tarkin.cruelty.shared.Ammo
{
    public class CrueltyAmmo : MonoBehaviour
    {
        [SerializeField] private TMP_Text textMag;
        [SerializeField] private TMP_Text textInventory;

        public void SetAmmo(int mag, int inventory)
        {
            textMag.text = mag.ToString();
            textInventory.text = inventory.ToString();
        }
    }
}