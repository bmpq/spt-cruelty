using EFT;
using UnityEngine;
using tarkin.cruelty.shared;
using System;

namespace tarkin.cruelty.fika
{
    internal class ObservedPlayerGrapplingController : MonoBehaviour
    {
        private Player _player;
        private GrappendixVisual _visual;

        public void Init(Player player, GameObject prefabGrappendixVisual)
        {
            _player = player;
            _visual = GameObject.Instantiate(prefabGrappendixVisual, transform).GetComponent<GrappendixVisual>();
        }

        public void Shoot(Vector3 direction, float speed)
        {

        }

        internal void ChangePivot(Vector3 hitPoint)
        {
        }

        internal void Hit(Vector3 hitPoint)
        {
        }

        internal void Retract(float speed)
        {
        }

        void OnDestroy()
        {
            if (_visual != null)
                GameObject.Destroy(_visual.gameObject);
        }
    }
}