using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tarkin.cruelty.shared.Health
{
    public class CrueltyHealth : MonoBehaviour
    {
        [SerializeField] private TMP_Text textHP;
        [SerializeField] private Graphic coloredGraphic;
        [SerializeField] private Color colorFull;
        [SerializeField] private Color colorLow;

        public void SetHealth(float current, float max)
        {
            textHP.text = current.ToString("F0");

            float healthPercentage = (float)current / max;
            coloredGraphic.color = Color.Lerp(colorLow, colorFull, healthPercentage);
        }
    }
}