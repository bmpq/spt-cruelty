using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Components;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
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
        public event Action<Player, Vector3, float> OnRemoteGrappleShot;
        public event Action<Player, Vector3> OnRemoteGrappleHit;
        public event Action<Player, Vector3> OnRemoteGrapplePivotChanged;
        private double _timestampLastSentPivotPacket;
        private const double PacketSendCooldown = 0.05;
        public event Action<Player, float> OnRemoteGrappleRetract;

        private readonly object _eventFikaCreated;
        private readonly ManualLogSource _logger;

        public FikaHandler(ManualLogSource logger)
        {
            _logger = logger;
            _eventFikaCreated = new Action<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>((Action<FikaNetworkManagerCreatedEvent>)_eventFikaCreated);
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                RegisterPackets(Singleton<IFikaNetworkManager>.Instance);
            }
        }

        public bool IsHeadless() => FikaBackendUtils.IsHeadless;

        private void OnFikaNetworkCreated(FikaNetworkManagerCreatedEvent fikaEvent) => RegisterPackets(fikaEvent.Manager);

        private void RegisterPackets(IFikaNetworkManager fikaNetworkManager)
        {
            fikaNetworkManager.RegisterPacket<GrappleShotPacket>(OnGrappleShotPacketReceived);
            fikaNetworkManager.RegisterPacket<GrappleHitPacket>(OnGrappleHitPacketReceived);
            fikaNetworkManager.RegisterPacket<GrappleRetractPacket>(OnGrappleRetractPacketReceived);
        }

        public void SendGrappleShot(Player player, Vector3 direction, float speed)
        {
            var packet = new GrappleShotPacket()
            {
                netId = (player as FikaPlayer).NetId,
                direction = direction,
                speed = speed
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        public void SendGrappleHit(Player player, Vector3 point)
        {
            var packet = new GrappleHitPacket()
            {
                netId = (player as FikaPlayer).NetId,
                hitPoint = point,
                first = true
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        public void SendGrapplePivotChanged(Player player, Vector3 point)
        {
            var now = Time.realtimeSinceStartupAsDouble;
            if (now - _timestampLastSentPivotPacket < PacketSendCooldown)
                return;

            var packet = new GrappleHitPacket()
            {
                netId = (player as FikaPlayer).NetId,
                hitPoint = point,
                first = false
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);

            _timestampLastSentPivotPacket = now;
        }

        public void SendGrappleRetract(Player player, float speed)
        {
            var packet = new GrappleRetractPacket()
            {
                netId = (player as FikaPlayer).NetId,
                speed = speed
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private void OnGrappleShotPacketReceived(GrappleShotPacket packet)
        {
            if (TryGetPlayer(packet.netId, out FikaPlayer player))
                OnRemoteGrappleShot?.Invoke(player, packet.direction, packet.speed);
        }

        private void OnGrappleHitPacketReceived(GrappleHitPacket packet)
        {
            if (TryGetPlayer(packet.netId, out FikaPlayer player))
            {
                if (packet.first) OnRemoteGrappleHit?.Invoke(player, packet.hitPoint);
                else OnRemoteGrapplePivotChanged?.Invoke(player, packet.hitPoint);
            }
        }

        private void OnGrappleRetractPacketReceived(GrappleRetractPacket packet)
        {
            if (TryGetPlayer(packet.netId, out FikaPlayer player))
                OnRemoteGrappleRetract?.Invoke(player, packet.speed);
        }

        private bool TryGetPlayer(int playerNetId, out FikaPlayer player)
        {
            player = null;
            if (!CoopHandler.TryGetCoopHandler(out var coopHandler)) 
                return false;

            if (!coopHandler.Players.TryGetValue(playerNetId, out player)) 
                return false;

            return !player.IsYourPlayer;
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
