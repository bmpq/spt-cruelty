using EFT;
using UnityEngine;
using tarkin.cruelty.shared;

namespace tarkin.cruelty.fika
{
    internal class ObservedPlayerGrapplingController : MonoBehaviour
    {
        private Player _player;
        private GrappendixVisual _visual;

        private Vector3 _pivotPoint;
        private Vector3 _currentHookPos;
        private Vector3 _hookDirection;

        private float _hookSpeed;
        private float _retractSpeed;

        private GrappleState _state;

        internal void Init(Player player, GameObject prefabGrappendixVisual)
        {
            _player = player;
            _visual = GameObject.Instantiate(prefabGrappendixVisual, transform).GetComponent<GrappendixVisual>();
        }

        internal void Shoot(Vector3 direction, float speed)
        {
            _hookDirection = direction;
            _hookSpeed = speed;

            _currentHookPos = _player.PlayerBones.HeadCameraCollider.transform.position;

            _state = GrappleState.Seeking;
            _visual.SetState(_state);
        }

        internal void Hit(Vector3 hitPoint)
        {
            _pivotPoint = hitPoint;
            _currentHookPos = hitPoint;

            _state = GrappleState.Taut;
            _visual.SetState(_state);
        }

        internal void ChangePivot(Vector3 hitPoint)
        {
            _pivotPoint = hitPoint;

            if (_state == GrappleState.Taut)
            {
                _currentHookPos = _pivotPoint;
            }
        }

        internal void Retract(float speed)
        {
            _retractSpeed = speed;

            if (_state == GrappleState.Taut)
            {
                _currentHookPos = _pivotPoint;
            }

            _state = GrappleState.Retracting;
            _visual.SetState(_state);
        }

        void Update()
        {
            if (_player == null || _player.PlayerBones == null) return;

            Vector3 playerPoint = _player.PlayerBones.Pelvis.position;

            if (_state == GrappleState.Seeking)
            {
                _currentHookPos += _hookDirection * _hookSpeed * Time.deltaTime;
                _visual.SetPoints(_currentHookPos, playerPoint);
            }
            else if (_state == GrappleState.Taut)
            {
                _visual.SetPoints(_pivotPoint, playerPoint);
            }
            else if (_state == GrappleState.Retracting)
            {
                _currentHookPos = Vector3.MoveTowards(_currentHookPos, playerPoint, _retractSpeed * Time.deltaTime);
                _visual.SetPoints(_currentHookPos, playerPoint);
            }
        }

        void OnDestroy()
        {
            if (_visual != null)
                GameObject.Destroy(_visual.gameObject);
        }
    }
}