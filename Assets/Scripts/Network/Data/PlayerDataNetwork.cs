using Unity.Collections;
using Unity.Netcode;
using System;

public struct PlayerDataNetwork : IEquatable<PlayerDataNetwork>, INetworkSerializable
{
    public ulong ClientId;
    public int CharacterId;
    public FixedString64Bytes Name;
    public FixedString64Bytes Id;
    public bool Ready;
    public int Ping;

    public bool Equals(PlayerDataNetwork other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId &&
            Name == other.Name &&
            Id == other.Id &&
            Ready == other.Ready &&
            Ping == other.Ping;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref Ready);
        serializer.SerializeValue(ref Ping);
    }

    public override readonly string ToString()
    {
        return $"{ClientId}: Char: {CharacterId}, Name: {Name}, Id: {Id}, Ready: {Ready}, Ping: {Ping}";
    }
}
