using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace tarkin.cruelty.world
{
    internal class Patch_LevelSettings_OnPreCullCallback : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LevelSettings), nameof(LevelSettings.method_0));
        }

        [PatchPrefix]
        private static bool PatchPrefix(LevelSettings __instance)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientSkyColor = Color.white * 0.5f;

            if (Singleton<TOD_Sky>.Instantiated)
            {

            }

            return false;
        }
    }

    internal class Patch_TOD_Sky_UpdateCelestials : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TOD_Sky), nameof(TOD_Sky.method_18));
        }

        [PatchPostfix]
        private static void PatchPostfix(TOD_Sky __instance)
        {
            __instance.LightObject.intensity = 1f;
        }
    }
}
