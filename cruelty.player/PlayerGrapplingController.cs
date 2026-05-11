using Comfort.Common;
using EFT;
using Systems.Effects;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [DefaultExecutionOrder(100)] // probably not necessary
    public class PlayerGrapplingController : MonoBehaviour
    {
        private Player _player;

        public bool IsGrappling { get; private set; }
        public bool IsHookFlying { get; private set; }

        public Vector3 GrappleTarget { get; private set; }
        public float RopeLength { get; private set; }

        private KeyCode grappleKey = KeyCode.G;
        private float maxGrappleDistance = 50f;

        private float hookSpeed = 120f;
        private Vector3 currentHookPos;
        private Vector3 hookDirection;
        private float distanceTraveled;

        Transform cam;
        GrappendixVisual _visual;

        public void Init(Player player, GameObject prefabGrappendixVisual)
        {
            _player = player;
            cam = _player.PlayerBones.HeadCameraCollider.transform;

            _visual = Instantiate(prefabGrappendixVisual).GetComponent<GrappendixVisual>();
            _visual.gameObject.SetActive(false);
        }

        void Update()
        {
            if (_player == null || !_player.IsYourPlayer)
            {
                Destroy(this);
                return;
            }    

            HandleInput();

            Vector3 playerPoint = _player.PlayerBones.Pelvis.Original.position;

            if (IsHookFlying)
            {
                float step = hookSpeed * Time.deltaTime;

                bool tipHit = Physics.Raycast(currentHookPos, hookDirection, out RaycastHit tipHitInfo, step, LayerMaskController.HighPolyWithTerrainMask);

                Vector3 nextHookPos = tipHit ? tipHitInfo.point : currentHookPos + (hookDirection * step);

                Vector3 ropeDir = nextHookPos - playerPoint;
                float ropeDist = ropeDir.magnitude;

                bool ropeHit = Physics.Raycast(playerPoint, ropeDir.normalized, out RaycastHit ropeHitInfo, ropeDist, LayerMaskController.HighPolyWithTerrainMask);

                if (ropeHit)
                {
                    currentHookPos = ropeHitInfo.point;
                    IsHookFlying = false;

                    StartGrapple(currentHookPos);

                    if (Singleton<Effects>.Instantiated)
                        Singleton<Effects>.Instance.EmitBloodOnEnvironment(ropeHitInfo.point, ropeHitInfo.normal);
                }
                else if (tipHit)
                {
                    currentHookPos = tipHitInfo.point;
                    IsHookFlying = false;

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

                if (IsHookFlying)
                {
                    _visual.SetPoints(currentHookPos, playerPoint);
                }
            }

            if (IsGrappling)
            {
                float distToTarget = Vector3.Distance(playerPoint, GrappleTarget);

                if (Physics.Raycast(playerPoint, (GrappleTarget - playerPoint).normalized, out RaycastHit hit, distToTarget, LayerMaskController.HighPolyWithTerrainMask))
                {
                    GrappleTarget = hit.point;
                }

                _player.MovementContext.ResetFlying();
                _player.MovementContext.FreefallTime = 0;

                if (_player.MovementContext.CurrentState is JumpPlayerState jumpState)
                {
                    _player.MovementContext.PlayerAnimatorEnableJump(enabled: false);
                    _player.MovementContext.PlayerAnimatorEnableLanding(enabled: true);
                }

                _visual.SetPoints(GrappleTarget, playerPoint);
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(grappleKey))
            {
                StartHookFlight();
            }

            if (Input.GetKeyUp(grappleKey))
            {
                StopGrapple();
            }
        }

        private void StartHookFlight()
        {
            hookDirection = cam.forward;
            currentHookPos = cam.position;
            distanceTraveled = 0f;

            IsHookFlying = true;
            _visual.gameObject.SetActive(true);
        }

        private void StartGrapple(Vector3 point)
        {
            GrappleTarget = point;
            IsGrappling = true;

            RopeLength = Vector3.Distance(_player.Transform.position, point);
            _player.MovementContext.PlayerAnimator.Animator.Play("Sprint", 0, 0f); // skip land stumble if grapple start mid air

            _visual.gameObject.SetActive(true);
        }

        private void StopGrapple()
        {
            if (IsHookFlying)
            {
                IsHookFlying = false;
                _visual.gameObject.SetActive(false);
            }

            if (!IsGrappling)
                return;
            IsGrappling = false;

            _visual.gameObject.SetActive(false);

            ApplyExitMomentum(_player.Velocity);
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
        }
    }
}