using System.Numerics;
using C2MConverter.Utils;
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
    public static Vector3 ToRadians(Vector3 v)
    {
        return v * (MathF.PI / 180f);
    }

    public static Quaternion EulToQuat(Vector3 eulerDeg)
    {
        Vector3 euler = ToRadians(eulerDeg);

        float cx = MathF.Cos(euler.X * 0.5f);
        float sx = MathF.Sin(euler.X * 0.5f);
        float cy = MathF.Cos(euler.Y * 0.5f);
        float sy = MathF.Sin(euler.Y * 0.5f);
        float cz = MathF.Cos(euler.Z * 0.5f);
        float sz = MathF.Sin(euler.Z * 0.5f);

        float w = cz * cy * cx + sz * sy * sx;
        float x = cz * cy * sx - sz * sy * cx;
        float y = cz * sy * cx + sz * cy * sx;
        float z = sz * cy * cx - cz * sy * sx;

        return new Quaternion(x, y, z, w);
    }

    public C2MInstance(BinaryReader reader)
    {
        ModelName = reader.ReadUtf8String();
        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        var rotationMode = (RotationMode)reader.ReadByte();
        switch (rotationMode)
        {
            case RotationMode.Quaternion:
                Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                break;
            case RotationMode.Euler:
                var euler = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Rotation = EulToQuat(euler);
                break;
            default:
                throw new InvalidDataException($"Unknown rotation mode: {rotationMode}");
        }

        Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Type = reader.ReadByte();
    }
}
