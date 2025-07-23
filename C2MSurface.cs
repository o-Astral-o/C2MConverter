using C2MConverter.Utils;
using Serilog;

namespace C2MConverter;

public class C2MSurface
{
    public string Name;
    public string[] Materials;
    public uint FacesCount;
    public uint[] Faces;

    public C2MSurface(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        Log.Debug("Reading surface: {SurfaceName}", Name);
        var materialsCount = reader.ReadUInt32();
        Materials = new string[materialsCount];
        
        for (int i = 0; i < materialsCount; i++)
        {
            Materials[i] = reader.ReadUtf8String();
        }

        FacesCount = reader.ReadUInt32() * 3;
        Faces = new uint[FacesCount];
        for (int i = 0; i < FacesCount; i++)
        {
            Faces[i] = reader.ReadUInt32();
        }
    }
}