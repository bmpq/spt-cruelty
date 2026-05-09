using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using Diz.Utils;
using EFT;
using EFT.Ballistics;
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
using UnityEngine.Rendering;

namespace tarkin.cruelty.world
{
    [BepInPlugin("com.tarkin.cruelty.world", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        List<IDisposable> _disposables = new List<IDisposable>();

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            Patch_GameWorld_OnGameStarted.OnPostfix += OnGameStarted;
            if (Singleton<GameWorld>.Instantiated)
            {
                OnGameStarted(Singleton<GameWorld>.Instance);
            }
        }

        private void OnGameStarted(GameWorld world)
        {
            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-world");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            try
            {
                string path = FindAssetPath(bundle, "MaterialTextureMapping.asset");
                Logger.LogWarning(path);
                MaterialTextureMapping asset = bundle.LoadAsset<MaterialTextureMapping>(path);
                Logger.LogWarning(asset.name);

                Process(asset);

                foreach (var light in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (light.type == LightType.Directional)
                    {
                        light.shadows = LightShadows.None;
                        continue;
                    }
                    light.enabled = false;
                }

                foreach (var light in FindObjectsByType<BaseLight>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    light.enabled = false;
                }

                DestroyAll<StencilShadow>();
                DestroyAll<AnalyticSource>();
                DestroyAll<StaticDeferredDecal>();

                DisableCameraPostProcessing();

                foreach (var group in FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    group.ForceLOD(group.lodCount - 1);
                }
            }
            catch (Exception ex) { Logger.LogError(ex); }

            bundle.Unload(false);
        }

        void DisableCameraPostProcessing()
        {
            if (!CameraClass.Exist || CameraClass.Instance.Camera == null)
                return;

            Camera mainCamera = CameraClass.Instance.Camera;

            mainCamera.GetComponent<PrismEffects>().enabled = false;
            mainCamera.GetComponent<CC_Vintage>().enabled = false;
            mainCamera.GetComponent<CC_Sharpen>().enabled = false;
        }

        static void DestroyAll<T>() where T : MonoBehaviour
        {
            foreach (var item in FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                Destroy(item.gameObject);
            }
        }


        public static void Process(MaterialTextureMapping mapping)
        {
            var allRends = Component.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Shader diffuseShader = Shader.Find("Diffuse");

            foreach (var rend in allRends)
            { 
                if (rend.gameObject.scene.buildIndex < 0)
                    continue;

                var mats = rend.sharedMaterials;

                if (rend.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                {
                    Component.Destroy(rend);
                    continue;
                }

                foreach (var mat in mats)
                {
                    if (mat == null)
                        continue;

                    mat.name = mat.name.Replace(" (Instance)", "");

                    if (mat.name.Contains("puddle"))
                    {
                        Component.Destroy(rend);
                        break;
                    }

                    if (mat.shader.name.Contains("Transparent"))
                    {
                        mat.mainTexture = null;
                        mat.color = new Color(0.7f, 0f, 0.7f, 0.5f);
                        continue;
                    }

                    mat.shader = diffuseShader;

                    mat.mainTexture = mapping.GetMainTexture(mat);
                }

                rend.sharedMaterials = mats;
            }
        }

        private string FindAssetPath(AssetBundle bundle, string fileName)
        {
            foreach (var path in bundle.GetAllAssetNames())
            {
                Logger.LogInfo(path);
            }

            string target = "/" + fileName.ToLowerInvariant();
            string foundPath = bundle.GetAllAssetNames().FirstOrDefault(path => path.ToLowerInvariant().EndsWith(target));

            if (foundPath == null)
                Logger.LogError($"Could not find asset ending with {fileName} in bundle!");

            return foundPath;
        }

        void OnDestroy()
        {
            Patch_GameWorld_OnGameStarted.OnPostfix -= OnGameStarted;

            foreach (var item in _disposables)
            {
                item.Dispose();
            }

            _patchManager.DisablePatches();
            _patchManager = null;
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
