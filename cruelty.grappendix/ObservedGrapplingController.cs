using EFT;
using UnityEngine;

namespace tarkin.cruelty.player
{
    public class ObservedGrapplingController : MonoBehaviour
    {
        private Player _player;
        private GrapplingView _view;
        private readonly LayerMask collisionMask = LayerMaskController.HighPolyWithTerrainMask | LayerMaskController.TransparentLayerMask;

        public void Init(Player player, GameObject prefabVisual)
        {
            _player = player;
            _view = new GrapplingView(player, prefabVisual);
        }

        public void OnShotReceived(Vector3 direction, float speed)
        {
            _view.Shoot(_player.PlayerBones.HeadCameraCollider.transform.position, direction, speed);
        }

        public void OnHitReceived(Vector3 hitPoint)
        {
            Vector3 playerPoint = _player.PlayerBones.Pelvis.position;
            Vector3 dir = (hitPoint - playerPoint).normalized;
            Vector3 normal = Vector3.up;

            if (Physics.Raycast(hitPoint - dir * 0.1f, dir, out RaycastHit hitInfo, 0.11f, collisionMask))
                normal = hitInfo.normal;

            _view.Hit(hitPoint, normal);
        }

        public void OnPivotChangedReceived(Vector3 point)
        {
            _view.ChangePivot(point);
        }

        public void OnRetractReceived(float speed)
        {
            _view.Retract(speed);
        }

        void Update()
        {
            if (_player != null && _view != null)
                _view.TickVisuals(_player.PlayerBones.Pelvis.position);
        }

        void OnDestroy()
        {
            _view?.Dispose();
        }
    }
}