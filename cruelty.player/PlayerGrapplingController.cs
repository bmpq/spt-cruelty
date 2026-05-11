using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Systems.Effects;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [DefaultExecutionOrder(100)] // probably not necessary
    public class PlayerGrapplingController : MonoBehaviour
    {
        private static PlayerGrapplingController instance;
        private Player _player;

        private GrappleState _state;

        private Vector3 grappleTarget;
        private float ropeLength;

        private float maxGrappleDistance = 70f;

        private float hookSpeed = 120f;
        private Vector3 currentHookPos;
        private Vector3 hookDirection;
        private float distanceTraveled;

        private bool _shouldMuteMovementSounds;

        private Transform cam;
        private GrappendixVisual _visual;

        public void Init(Player player, GameObject prefabGrappendixVisual)
        {
            instance = this;

            _player = player;
            cam = _player.PlayerBones.HeadCameraCollider.transform;

            _visual = Instantiate(prefabGrappendixVisual).GetComponent<GrappendixVisual>();

            StopGrapple();
        }

        void Update()
        {
            if (_player == null || !_player.IsYourPlayer || !_player.ActiveHealthController.IsAlive)
            {
                Destroy(this);
                return;
            }    

            HandleInput();

            Vector3 playerPoint = _player.PlayerBones.Pelvis.Original.position;

            if (_state == GrappleState.Seeking)
            {
                float step = hookSpeed * Time.deltaTime;

                bool tipHit = Physics.Raycast(currentHookPos, hookDirection, out RaycastHit tipHitInfo, step, LayerMaskController.HighPolyWithTerrainMask | LayerMaskController.TransparentLayerMask);

                Vector3 nextHookPos = tipHit ? tipHitInfo.point : currentHookPos + (hookDirection * step);

                Vector3 ropeDir = nextHookPos - playerPoint;
                float ropeDist = ropeDir.magnitude;

                bool ropeHit = Physics.Raycast(playerPoint, ropeDir.normalized, out RaycastHit ropeHitInfo, ropeDist, LayerMaskController.HighPolyWithTerrainMask | LayerMaskController.TransparentLayerMask);

                if (ropeHit)
                {
                    currentHookPos = ropeHitInfo.point;

                    StartGrapple(currentHookPos);

                    if (Singleton<Effects>.Instantiated)
                        Singleton<Effects>.Instance.EmitBloodOnEnvironment(ropeHitInfo.point, ropeHitInfo.normal);
                }
                else if (tipHit)
                {
                    currentHookPos = tipHitInfo.point;

                    StartGrapple(currentHookPos);

                    if (Singleton<Effects>.Instantiated)
                        Singleton<Effects>.Instance.EmitBloodOnEnvironment(tipHitInfo.point, tipHitInfo.normal);
                }
                else
                {
                    currentHookPos = nextHookPos;
                    distanceTraveled += step;

                    if (distanceTraveled >= maxGrappleDistance)
                    {
                        StopGrapple();
                    }
                }

                if (_state == GrappleState.Seeking)
                {
                    _visual.SetPoints(currentHookPos, playerPoint);
                }
            }

            if (_state == GrappleState.Taut)
            {
                float distToTarget = Vector3.Distance(playerPoint, grappleTarget);

                if (Physics.Raycast(playerPoint, (grappleTarget - playerPoint).normalized, out RaycastHit hit, distToTarget, LayerMaskController.HighPolyWithTerrainMask | LayerMaskController.TransparentLayerMask))
                {
                    grappleTarget = hit.point;
                }

                _player.MovementContext.ResetFlying();
                _player.MovementContext.FreefallTime = 0;

                if (_player.MovementContext.CurrentState is JumpPlayerState jumpState)
                {
                    _player.MovementContext.PlayerAnimatorEnableJump(enabled: false);
                    _player.MovementContext.PlayerAnimatorEnableLanding(enabled: true);
                }

                _visual.SetPoints(grappleTarget, playerPoint);
            }

            if (_state == GrappleState.Retracting)
            {
                currentHookPos = Vector3.MoveTowards(currentHookPos, playerPoint, Time.deltaTime * hookSpeed * 0.5f);
                _visual.SetPoints(currentHookPos, playerPoint);
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(Plugin.KeybindGrapple.Value.MainKey))
            {
                StartHookFlight();
            }

            if (Input.GetKeyUp(Plugin.KeybindGrapple.Value.MainKey))
            {
                StopGrapple();
            }
        }

        private void StartHookFlight()
        {
            hookDirection = cam.forward;
            currentHookPos = cam.position;
            distanceTraveled = 0f;

            _state = GrappleState.Seeking;
            _visual.SetState(GrappleState.Seeking);
        }

        private void StartGrapple(Vector3 point)
        {
            _state = GrappleState.Taut;

            grappleTarget = point;

            ropeLength = Vector3.Distance(_player.Transform.position, point);

            _shouldMuteMovementSounds = true;

            _player.MovementContext.PlayerAnimator.Animator.Play("Sprint", 0, 0f); // skip land stumble if grapple start mid air

            _visual.SetState(_state);
        }

        private void ApplyGrapplePhysics(MovementContext context, ref Vector3 motion, float deltaTime)
        {
            context.CharacterController.SpeedLimit = -1f;

            Vector3 currentPos = context.TransformPosition;
            Vector3 toTarget = this.grappleTarget - currentPos;
            float currentDistance = toTarget.magnitude;
            Vector3 directionToTarget = toTarget.normalized;

            Vector3 currentVelocity = context.Velocity;
            currentVelocity += Physics.gravity * deltaTime;

            float reelSpeed = 25f;
            currentVelocity += directionToTarget * reelSpeed * deltaTime;

            if (currentDistance > this.ropeLength)
            {
                float stiffness = 23f;
                float damping = 4f;

                // F = -k * x
                float stretch = currentDistance - this.ropeLength;
                Vector3 springForce = directionToTarget * (stretch * stiffness);
                Vector3 dampingForce = -currentVelocity * damping;

                currentVelocity += (springForce + dampingForce) * deltaTime;
            }

            float drag = (1f - (0.5f * deltaTime));
            currentVelocity *= drag;

            motion = currentVelocity * deltaTime;
        }

        private void StopGrapple()
        {
            if (_state == GrappleState.Taut)
            {
                ApplyExitMomentum(_player.Velocity);
            }

            _state = GrappleState.Retracting;

            _shouldMuteMovementSounds = false;
            _visual.SetState(_state);
        }

        void ApplyExitMomentum(Vector3 momentum)
        {
            _player.MovementContext.InputMotionBeforeLimit = momentum;

            if (_player.MovementContext.CurrentState is not SprintStateClass)
            {
                // JumpPlayerState to apply momentum checks if previous state was Sprint
                _player.MovementContext.PlayerAnimator.Animator.Play("Sprint", 0, 0f);
                _player.MovementContext.PlayerAnimator.Animator.Update(0.1f); // triggers all bsg state change logic immediately (to write MovementContext.PreviousState)
            }

            _player.MovementContext.PlayerAnimator.Animator.Play("Jump_Move", 0, 0f);
        }

        void OnDestroy()
        {
            if (_visual != null)
                Destroy(_visual.gameObject);

            if (instance == this)
                instance = null;
        }

        private class Patch_MovementContext_DirectApplyMotion : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.DirectApplyMotion));
            }

            [PatchPrefix]
            static void Prefix(MovementContext __instance, ref Vector3 motion, float deltaTime, Player ____player)
            {
                if (instance == null || instance._player != ____player || instance._state != GrappleState.Taut)
                    return;

                instance.ApplyGrapplePhysics(__instance, ref motion, deltaTime);
            }
        }

        private static bool ShouldMuteMovementSounds(Player player)
        {
            if (instance == null || instance._player != player)
                return false;
            return instance._shouldMuteMovementSounds;
        }

        private class Patch_Player_StateChangedHandler : ModulePatch
        {
            protected override MethodBase GetTargetMethod() 
                => AccessTools.Method(typeof(Player), nameof(Player.method_55));
            [PatchPrefix] private static bool PatchPrefix(Player __instance)
                => !ShouldMuteMovementSounds(__instance);
        }

        private class Patch_Player_PlayTurnSound : ModulePatch
        {
            protected override MethodBase GetTargetMethod() 
                => AccessTools.Method(typeof(Player), nameof(Player.method_62));
            [PatchPrefix] static bool PatchPrefix(Player __instance) 
                => !ShouldMuteMovementSounds(__instance);
        }
    }
}