using EFT;
using EFT.Counters;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    internal class CrueltyAdapterBorder : IDisposable, IPlayerOwnerDependent
    {
        readonly CrueltyBorder _crueltyBorder;

        public CrueltyAdapterBorder(CommonUI commonUI, AssetBundle bundle, GamePlayerOwner playerOwner)
        {
            GameObject prefab = bundle.LoadAsset<GameObject>("Packages/com.tarkin.cruelty.shared/Border/CrueltyBorder.prefab");

            _crueltyBorder = GameObject.Instantiate(prefab, commonUI.EftBattleUIScreen.transform).GetComponent<CrueltyBorder>();
            _crueltyBorder.SetBorder(CrueltyBorder.BorderType.None);

            ChangePlayerOwner(playerOwner);
        }

        public void ChangePlayerOwner(GamePlayerOwner playerOwner)
        {
            Player player = playerOwner?.Player;
            if (player == null)
            {
                _crueltyBorder.SetBorder(CrueltyBorder.BorderType.None);
                return;
            }

            CounterTag counterTag = ((player.Profile.Info.Side == EPlayerSide.Savage) ? CounterTag.Scav : CounterTag.Pmc);
            int winStreak = player.Profile.EftStats.OverallCounters.GetInt(CounterTag.CurrentWinStreak, counterTag);

            if (winStreak > 0)
            {
                _crueltyBorder.SetBorder(CrueltyBorder.BorderType.DivineLight);
            }
            else
            {
                _crueltyBorder.SetBorder(CrueltyBorder.BorderType.FleshAutomaton);
            }
        }

        public void Dispose()
        {
            GameObject.Destroy(_crueltyBorder.gameObject);
        }
    }
}
