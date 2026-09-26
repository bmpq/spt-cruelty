using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using EFT.Ballistics;

namespace tarkin.cruelty.grappendix
{
    [DefaultExecutionOrder(100)] // probably not necessary
    public class PlayerGrapplingController : MonoBehaviour
    {
        private static PlayerGrapplingController GrapplingControllerInstance;
        private Player _player;

        private GrappleState _state;

        private readonly LayerMask collisionMask = LayersMaskController.HighPolyWithTerrainMask | LayersMaskController.TransparentLayerMask | LayersMaskController.PlayerMask;

        private Transform trackingTarget;
        private Vector3 grappleTarget;
        private float ropeLength;

        private float maxGrappleDistance = 80f;

        private float hookSpeed = 120f;
        private float retractSpeed = 70f;
        private float sphereCastRadius = 0.1f;
        private Vector3 currentHookPos;
        private Vector3 hookDirection;
        private float distanceTraveled;

        private PlayerMovementSoundMute _movementSoundMute;

        private Transform cam;
        private GrapplingView _view;

        public delegate void OnGrappleShot(Player player, Vector3 direction, float speed);
        public delegate void OnGrappleHit(Player player, Vector3 point);
        public delegate void OnGrapplePivotChanged(Player player, Vector3 point);
        public delegate void OnGrappleRetract(Player player, float speed);

        public event OnGrappleShot OnShot;
        public event OnGrappleHit OnHit;
        public event OnGrapplePivotChanged OnPivotChanged;
        public event OnGrappleRetract OnRetract;

        public void Init(Player player)
        {
            GrapplingControllerInstance = this;

            _player = player;
            cam = _player.PlayerBones.HeadCameraCollider.transform;

            _player.OnPlayerDead += OnPlayerDead;

            _view = new GrapplingView(player);

            _movementSoundMute = new PlayerMovementSoundMute(_player);
        }

        private void OnPlayerDead(Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part) => Destroy(this);

