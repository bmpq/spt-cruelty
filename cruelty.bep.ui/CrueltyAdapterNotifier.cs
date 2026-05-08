using System;
using System.Reflection;
using UnityEngine;

using HarmonyLib;
using SPT.Reflection.Patching;

using EFT.UI;
using EFT.Communications;

using tarkin.cruelty.shared.ui.Notifier;
using Comfort.Common;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterNotifier : IDisposable
    {
        readonly CrueltyNotifier _crueltyNotifier;

        NotificationManager _notificationManager;

        public CrueltyAdapterNotifier(GameObject prefab, PreloaderUI preloaderUI)
        {
            Patch_NotificationManager_Activate.OnPostfix += OnNotificationManagerActivate;
            if (Singleton<NotificationManager>.Instantiated)
                OnNotificationManagerActivate(Singleton<NotificationManager>.Instance);

            Transform parentCanvas = preloaderUI.transform.GetChild(0);

            _crueltyNotifier = GameObject.Instantiate(prefab, parentCanvas).GetComponent<CrueltyNotifier>();
        }

        void OnNotificationManagerActivate(NotificationManager notificationManager)
        {
            if (_notificationManager != null)
                _notificationManager.OnNotificationReceived -= OnNotificationReceived;
            _notificationManager = notificationManager;
            notificationManager.OnNotificationReceived += OnNotificationReceived;
        }

        private void OnNotificationReceived(Notification notification)
        {
            float duration = 3f;

            switch (notification.Duration)
            {
                case ENotificationDurationType.Infinite:
                    duration = float.PositiveInfinity;
                    break;
                case ENotificationDurationType.Long:
                    duration *= 2f;
                    break;
            }

            _crueltyNotifier.Pop(notification.Description.Transliterate(), duration);
        }

        public void Dispose()
        {
            _notificationManager.OnNotificationReceived -= OnNotificationReceived;
            Patch_NotificationManager_Activate.OnPostfix -= OnNotificationManagerActivate;
            GameObject.Destroy(_crueltyNotifier.gameObject);
        }

        private class Patch_NotificationManager_Activate : ModulePatch
        {
            public static event Action<NotificationManager> OnPostfix;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(NotificationManager),
                    nameof(NotificationManager.Activate), [typeof(IBackEndSession), typeof(ENotificationTransportType)]);
            }

            [PatchPostfix]
            private static void PatchPostfix(NotificationManager __instance)
            {
                OnPostfix?.Invoke(__instance);
            }
        }

        private class Patch_NotifierView_ShowNotification : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(NotifierView),
                    nameof(NotifierView.method_1));
            }

            [PatchPrefix]
            private static bool PatchPrefix(NotificationManager __instance)
            {
                return false;
            }
        }
    }
}
