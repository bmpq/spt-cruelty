using UnityEngine;

namespace tarkin.cruelty.shared.Pointer
{
    public class CrueltyPointer : MonoBehaviour
    {
        [SerializeField] private GameObject pointerSense;
        [SerializeField] private GameObject pointerGrab;

        public void SetCursorSense()
        {
            pointerSense.SetActive(true);
            pointerGrab.SetActive(false);
        }

        public void SetCursorGrab()
        {
            pointerSense.SetActive(false);
            pointerGrab.SetActive(true);
        }

        public void HideCursor()
        {
            pointerSense.SetActive(false);
            pointerGrab.SetActive(false);
        }
    }
}