using System.Collections;
using TMPro;
using UnityEngine;

namespace tarkin.cruelty.shared.Ammo
{
    public class CrueltyAmmo : MonoBehaviour
    {
        [SerializeField] private TMP_Text textMag;
        [SerializeField] private TMP_Text textInventory;

        [SerializeField] private RectTransform spinner;
        [SerializeField] private float spinAddAmount = -2000f;
        [SerializeField] private float spinDecayRate = 3f;

        private float _spinSpeed;

        public void SetAmmo(int mag, int inventory)
        {
            textMag.text = mag.ToString();
            textInventory.text = inventory.ToString();
        }

        public void Spin()
        {
            _spinSpeed += spinAddAmount;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Spin();
            }
#endif

            _spinSpeed = Mathf.Lerp(_spinSpeed, 0f, Time.deltaTime * spinDecayRate);

            if (!Mathf.Approximately(_spinSpeed, 0))
                spinner.Rotate(new Vector3(0, 0, 1), _spinSpeed * Time.deltaTime);
        }
    }
}