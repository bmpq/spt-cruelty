using Comfort.Common;
using EFT;
using Fika.Core.Main.Components;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using System;
using UnityEngine;

namespace tarkin.cruelty.fika
{
    public class FikaHandler : IDisposable
    {
        private object _eventFikaCreated;

        private GameObject _prefabGrappendixVisual;

        public FikaHandler(GameObject prefabGrappendixVisual)
        {
            _prefabGrappendixVisual = prefabGrappendixVisual;

            _eventFikaCreated = new Action<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>((Action<FikaNetworkManagerCreatedEvent>)_eventFikaCreated);
        }

        private void OnFikaNetworkCreated(FikaNetworkManagerCreatedEvent fikaEvent)
        {
            fikaEvent.Manager.RegisterPacket<GrappleShotPacket>(OnGrappleShotPacketReceived);
            fikaEvent.Manager.RegisterPacket<GrappleHitPacket>(OnGrappleHitPacketReceived);
            fikaEvent.Manager.RegisterPacket<GrappleRetractPacket>(OnGrappleRetractPacketReceived);
        }

        public void SendGrappleShot(Player player, Vector3 direction, float speed)
        {
            var packet = new GrappleShotPacket()
            {
                playerId = player.PlayerId,
                direction = direction,
                speed = speed
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        public void SendGrappleHit(Player player, Vector3 point)
        {
            var packet = new GrappleHitPacket()
            {
                playerId = player.PlayerId,
                hitPoint = point,
                first = true
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        public void SendGrapplePivotChanged(Player player, Vector3 point)
        {
            var packet = new GrappleHitPacket()
            {
                playerId = player.PlayerId,
                hitPoint = point,
                first = false
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        public void SendGrappleRetract(Player player, float speed)
        {
            var packet = new GrappleRetractPacket()
            {
                playerId = player.PlayerId,
                speed = speed
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private void OnGrappleShotPacketReceived(GrappleShotPacket packet)
        {
            if (!TryGetController(packet.playerId, out var observedController))
                return;

            observedController.Shoot(packet.direction, packet.speed);
        }

        private void OnGrappleHitPacketReceived(GrappleHitPacket packet)
        {
            if (!TryGetController(packet.playerId, out var observedController))
                return;

            if (packet.first)
                observedController.Hit(packet.hitPoint);
            else
                observedController.ChangePivot(packet.hitPoint);
        }

        private void OnGrappleRetractPacketReceived(GrappleRetractPacket packet)
        {
            if (!TryGetController(packet.playerId, out var observedController))
                return;

            observedController.Retract(packet.speed);
        }

        private bool TryGetController(int playerId, out ObservedPlayerGrapplingController observedController)
        {
            observedController = null;

            if (!CoopHandler.TryGetCoopHandler(out var coopHandler))
                return false;

            if (!coopHandler.Players.TryGetValue(playerId, out var player))
                return false;

            if (player.IsYourPlayer)
                return false;

            if (!player.TryGetComponent<ObservedPlayerGrapplingController>(out observedController))
            {
                observedController = player.gameObject.AddComponent<ObservedPlayerGrapplingController>();
                observedController.Init(player, _prefabGrappendixVisual);
            }

            return true;
        }

        public void Dispose()
        {
            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>((Action<FikaNetworkManagerCreatedEvent>)_eventFikaCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                Singleton<IFikaNetworkManager>.Instance.UnregisterPacket<GrappleShotPacket>();
                Singleton<IFikaNetworkManager>.Instance.UnregisterPacket<GrappleHitPacket>();
                Singleton<IFikaNetworkManager>.Instance.UnregisterPacket<GrappleRetractPacket>();
            }
        }
    }
}
