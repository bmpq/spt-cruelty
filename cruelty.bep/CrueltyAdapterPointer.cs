using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using tarkin.cruelty.shared.Pointer;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    internal class CrueltyAdapterPointer : IDisposable
    {
        readonly static FieldInfo Field_ActionPanel__pointer = AccessTools.Field(typeof(ActionPanel), "_pointer");

        readonly UIPointer _eftPointer;

        readonly CrueltyPointer _crueltyPointer;

        bool _sensing;
        bool _actionAvailable;

        public CrueltyAdapterPointer(CommonUI commonUI, GameObject prefabPointer)
        {
            Patch_ActionPanel_AvailableInteractionStateChangedHandler.OnPostfix += AvailableInteractionStateChanged;
            Patch_ActionPanel_ShowPointer.OnPostfix += OnSenseChange;

            _eftPointer = Field_ActionPanel__pointer.GetValue(commonUI.EftBattleUIScreen.ActionPanel) as UIPointer;
            _eftPointer.gameObject.SetActive(false);

            _crueltyPointer = GameObject.Instantiate(prefabPointer, commonUI.EftBattleUIScreen.ActionPanel.transform).GetComponent<CrueltyPointer>();

            _crueltyPointer.HideCursor();
        }

        void OnSenseChange(bool sensing)
        {
            _sensing = sensing;

            if (_actionAvailable)
                return;

            if (_sensing)
            {
                SetCursorSense();
            }
            else
            {
                HideCursor();
            }
        }

        private void AvailableInteractionStateChanged(AvailableInteractionState interactionState)
        {
            _actionAvailable = (interactionState != null && interactionState.Actions.Count > 0);

            if (_actionAvailable)
            {
                SetCursorGrab();
            }
            else
            {
                if (_sensing)
                    SetCursorSense();
                else
                    HideCursor();
            }
        }

        void SetCursorSense()
        {
            _crueltyPointer.SetCursorSense();
        }

        void SetCursorGrab()
        {
            _crueltyPointer.SetCursorGrab();
        }

        void HideCursor()
        {
            _crueltyPointer.HideCursor();
        }

        public void Dispose()
        {
            Patch_ActionPanel_AvailableInteractionStateChangedHandler.OnPostfix -= AvailableInteractionStateChanged;
            Patch_ActionPanel_ShowPointer.OnPostfix -= OnSenseChange;

            _eftPointer.gameObject.SetActive(true);

            GameObject.Destroy(_crueltyPointer.gameObject);
        }

        private class Patch_ActionPanel_AvailableInteractionStateChangedHandler : ModulePatch
        {
            public static event Action<AvailableInteractionState> OnPostfix;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ActionPanel), nameof(ActionPanel.method_0));
            }

            [PatchPostfix]
            private static void PatchPostfix(ActionPanel __instance, AvailableInteractionState interactionState)
            {
                OnPostfix?.Invoke(interactionState);
            }
        }

        private class Patch_ActionPanel_ShowPointer : ModulePatch
        {
            public static event Action<bool> OnPostfix;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ActionPanel), nameof(ActionPanel.ShowPointer));
            }

            [PatchPostfix]
            private static void PatchPostfix(ActionPanel __instance, bool b)
            {
                OnPostfix?.Invoke(b);
            }
        }
    }
}
