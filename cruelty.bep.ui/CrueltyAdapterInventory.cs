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
        public CrueltyAdapterInventory(CommonUI commonUI, Sprite spriteGridCell, Sprite spriteUnsearchedFill)
        {
            Patch_GridView_Awake.spriteGridCell = spriteGridCell;
            Patch_GridItemView_Init.spriteUnsearchedFill = spriteUnsearchedFill;
        }

        public void Dispose()
        {
            Patch_GridView_Awake.spriteGridCell = null;
            Patch_GridItemView_Init.spriteUnsearchedFill = null;
        }

        private class Patch_GridView_Awake : ModulePatch
        {
            public static Sprite spriteGridCell;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GridView), nameof(GridView.Show));
            }

            [PatchPostfix]
            private static void PatchPostfix(GridView __instance)
            {
                Transform tile = __instance.transform.Find("Background Tile");
                if (tile != null && tile.TryGetComponent<Image>(out var image))
                {
                    image.sprite = spriteGridCell;
                }
            }
        }

        private class Patch_GridItemView_Init : ModulePatch
        {
            public static Sprite spriteUnsearchedFill;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.Init));
            }

            [PatchPostfix]
            private static void PatchPostfix(GridItemView __instance)
            {
                Transform unsearchedBackground = __instance.transform.Find("UnsearchedBackground");
                if (unsearchedBackground != null && unsearchedBackground.TryGetComponent<Image>(out var image))
                {
                    image.sprite = spriteUnsearchedFill;
                    image.color = Color.white;
                }
            }
        }
    }
}
