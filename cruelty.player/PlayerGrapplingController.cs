using EFT;
using UnityEngine;

namespace tarkin.cruelty.player
{
    public class PlayerGrapplingController : MonoBehaviour
    {
        private Player _player;

        public bool IsGrappling { get; private set; }
        public Vector3 GrappleTarget { get; private set; }

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
            _visual.gameObject.SetActive(true);
        }

        private void StopGrapple()
        {
            if (!IsGrappling)
                return;
            IsGrappling = false;

            _visual.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (_visual != null)
                Destroy(_visual.gameObject);
        }
    }
}