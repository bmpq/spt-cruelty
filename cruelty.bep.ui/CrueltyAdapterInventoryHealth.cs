using System;
using UnityEngine;

using EFT.UI;
using System.Reflection;
using HarmonyLib;
using EFT.UI.Health;

namespace tarkin.cruelty.bep.ui
{
    internal class CrueltyAdapterInventoryHealth : IDisposable
    {
        static readonly FieldInfo Field_InventoryScreen__itemsPanel = AccessTools.Field(typeof(InventoryScreen), "_itemsPanel");
        static readonly FieldInfo Field_ItemsPanel__healthPanel = AccessTools.Field(typeof(ItemsPanel), "_healthPanel");

        public CrueltyAdapterInventoryHealth(CommonUI commonUI, AssetBundle bundle)
        {
            //(Field_ItemsPanel__healthPanel.GetValue(Field_InventoryScreen__itemsPanel.GetValue(commonUI.InventoryScreen) as ItemsPanel) as InventoryScreenHealthPanel)
            //commonUI.InventoryScreen.transform.Find("Items Panel").Find("LeftSide")
        }

        public void Dispose()
        {

        }
    }
}
