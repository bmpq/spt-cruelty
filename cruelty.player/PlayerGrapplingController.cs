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

        void Awake()
        {
            _player = GetComponent<Player>();
            cam = _player.PlayerBones.HeadCameraCollider.transform;
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
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(grappleKey))
            {
                if (cam != null && Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxGrappleDistance, LayerMaskController.HighPolyWithTerrainMask))
                {
                    GrappleTarget = hit.point;
                    IsGrappling = true;
                }
            }

            if (Input.GetKeyUp(grappleKey))
            {
                StopGrapple();
            }
        }

        private void StopGrapple()
        {
            if (!IsGrappling)
                return;
            IsGrappling = false;
        }
    }
}