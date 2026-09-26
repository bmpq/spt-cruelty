using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using tarkin.cruelty.fika;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tarkin.cruelty.grappendix
{
    [BepInPlugin("com.tarkin.cruelty.player", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        internal static ConfigEntry<KeyboardShortcut> KeybindGrapple;
        internal static ConfigEntry<bool> FallDamageImmunity;

        // not declaring exact type so an assembly traverser doesn't crash the game when fika is not installed
        private IDisposable _fika;

        void Start()
        {
            Logger = new EFTLogger("cruelty.player", () =>
            #if DEBUG
                true
            #else
                false
            #endif
            );
            BepInEx.Logging.Logger.Sources.Add(Logger);

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            KeybindGrapple = Config.Bind("Keybinds", "Keybind Grapple", new KeyboardShortcut(KeyCode.V));
            FallDamageImmunity = Config.Bind("", "FallDamageImmunity", false);

            if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
                _fika = new FikaHandler(Logger);

            Patch_GameWorld_OnGameStarted.OnPostfix += Init;
            if (Singleton<AbstractGame>.Instantiated)
            {
                Init(Singleton<GameWorld>.Instance);

#if DEBUG
                Singleton<GameWorld>.Instance.MainPlayer.ActiveHealthController.ChangeEnergy(99);
                Singleton<GameWorld>.Instance.MainPlayer.ActiveHealthController.ChangeHydration(99);
#endif
            }
        }

        void Init(GameWorld gameWorld)
        {
            if (_fika != null && ((FikaHandler)_fika).IsHeadless())
                return;
                
            Player mainPlayer = gameWorld.MainPlayer;
            PlayerGrapplingController controller = mainPlayer.gameObject.AddComponent<PlayerGrapplingController>();
            controller.Init(mainPlayer);
            if (_fika != null && _fika is FikaHandler fikaHandler)
            {
                controller.OnShot += fikaHandler.SendGrappleShot;
                controller.OnHit += fikaHandler.SendGrappleHit;
                controller.OnPivotChanged += fikaHandler.SendGrapplePivotChanged;
                controller.OnRetract += fikaHandler.SendGrappleRetract;

                fikaHandler.OnRemoteGrappleShot += (player, dir, speed) 
                    => GetOrAddObservedGrapplingController(player).OnShotReceived(dir, speed);
                fikaHandler.OnRemoteGrappleHit += (player, point) 
                    => GetOrAddObservedGrapplingController(player).OnHitReceived(point);
                fikaHandler.OnRemoteGrapplePivotChanged += (player, point) 
                    => GetOrAddObservedGrapplingController(player).OnPivotChangedReceived(point);
                fikaHandler.OnRemoteGrappleRetract += (player, speed) 
                    => GetOrAddObservedGrapplingController(player).OnRetractReceived(speed);

                ObservedGrapplingController GetOrAddObservedGrapplingController(Player player)
                {
                    if (!player.TryGetComponent<ObservedGrapplingController>(out var observedController))
                    {
                        observedController = player.gameObject.AddComponent<ObservedGrapplingController>();
                        observedController.Init(player);
                    }
                    return observedController;
                }
            }

            LevelBorders.Disable();
        }

        void OnDestroy()
        {
            Patch_GameWorld_OnGameStarted.OnPostfix -= Init;

            FindObjectsByTypeAndDestroy<PlayerGrapplingController>();
            FindObjectsByTypeAndDestroy<ObservedGrapplingController>();

            _patchManager.DisablePatches();
            _patchManager = null;

            _fika?.Dispose();

            BepInEx.Logging.Logger.Sources.Remove(Logger);
        }

        static void FindObjectsByTypeAndDestroy<T>() where T : Component
        {
            foreach (var item in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Component.Destroy(item);
            }
        }

        private class Patch_GameWorld_OnGameStarted : ModulePatch
        {
            public static event Action<GameWorld> OnPostfix;
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
            }

            [PatchPostfix]
            private static void PatchPostfix(GameWorld __instance)
            {
                OnPostfix?.Invoke(__instance);
            }
        }
    }
}
