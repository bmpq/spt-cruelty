using System;
using UnityEngine;

using EFT;
using EFT.UI;
using EFT.InventoryLogic;

using tarkin.cruelty.shared.Ammo;

namespace tarkin.cruelty.bep
{
    internal class CrueltyAdapterAmmo : IDisposable, IPlayerOwnerDependent
    {
        readonly CrueltyAmmo _crueltyAmmo;

        private GamePlayerOwner _playerOwner = null;

        public CrueltyAdapterAmmo(CommonUI commonUI, AssetBundle bundle, GamePlayerOwner playerOwner)
        {
            GameObject prefab = bundle.LoadAsset<GameObject>("Packages/com.tarkin.cruelty.shared/Ammo/CrueltyAmmo.prefab");
            _crueltyAmmo = GameObject.Instantiate(prefab, commonUI.EftBattleUIScreen.transform).GetComponent<CrueltyAmmo>();

            ChangePlayerOwner(playerOwner);
        }

        public void ChangePlayerOwner(GamePlayerOwner playerOwner)
        {
            if (_playerOwner != null && _playerOwner.Player != null)
            {
                Player_OnHandsControllerChanged(_playerOwner.Player.HandsController, null);
                _playerOwner.Player.OnHandsControllerChanged -= Player_OnHandsControllerChanged;
            }

            _playerOwner = playerOwner;

            if (playerOwner == null || playerOwner.Player == null)
                return;

            playerOwner.Player.OnHandsControllerChanged += Player_OnHandsControllerChanged;

            Player_OnHandsControllerChanged(null, playerOwner.Player.HandsController);
        }

        private void Player_OnHandsControllerChanged(Player.AbstractHandsController prevHands, Player.AbstractHandsController currentHands)
        {
            if (prevHands != null && prevHands is Player.FirearmController prevFirearmController)
            {
                prevFirearmController.OnShot -= OnShot;
            }

            if (currentHands is Player.FirearmController firearmController && firearmController.Weapon != null)
            {
                firearmController.OnShot += OnShot;

                _crueltyAmmo.gameObject.SetActive(true);

                SetAmmoCounter(firearmController.Weapon);
            }
            else
            {
                _crueltyAmmo.gameObject.SetActive(false);
            }
        }

        private void OnShot()
        {
            SetAmmoCounter((_playerOwner.Player.HandsController as Player.FirearmController).Weapon);
        }

        private void SetAmmoCounter(Weapon weapon)
        {
            int chamberAmmoCount = weapon.ChamberAmmoCount;

            Magazine mag = weapon.GetCurrentMagazine();
            if (mag != null)
            {
                int currentMagAmmo = mag.Count;
                int maxMagAmmo = mag.MaxCount;

                _crueltyAmmo.SetAmmo(currentMagAmmo + chamberAmmoCount, 0);
            }
            else
            {
                _crueltyAmmo.SetAmmo(chamberAmmoCount, 0);
            }
        }

        public void Dispose()
        {
            ChangePlayerOwner(null);

            GameObject.Destroy(_crueltyAmmo.gameObject);
        }
    }
}
