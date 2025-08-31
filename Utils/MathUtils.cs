using System.Numerics;

namespace C2MConverter.Utils;

public static class MathUtils
{
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
}