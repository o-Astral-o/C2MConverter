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
    public string ModelName;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

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
                Rotation = Quaternion.CreateFromYawPitchRoll(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                break;
            default:
                throw new InvalidDataException($"Unknown rotation mode: {rotationMode}");
        }

        Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
