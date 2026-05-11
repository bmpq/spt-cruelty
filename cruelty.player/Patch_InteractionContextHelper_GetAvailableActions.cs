using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace tarkin.cruelty.player
{
    internal class Patch_InteractionContextHelper_GetAvailableActions : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(
                typeof(InteractionContextHelper), 
                nameof(InteractionContextHelper.GetAvailableActions), 
                [typeof(GamePlayerOwner), typeof(IInteractive)]);

        [PatchPostfix]
        static void PatchPostfix(ref AvailableInteractionState __result, IInteractive interactive)
        {
            if (interactive is Corpse corpse)
            {
                __result.Actions.Add(new InteractionAction()
                {
                    Name = "Consume",
                    Action = () =>
                    {
                        corpse.Kill();
                    },
                    Disabled = false,
                    TargetName = null
                });
            }
        }
    }
}
