using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [BepInPlugin("com.tarkin.cruelty.player", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            Patch_GameWorld_OnGameStarted.OnPostfix += Init;
            if (Singleton<GameWorld>.Instantiated)
            {
                Init(Singleton<GameWorld>.Instance);
            }
        }

        void Init(GameWorld gameWorld)
        {
            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-world");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            GameObject[] allPrefabs = bundle.LoadAllAssets<GameObject>();
            GameObject prefabGrappendixVisual = allPrefabs.First(p => p.name == "GrappendixVisual");

            Player mainPlayer = gameWorld.MainPlayer;
            mainPlayer.gameObject.AddComponent<PlayerGrapplingController>().Init(mainPlayer, prefabGrappendixVisual);

            bundle.Unload(false);
        }

        void OnDestroy()
        {
            Patch_GameWorld_OnGameStarted.OnPostfix -= Init;

            FindObjectsByTypeAndDestroy<PlayerGrapplingController>();

            _patchManager.DisablePatches();
            _patchManager = null;
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
