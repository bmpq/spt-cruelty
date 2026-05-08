using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace tarkin.cruelty.shared.ui.Notifier
{
    public class CrueltyNotification : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private Coroutine animationCoroutine;

        public void Show(string message, float duration, Action onComplete)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(AnimateRoutine(message, duration, onComplete));
        }

        private IEnumerator AnimateRoutine(string message, float duration, Action onComplete)
        {
            float animDuration = 0.2f;
            float elapsed = 0f;

            if (!string.IsNullOrEmpty(message))
            {
                while (elapsed < animDuration)
                {
                    elapsed += Time.deltaTime;
                    int charCount = Mathf.FloorToInt(Mathf.Lerp(0, message.Length, elapsed / animDuration));
                    text.text = message.Substring(0, charCount);
                    yield return null;
                }
            }
            text.text = message;

            yield return new WaitForSeconds(duration);

            elapsed = 0f;
            if (!string.IsNullOrEmpty(message))
            {
                while (elapsed < animDuration)
                {
                    elapsed += Time.deltaTime;
                    int charCount = Mathf.FloorToInt(Mathf.Lerp(message.Length, 0, elapsed / animDuration));
                    text.text = message.Substring(0, charCount);
                    yield return null;
                }
            }
            text.text = "";

            onComplete?.Invoke();
        }
    }
}