        void Update()
        {
            if (_player == null || !_player.IsYourPlayer)
            {
                Destroy(this);
                return;
            }    

            HandleInput();

            Vector3 playerPoint = _player.PlayerBones.Pelvis.Original.position;

            if (_state == GrappleState.Seeking)
            {
                float step = hookSpeed * Time.deltaTime;

                bool tipHit = Physics.SphereCast(currentHookPos, sphereCastRadius, hookDirection, out RaycastHit tipHitInfo, step, collisionMask);

                Vector3 nextHookPos = tipHit ? tipHitInfo.point : currentHookPos + (hookDirection * step);

                Vector3 ropeDir = nextHookPos - playerPoint;
                float ropeDist = ropeDir.magnitude;

                bool ropeHit = Physics.SphereCast(playerPoint, sphereCastRadius, ropeDir.normalized, out RaycastHit ropeHitInfo, ropeDist, collisionMask);

                if (ropeHit)
                {
                    if (ropeHitInfo.collider.TryGetComponent(out Player player))
                    {
                        trackingTarget = player.PlayerBones.Pelvis.Original;
                    }

                    currentHookPos = ropeHitInfo.point;

                    StartGrapple(currentHookPos, ropeHitInfo.normal);
                }
                else if (tipHit)
                {
                    if (tipHitInfo.collider.TryGetComponent(out Player player))
                    {
                        trackingTarget = player.PlayerBones.Pelvis.Original;
                    }

                    currentHookPos = tipHitInfo.point;

                    StartGrapple(currentHookPos, tipHitInfo.normal);
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

                _view.OverwriteHookPos(nextHookPos);
            }

            if (_state == GrappleState.Taut)
            {
                void ChangePivot(Vector3 pivot)
                {
                    grappleTarget = pivot;
                    _view.ChangePivot(grappleTarget);
                    OnPivotChanged?.Invoke(_player, grappleTarget);
                }


                if (trackingTarget != null)
                {
                    ChangePivot(trackingTarget.position);
                }

                float distToTarget = Vector3.Distance(playerPoint, grappleTarget);
                if (Physics.Raycast(playerPoint, (grappleTarget - playerPoint).normalized, out RaycastHit hit, distToTarget, collisionMask))
                {
                    if (hit.collider.TryGetComponent(out Player player))
                    {
                        trackingTarget = player.PlayerBones.Pelvis.Original;
                    }
                    else
                    {
                        trackingTarget = null;
                        ChangePivot(hit.point);
                    }
                }

                _player.MovementContext.ResetFlying();
                _player.MovementContext.FreefallTime = 0;

                if (_player.MovementContext.CurrentState is JumpPlayerState jumpState)
                {
                    _player.MovementContext.PlayerAnimatorEnableJump(enabled: false);
                    _player.MovementContext.PlayerAnimatorEnableLanding(enabled: true);
                }
            }

            if (_state == GrappleState.Retracting)
            {
                currentHookPos = Vector3.MoveTowards(currentHookPos, playerPoint, retractSpeed * Time.deltaTime);
            }

            _view.TickVisuals(_player.PlayerBones.Pelvis.position);
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

            _view.Shoot(cam.position, cam.forward, hookSpeed);

            _player.ProceduralWeaponAnimation.ForceReact.AddForce(new Vector3(-1f, 0, 0), 0.1f, 0.2f, 1f);

            OnShot?.Invoke(_player, hookDirection, hookSpeed);
        }

        private void StartGrapple(Vector3 point, Vector3 normal)
        {
            _state = GrappleState.Taut;

            grappleTarget = point;

            ropeLength = Vector3.Distance(_player.Transform.position, point);

            _movementSoundMute.shouldMuteMovementSounds = true;

            _player.MovementContext.PlayerAnimator.Animator.Play("Sprint", 0, 0f); // skip land stumble if grapple start mid air

            _view.Hit(point, normal);

            _player.ProceduralWeaponAnimation.ForceReact.AddForce(1f, 0.1f, 0.2f);

            OnHit?.Invoke(_player, point);
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

            float reelSpeed = 32f;
            currentVelocity += directionToTarget * reelSpeed * deltaTime;

            if (currentDistance > this.ropeLength)
            {
                float stiffness = 25f;
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
            trackingTarget = null;

            if (_state == GrappleState.Taut)
            {
                ApplyExitMomentum(_player.Velocity);
            }

            _state = GrappleState.Retracting;

            _movementSoundMute.shouldMuteMovementSounds = false;
            _view.Retract(retractSpeed);

            OnRetract?.Invoke(_player, retractSpeed);
        }

        void ApplyExitMomentum(Vector3 momentum)
        {
            _player.MovementContext.InputMotionBeforeLimit = momentum;

            _player.MovementContext.PlayerAnimator.Animator.Play("Jump_Move", 0, 0f);
            _player.MovementContext.PlayerAnimator.Animator.Update(0.1f);

            JumpPlayerState jumpState = (JumpPlayerState)_player.MovementContext.CurrentState;
            jumpState._isSprintWasPreviousState = true; // to start auto sprint on land if there is forward input
            jumpState._liftForce = momentum.y * Vector3.up;
        }

        void OnDestroy()
        {
            _view?.Dispose();

            if (_player != null)
                _player.OnPlayerDead -= OnPlayerDead;

            _movementSoundMute?.Dispose();

            if (GrapplingControllerInstance == this)
                GrapplingControllerInstance = null;
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
                if (GrapplingControllerInstance == null || 
                    GrapplingControllerInstance._player != ____player || 
                    GrapplingControllerInstance._state != GrappleState.Taut)
                    return;

                GrapplingControllerInstance.ApplyGrapplePhysics(__instance, ref motion, deltaTime);
            }
        }

        private class Patch_JumpPlayerState_ApplyMovementAndRotation : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(JumpPlayerState), nameof(JumpPlayerState.ApplyMovementAndRotation));
            }

            [PatchPrefix]
            static void Prefix(JumpPlayerState __instance)
            {
                __instance.MovementContext.CharacterController.SpeedLimit = -1f;
            }
        }
    }
}