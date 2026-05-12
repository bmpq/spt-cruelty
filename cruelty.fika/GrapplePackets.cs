using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace tarkin.cruelty.fika
{
    internal struct GrappleShotPacket : INetSerializable
    {
        public int netId;
        public Vector3 direction;
        public float speed;

        public void Deserialize(NetDataReader reader)
        {
            netId = reader.GetInt();
            direction = reader.GetUnmanaged<Vector3>();
            speed = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(netId);
            writer.PutUnmanaged(direction);
            writer.Put(speed);
        }
    }

    internal struct GrappleHitPacket : INetSerializable
    {
        public int netId;
        public Vector3 hitPoint;
        public bool first;

        public void Deserialize(NetDataReader reader)
        {
            netId = reader.GetInt();
            hitPoint = reader.GetUnmanaged<Vector3>();
            first = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(netId);
            writer.PutUnmanaged(hitPoint);
            writer.Put(first);
        }
    }

    internal struct GrappleRetractPacket : INetSerializable
    {
        public int netId;
        public float speed;

        public void Deserialize(NetDataReader reader)
        {
            netId = reader.GetInt();
            speed = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(netId);
            writer.Put(speed);
        }
    }
}
