using EFT;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [DefaultExecutionOrder(100)] // probably not necessary
    public class PlayerGrapplingController : MonoBehaviour
    {
        private Player _player;

        public bool IsGrappling { get; private set; }
        public Vector3 GrappleTarget { get; private set; }
        public Vector3 GrappleMomentum { get; set; }
        public float RopeLength { get; private set; }

        private KeyCode grappleKey = KeyCode.G;
        private float maxGrappleDistance = 50f;

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

            if (IsGrappling)
            {
                _player.MovementContext.ResetFlying();
                _player.MovementContext.FreefallTime = 0;

                if (_player.MovementContext.CurrentState is JumpPlayerState jumpState)
                {
                    _player.MovementContext.PlayerAnimatorEnableJump(enabled: false);
                    _player.MovementContext.PlayerAnimatorEnableLanding(enabled: true);
                }

                _visual.SetPoints(GrappleTarget, _player.PlayerBones.Pelvis.Original.position);
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(grappleKey))
            {
                if (cam != null && Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxGrappleDistance, LayerMaskController.HighPolyWithTerrainMask))
                {
                    StartGrapple(hit.point);
                }
            }

            if (Input.GetKeyUp(grappleKey))
            {
                StopGrapple();
            }
        }

        private void StartGrapple(Vector3 point)
        {
            GrappleTarget = point;
            IsGrappling = true;

            RopeLength = Vector3.Distance(_player.Transform.position, point);
            GrappleMomentum = _player.MovementContext.Velocity;
            _player.MovementContext.PlayerAnimator.Animator.Play("Sprint", 0, 0f); // skip land stumble if grapple start mid air

            _visual.gameObject.SetActive(true);
        }

        private void StopGrapple()
        {
            if (!IsGrappling)
                return;
            IsGrappling = false;

            _visual.gameObject.SetActive(false);

            ApplyExitMomentum(GrappleMomentum);
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