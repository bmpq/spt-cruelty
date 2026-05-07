using System;
using UnityEngine;

using EFT;
using EFT.UI;

namespace tarkin.cruelty.bep
{
    internal class CrueltySixthSense : IDisposable, IPlayerOwnerDependent, IUnityUpdateReceiver
    {
        readonly GameObject _crueltyEye;

        GamePlayerOwner _playerOwner;

        public CrueltySixthSense(CommonUI commonUI, AssetBundle bundle, GamePlayerOwner gamePlayerOwner) 
        {
            GameObject prefab = bundle.LoadAsset<GameObject>("Packages/com.tarkin.cruelty.shared/SixthSense/CrueltySixthSense.prefab");
            _crueltyEye = GameObject.Instantiate(prefab, commonUI.EftBattleUIScreen.transform);
            _crueltyEye.SetActive(false);

            ChangePlayerOwner(gamePlayerOwner);
        }

        public void ChangePlayerOwner(GamePlayerOwner playerOwner)
        {
            _playerOwner = playerOwner;
        }

        public void Update()
        {
            if (_playerOwner == null || _playerOwner.Player == null)
                return;

            bool shouldShowEye = false;

            foreach (var player in _playerOwner.Player.GameWorld.AllAlivePlayersList)
            {
                if (!player.IsAI)
                    continue;

                EnemyInfo enemyInfo = player.AIData.BotOwner.Memory?.GoalEnemy;

                if (enemyInfo == null)
                    continue;

                if (!enemyInfo.IsVisible)
                    continue;

                if (enemyInfo.Person == null || !enemyInfo.Person.IsYourPlayer)
                    continue;

                shouldShowEye = true;
                break;
            }

            _crueltyEye.SetActive(shouldShowEye);
        }

        public void Dispose()
        {
            GameObject.Destroy(_crueltyEye.gameObject);
        }
    }
}
