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

namespace tarkin.cruelty.player
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
            Logger = new EFTLogger("cruelty.player", () => true);
            BepInEx.Logging.Logger.Sources.Add(Logger);

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            KeybindGrapple = Config.Bind("Keybinds", "Keybind Grapple", new KeyboardShortcut(KeyCode.G));
            FallDamageImmunity = Config.Bind("", "FallDamageImmunity", false);

            if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
                _fika = new FikaHandler(Logger);

            Patch_GameWorld_OnGameStarted.OnPostfix += Init;
            if (Singleton<GameWorld>.Instantiated)
            {
                Init(Singleton<GameWorld>.Instance);
            }
        }

        void Init(GameWorld gameWorld)
        {
            if (_fika != null && ((FikaHandler)_fika).IsHeadless())
                return;
                
            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-world");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            GameObject[] allPrefabs = bundle.LoadAllAssets<GameObject>();
            GameObject prefabGrappendixVisual = allPrefabs.First(p => p.name == "GrappendixVisual");
            bundle.Unload(false);

            Player mainPlayer = gameWorld.MainPlayer;
            PlayerGrapplingController controller = mainPlayer.gameObject.AddComponent<PlayerGrapplingController>();
            controller.Init(mainPlayer, prefabGrappendixVisual);
            if (_fika != null && _fika is FikaHandler fikaHandler)
            {
                controller.OnShot += fikaHandler.SendGrappleShot;
                controller.OnHit += fikaHandler.SendGrappleHit;
                controller.OnPivotChanged += fikaHandler.SendGrapplePivotChanged;
                controller.OnRetract += fikaHandler.SendGrappleRetract;

                fikaHandler.OnRemoteGrappleShot += (player, dir, speed) 
                    => GetOrAddObservedGrapplingController(player, prefabGrappendixVisual).OnShotReceived(dir, speed);
                fikaHandler.OnRemoteGrappleHit += (player, point) 
                    => GetOrAddObservedGrapplingController(player, prefabGrappendixVisual).OnHitReceived(point);
                fikaHandler.OnRemoteGrapplePivotChanged += (player, point) 
                    => GetOrAddObservedGrapplingController(player, prefabGrappendixVisual).OnPivotChangedReceived(point);
                fikaHandler.OnRemoteGrappleRetract += (player, speed) 
                    => GetOrAddObservedGrapplingController(player, prefabGrappendixVisual).OnRetractReceived(speed);

                ObservedGrapplingController GetOrAddObservedGrapplingController(Player player, GameObject prefab)
                {
                    if (!player.TryGetComponent<ObservedGrapplingController>(out var observedController))
                    {
                        observedController = player.gameObject.AddComponent<ObservedGrapplingController>();
                        observedController.Init(player, prefab);
                    }
                    return observedController;
                }
            }

            RemoveMapPlayerBlockers();
        }

        void RemoveMapPlayerBlockers()
        {
            HashSet<string> targetNames = ["Custom_LevelBorders"];

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var go in scene.GetRootGameObjects())
                {
                    if (targetNames.Contains(go.name))
                        go.SetActive(false);
                }
            }
            
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
