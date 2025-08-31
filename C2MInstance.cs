using System.Numerics;
using C2MConverter.Utils;
using Cast.NET.Nodes;
using Serilog;

namespace C2MConverter;

enum RotationMode
{
    Quaternion = 0,
    Euler = 1
}

public class C2MInstance
{
    public byte Type;
    public string ModelName;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public C2MInstance(BinaryReader reader)
    {
        ModelName = reader.ReadUtf8String();
        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        var euler = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Rotation = MathUtils.EulToQuat(euler);
        Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
public enum DynamicInstanceType : byte
{
    Default,
    Destructible
}

public class C2MDynamicInstance
{
    public string Name;
    public string DestroyedName;

    public float Health;

    public DynamicInstanceType Type;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public C2MDynamicInstance(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        DestroyedName = reader.ReadUtf8String();
        Health = reader.ReadSingle();
        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Rotation = MathUtils.EulToQuat(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
        Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Type = (DynamicInstanceType)reader.ReadByte();
    }
}