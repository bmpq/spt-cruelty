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

        int _consecutiveFramesDetected;
        const int ConsecutiveFramesDetectedThreshold = 2;

        public CrueltySixthSense(CommonUI commonUI, GameObject prefabSixthSense, GamePlayerOwner gamePlayerOwner) 
        {
            _crueltyEye = GameObject.Instantiate(prefabSixthSense, commonUI.EftBattleUIScreen.transform);
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

            bool detectedThisFrame = false;

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

                detectedThisFrame = true;
                break;
            }

            if (detectedThisFrame)
                _consecutiveFramesDetected++;
            else
                _consecutiveFramesDetected = 0;

            _crueltyEye.SetActive(_consecutiveFramesDetected > ConsecutiveFramesDetectedThreshold);
        }

        public void Dispose()
        {
            GameObject.Destroy(_crueltyEye.gameObject);
        }
    }
}
