using System;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;
using TMPro;

namespace tarkin.cruelty.bep
{
    internal class CrueltyIndiscriminateFontReplacer : IDisposable
    {
        readonly TMP_FontAsset _font;
        readonly TMP_FontAsset _font2;

        public CrueltyIndiscriminateFontReplacer(TMP_FontAsset font, TMP_FontAsset font2)
        {
            _font = font;
            _font2 = font2;
            Patch_TextMeshProUGUI_Awake.OnPostfix += Patch_TextMeshProUGUI_Awake_OnPostfix;
        }

        private void Patch_TextMeshProUGUI_Awake_OnPostfix(TextMeshProUGUI text)
        {
            if (text.font != _font2)
                text.font = _font;
        }

        public void Dispose()
        {
            Patch_TextMeshProUGUI_Awake.OnPostfix -= Patch_TextMeshProUGUI_Awake_OnPostfix;
        }

        private class Patch_TextMeshProUGUI_Awake : ModulePatch
        {
            public static event Action<TextMeshProUGUI> OnPostfix;
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(TextMeshProUGUI), "Awake");
            }

            [PatchPostfix]
            private static void PatchPostfix(TextMeshProUGUI __instance)
            {
                OnPostfix?.Invoke(__instance);
            }
        }
    }
}
