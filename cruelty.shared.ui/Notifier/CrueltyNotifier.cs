using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace tarkin.cruelty.shared.ui.Notifier
{
    public class CrueltyNotifier : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private CrueltyNotification prefabText;

        private Queue<CrueltyNotification> pool = new Queue<CrueltyNotification>();

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                Pop("this is a test message");
        }
#endif

        public void Pop(string message, float duration = 5f)
        {
            CrueltyNotification notification = GetFromPool();

            notification.transform.SetAsFirstSibling();

            notification.Show(message, duration, () => ReturnToPool(notification));
        }

        private CrueltyNotification GetFromPool()
        {
            if (pool.Count > 0)
            {
                CrueltyNotification instance = pool.Dequeue();
                instance.gameObject.SetActive(true);
                return instance;
            }

            CrueltyNotification newInstance = Instantiate(prefabText, container);
            return newInstance;
        }

        private void ReturnToPool(CrueltyNotification notification)
        {
            notification.gameObject.SetActive(false);
            pool.Enqueue(notification);
        }
    }
}