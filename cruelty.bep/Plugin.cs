using BepInEx;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    [BepInPlugin("com.tarkin.cruelty", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        static readonly FieldInfo Field_BattleUIScreen__controller = AccessTools.Field(typeof(EftBattleUIScreen), "gparam_0");

        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        List<IDisposable> _disposables = new List<IDisposable>();

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            Patch_CommonUI_Awake.OnAwake += Load;
            if (MonoBehaviourSingleton<CommonUI>.Instantiated)
            {
                Load(MonoBehaviourSingleton<CommonUI>.Instance);
            }

            Patch_EftBattleUIScreen_Show.OnShow += OnPlayerOwnerChanged;
        }

        void Load(CommonUI commonUI)
        {
            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-ui");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            GameObject[] allPrefabs = bundle.LoadAllAssets<GameObject>();

            GameObject prefabHealth = allPrefabs.First(p => p.name == "CrueltyHealth");
            GameObject prefabAmmo = allPrefabs.First(p => p.name == "CrueltyAmmo");
            GameObject prefabPointer = allPrefabs.First(p => p.name == "CrueltyPointer");
            GameObject prefabBorder = allPrefabs.First(p => p.name == "CrueltyBorder");
            GameObject prefabSixthSense = allPrefabs.First(p => p.name == "CrueltySixthSense");
            GameObject prefabActivateSoftware = allPrefabs.First(p => p.name == "ActivateSoftware");
            GameObject prefabNotifier = allPrefabs.First(p => p.name == "CrueltyNotifier");

            Texture2D textureBiosuit = bundle.LoadAllAssets<Texture2D>().First(t => t.name == "CR_terrorsuit");

            TMP_FontAsset font = bundle.LoadAllAssets<TMP_FontAsset>().First(f => f.name == "XanhMono-Regular SDF");
            TMP_FontAsset font2 = bundle.LoadAllAssets<TMP_FontAsset>().First(f => f.name == "gamefont RASTER");

            GamePlayerOwner currentBattleUIPlayerOwner = (Field_BattleUIScreen__controller.GetValue(commonUI.EftBattleUIScreen) as EftBattleUIScreenController)?.Owner;

            _disposables.Add(new CrueltyAdapterHealth(commonUI, prefabHealth, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltyAdapterAmmo(commonUI, prefabAmmo, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltyAdapterPointer(commonUI, prefabPointer));
            _disposables.Add(new CrueltyAdapterVisor(textureBiosuit));
            _disposables.Add(new CrueltyAdapterBorder(commonUI, prefabBorder, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltySixthSense(commonUI, prefabSixthSense, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltyMisc(commonUI, prefabActivateSoftware));
            _disposables.Add(new CrueltyIndiscriminateFontReplacer(font, font2));
            _disposables.Add(new CrueltyAdapterNotifier(prefabNotifier, MonoBehaviourSingleton<PreloaderUI>.Instance));

            bundle.Unload(false);
        }

        void OnPlayerOwnerChanged(GamePlayerOwner playerOwner)
        {
            foreach (var item in _disposables)
            {
                if (item is IPlayerOwnerDependent dependent)
                {
                    dependent.ChangePlayerOwner(playerOwner);
                }
            }
        }

        void Update()
        {
            foreach (var item in _disposables)
            {
                if (item is IUnityUpdateReceiver receiver)
                {
                    receiver.Update();
                }
            }

#if DEBUG
            if (Input.GetKeyDown(KeyCode.C))
            {
                NotificationManager.DisplayMessageNotification("test notifoacitno");
            }
#endif
        }

        void OnDestroy()
        {
            Patch_EftBattleUIScreen_Show.OnShow -= OnPlayerOwnerChanged;
            Patch_CommonUI_Awake.OnAwake -= Load;

            foreach (var item in _disposables)
            {
                item.Dispose();
            }

            _patchManager.DisablePatches();
            _patchManager = null;
        }

        private class Patch_CommonUI_Awake : ModulePatch
        {
            public static event Action<CommonUI> OnAwake;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(CommonUI), nameof(CommonUI.Awake));
            }

            [PatchPostfix]
            private static void PatchPostfix(CommonUI __instance)
            {
                OnAwake?.Invoke(__instance);
            }
        }

        private class Patch_EftBattleUIScreen_Show : ModulePatch
        {
            public static event Action<GamePlayerOwner> OnShow;

            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(EftBattleUIScreen), nameof(EftBattleUIScreen.Show), [typeof(GamePlayerOwner)]);
            }

            [PatchPostfix]
            private static void PatchPostfix(EftBattleUIScreen __instance, GamePlayerOwner owner)
            {
                OnShow?.Invoke(owner);
            }
        }
    }
}
