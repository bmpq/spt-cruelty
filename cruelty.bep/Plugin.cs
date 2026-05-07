using BepInEx;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    [BepInPlugin("com.tarkin.cruelty", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        static readonly FieldInfo Field_BattleUIScreen__controller = AccessTools.Field(typeof(EftBattleUIScreen), "gparam_0");

        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        private AssetBundle _bundle;
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
            _bundle = AssetBundle.LoadFromFile(bundlePath);

            GamePlayerOwner currentBattleUIPlayerOwner = (Field_BattleUIScreen__controller.GetValue(commonUI.EftBattleUIScreen) as EftBattleUIScreenController)?.Owner;

            _disposables.Add(new CrueltyAdapterHealth(commonUI, _bundle, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltyAdapterAmmo(commonUI, _bundle, currentBattleUIPlayerOwner));
            _disposables.Add(new CrueltyAdapterPointer(commonUI, _bundle));
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
        }

        void OnDestroy()
        {
            Patch_EftBattleUIScreen_Show.OnShow -= OnPlayerOwnerChanged;
            Patch_CommonUI_Awake.OnAwake -= Load;

            foreach (var item in _disposables)
            {
                item.Dispose();
            }

            if (_bundle != null)
                _bundle.Unload(false);

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
