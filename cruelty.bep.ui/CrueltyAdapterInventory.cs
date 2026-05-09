using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterInventory : IDisposable
    {
        public CrueltyAdapterInventory(CommonUI commonUI, Sprite spriteGridCell)
        {
            Patch_GridView_Awake.sprite = spriteGridCell;
        }

        public void Dispose()
        {
            Patch_GridView_Awake.sprite = null;
        }

        private class Patch_GridView_Awake : ModulePatch
        {
            public static Sprite sprite;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GridView), nameof(GridView.Show));
            }

            [PatchPostfix]
            private static void PatchPostfix(GridView __instance)
            {
                __instance.transform.Find("Background Tile").GetComponent<Image>().sprite = sprite;
            }
        }
    }
}
