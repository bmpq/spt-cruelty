using EFT;
using UnityEngine;
using tarkin.cruelty.shared;
using Comfort.Common;
using Systems.Effects;
using System.Reflection;
using HarmonyLib;
using System.Collections.Generic;


#if SPT_4_0
using LayerMaskController = LayerMaskClass;
using CameraManager = CameraClass;
#endif

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

        Vector3 playerPoint => _player.PlayerBones.Pelvis.position;
        private readonly LayerMask collisionMask = LayerMaskController.HighPolyWithTerrainMask | LayerMaskController.TransparentLayerMask;


        private static readonly FieldInfo Field_Player__soundBySurface = AccessTools.Field(typeof(Player), "_soundBySurface");
        private SurfaceSet wetSurfaceSet;

        internal void Init(Player player, GameObject prefabGrappendixVisual)
        {
            _player = player;
            _visual = GameObject.Instantiate(prefabGrappendixVisual, transform).GetComponent<GrappendixVisual>();

            var allSurfaces = Field_Player__soundBySurface.GetValue(player) as Dictionary<BaseBallistic.ESurfaceSound, SurfaceSet>;
            wetSurfaceSet = allSurfaces[BaseBallistic.ESurfaceSound.Puddle];
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

            if (Singleton<Effects>.Instantiated)
            {
                Vector3 dir = (hitPoint - playerPoint).normalized;
                float projectionOffset = 0.1f;

                if (Physics.Raycast(hitPoint - dir * projectionOffset, dir, out RaycastHit hitInfo, projectionOffset * 1.1f, collisionMask))
                {
                    Singleton<Effects>.Instance.EmitBloodOnEnvironment(hitInfo.point, hitInfo.normal);
                }
            }

            PlayHitAudio(hitPoint);
        }

        void PlayHitAudio(Vector3 point)
        {
            if (!Singleton<BetterAudio>.Instantiated || !CameraManager.Exist || CameraManager.Instance.Camera == null)
                return;

            Singleton<BetterAudio>.Instance.PlayAtPoint(point, wetSurfaceSet.LandingSoundBank, CameraManager.Instance.Distance(point));
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