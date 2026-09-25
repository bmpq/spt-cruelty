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
        static void PatchPostfix(ref AvailableInteractionState __result, GamePlayerOwner owner, IInteractive interactive)
        {
            if (interactive is Corpse corpse)
            {
                __result.Actions.Add(new InteractionAction()
                {
                    Name = "Consume",
                    Action = () =>
                    {
                        corpse.Kill();
                        owner.Player.ActiveHealthController.ChangeEnergy(1f);
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.Head, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.LeftLeg, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.RightLeg, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.LeftArm, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.RightArm, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.Chest, 1f, new DamageInfo());
                        owner.Player.ActiveHealthController.ChangeHealth(EBodyPart.Stomach, 1f, new DamageInfo());
                    },
                    Disabled = false,
                    TargetName = null
                });
            }
        }
    }
}
