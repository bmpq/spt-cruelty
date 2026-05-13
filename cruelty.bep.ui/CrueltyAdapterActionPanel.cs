using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterActionPanel : IDisposable
    {
        public CrueltyAdapterActionPanel(CommonUI commonUI)
        {

        }

        public void Dispose()
        {
        }

        private class Patch_ActionPanel_Show : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ActionPanel), nameof(ActionPanel.Start));
            }

            [PatchPostfix]
            private static void PatchPostfix(ActionPanel __instance)
            {
                foreach (var item in __instance.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    item.fontSize = 33;
                }
            }
        }

        private class Patch_InteractionButton_Show : ModulePatch
        {
            static readonly Color ColorSelected = new Color(1, 1, 1, 1);
            static readonly Color ColorNormal = new Color(0, 0, 0, 1);
            static readonly Color ColorDisabled = new Color(0, 0, 1, 1);
            
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(InteractionButton), nameof(InteractionButton.Show));
            }

            [PatchPrefix]
            private static bool PatchPrefix(InteractionButton __instance,
                AvailableInteractionState interaction, InteractionAction action,
                UIParent ___UI,
                CustomTextMeshProUGUI ____text, Image ____image)
            {
                __instance.ShowGameObject();

                ____text.fontSize = 33;
                ____text.text = action.Name.Localized().ToUpper();
                ___UI.BindEvent(interaction.CurrentActionChanged, delegate
                {
                    bool isSelected = interaction.SelectedAction == action;
                    ____image.color = (isSelected ? ColorSelected : ColorNormal);
                    ____text.color = (isSelected ? ColorNormal : (action.Disabled ? ColorDisabled : ColorSelected));
                });

                return false;
            }
        }
    }
}
