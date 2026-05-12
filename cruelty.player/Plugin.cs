using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using tarkin.cruelty.fika;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [BepInPlugin("com.tarkin.cruelty.player", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        internal static ConfigEntry<KeyboardShortcut> KeybindGrapple;
        internal static ConfigEntry<bool> FallDamageImmunity;

        private GameObject _prefabGrappendixVisual;
        private IDisposable _fika;

        void Start()
        {
            Logger = new EFTLogger("crPl", () => true);
            BepInEx.Logging.Logger.Sources.Add(Logger);

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            KeybindGrapple = Config.Bind("Keybinds", "Keybind Grapple", new KeyboardShortcut(KeyCode.G));
            FallDamageImmunity = Config.Bind("", "FallDamageImmunity", false);

            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-world");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            GameObject[] allPrefabs = bundle.LoadAllAssets<GameObject>();
            _prefabGrappendixVisual = allPrefabs.First(p => p.name == "GrappendixVisual");
            bundle.Unload(false);

            Patch_GameWorld_OnGameStarted.OnPostfix += Init;
            if (Singleton<GameWorld>.Instantiated)
            {
                Init(Singleton<GameWorld>.Instance);
            }

            if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
                _fika = new FikaHandler(_prefabGrappendixVisual);
        }

        void Init(GameWorld gameWorld)
        {
            Player mainPlayer = gameWorld.MainPlayer;
            PlayerGrapplingController controller = mainPlayer.gameObject.AddComponent<PlayerGrapplingController>();
            controller.Init(mainPlayer, _prefabGrappendixVisual);
            if (_fika != null && _fika is FikaHandler fikaHandler)
            {
                controller.OnShot += fikaHandler.SendGrappleShot;
                controller.OnHit += fikaHandler.SendGrappleHit;
                controller.OnPivotChanged += fikaHandler.SendGrapplePivotChanged;
                controller.OnRetract += fikaHandler.SendGrappleRetract;
            }
        }

        void OnDestroy()
        {
            Patch_GameWorld_OnGameStarted.OnPostfix -= Init;

            FindObjectsByTypeAndDestroy<PlayerGrapplingController>();

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
