using Unity.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;

public struct PlanetDataNetwork : IEquatable<PlanetDataNetwork>, INetworkSerializable
{
    public int Layer;
    public FixedString32Bytes Name;
    public int ColorH;
    public FixedList32Bytes<bool> Materials; // Use FixedList32Bytes for a fixed-size array of booleans
    public FixedList32Bytes<bool> Layout; // Use FixedList32Bytes for a fixed-size array of booleans

    public bool Equals(PlanetDataNetwork other)
    {
        if (Layer != other.Layer || !Name.Equals(other.Name) || ColorH != other.ColorH)
        {
            return false;
        }

        if (Materials.Length != other.Materials.Length)
        {
            return false;
        }

        for (int i = 0; i < Materials.Length; i++)
        {
            if (Materials[i] != other.Materials[i])
            {
                return false;
            }
        }

        if (Layout.Length != other.Layout.Length)
        {
            return false;
        }

        for (int i = 0; i < Layout.Length; i++)
        {
            if (Layout[i] != other.Layout[i])
            {
                return false;
            }
        }

        return true;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Layer);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref ColorH);

        // Serialize the length of the Materials array
        int materialsLength = Materials.Length;
        serializer.SerializeValue(ref materialsLength);

        // Resize the Materials array if necessary (only during deserialization)
        if (serializer.IsReader && Materials.Length != materialsLength)
        {
            Materials.Length = materialsLength;
        }

        // Serialize each element in the Materials array
        for (int i = 0; i < materialsLength; i++)
        {
            bool material = Materials[i];
            serializer.SerializeValue(ref material);
            if (serializer.IsReader)
            {
                Materials[i] = material;
            }
        }

        // Serialize the length of the Layout array
        int layoutLength = Layout.Length;
        serializer.SerializeValue(ref layoutLength);

        // Resize the Layout array if necessary (only during deserialization)
        if (serializer.IsReader && Layout.Length != layoutLength)
        {
            Layout.Length = layoutLength;
        }

        // Serialize each element in the Layout array
        for (int i = 0; i < layoutLength; i++)
        {
            bool layout = Layout[i];
            serializer.SerializeValue(ref layout);
            if (serializer.IsReader)
            {
                Layout[i] = layout;
            }
        }
    }

    public override readonly string ToString()
    {
        string str = $"Layer: {Layer}: Name: {Name}, ColorH: {ColorH} ";
        for (int i = 0; i < Layout.Length; i++)
        {
            str += $"Layout {i}: {Layout[i]} ";
        }
        for (int i = 0; i < Materials.Length; i++)
        {
            str += $"Layout {i}: {Materials[i]} ";
        }
        return str.Trim();
    }
}
