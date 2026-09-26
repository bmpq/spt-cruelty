using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using tarkin.cruelty.grappendix.visuals;
using EFT.CameraControl;
using System;

namespace tarkin.cruelty.grappendix
{
    public class GrapplingView : IDisposable
    {
        private readonly Player _player;
        private readonly GrappendixVisualObject _visual;

        private readonly SurfaceSet _wetSurfaceSet;
        private static readonly FieldInfo Field_Player__soundBySurface = AccessTools.Field(typeof(Player), "_soundBySurface");

        private GrappleState _state;
        private Vector3 _currentHookPos;

        private Vector3 _hookDirection;
        private float _hookSpeed;
        private float _retractSpeed;
        private Vector3 _pivotPoint;

        public GrapplingView(Player player)
        {
            _player = player;
            _visual = new GameObject($"{player.Profile.Nickname} {nameof(GrappendixVisualObject)}").AddComponent<GrappendixVisualObject>();

            _state = GrappleState.Idle;
            _visual.SetState(_state);

            var allSurfaces = Field_Player__soundBySurface.GetValue(player) as Dictionary<BaseBallistic.ESurfaceSound, SurfaceSet>;
            _wetSurfaceSet = allSurfaces[BaseBallistic.ESurfaceSound.Puddle];
        }

        public void Shoot(Vector3 startPos, Vector3 direction, float speed)
        {
            _currentHookPos = startPos;
            _hookDirection = direction;
            _hookSpeed = speed;
            _state = GrappleState.Seeking;
            _visual.SetState(_state);
        }

        public void Hit(Vector3 hitPoint, Vector3 normal)
        {
            _pivotPoint = hitPoint;
            _currentHookPos = hitPoint;
            _state = GrappleState.Taut;
            _visual.SetState(_state);

            PlayHitAudio(hitPoint);

            if (Singleton<Effects>.Instantiated)
                Singleton<Effects>.Instance.EmitBloodOnEnvironment(hitPoint, normal);
        }

        public void ChangePivot(Vector3 hitPoint)
        {
            _pivotPoint = hitPoint;
            if (_state == GrappleState.Taut)
                _currentHookPos = _pivotPoint;
        }

        public void Retract(float speed)
        {
            _retractSpeed = speed;
            if (_state == GrappleState.Taut)
                _currentHookPos = _pivotPoint;

            _state = GrappleState.Retracting;
            _visual.SetState(_state);
        }

        void Idle()
        {
            _state = GrappleState.Idle;
            _visual.SetState(GrappleState.Idle);
        }

        public void TickVisuals(Vector3 playerPelvisPoint)
        {
            if (_state == GrappleState.Seeking)
            {
                _currentHookPos += _hookDirection * _hookSpeed * Time.deltaTime;
                _visual.SetPoints(_currentHookPos, playerPelvisPoint);
            }
            else if (_state == GrappleState.Taut)
            {
                _visual.SetPoints(_pivotPoint, playerPelvisPoint);
            }
            else if (_state == GrappleState.Retracting)
            {
                _currentHookPos = Vector3.MoveTowards(_currentHookPos, playerPelvisPoint, _retractSpeed * Time.deltaTime);
                _visual.SetPoints(_currentHookPos, playerPelvisPoint);
                if (Vector3.Distance(_currentHookPos, playerPelvisPoint) < 0.1f)
                {
                    Idle();
                }
            }
        }

        public void OverwriteHookPos(Vector3 precisePos) => _currentHookPos = precisePos;

        private void PlayHitAudio(Vector3 point)
        {
            if (!Singleton<BetterAudio>.Instantiated || !CameraManager.Exist || CameraManager.Instance.Camera == null)
                return;

            Singleton<BetterAudio>.Instance.PlayAtPoint(point, _wetSurfaceSet.LandingSoundBank, CameraManager.Instance.Distance(point));
        }

        public void Dispose()
        {
            if (_visual != null)
                GameObject.Destroy(_visual.gameObject);
        }
    }
}
