using C2MConverter.Utils;
using Serilog;

namespace C2MConverter;

public class C2MSurface
{
    public string Name;
    public byte UvCount;
    public ushort[] Materials;
    public uint FacesCount;
    public uint[] Faces;

    public C2MSurface(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        UvCount = reader.ReadByte();

        var materialsCount = reader.ReadByte();
        Materials = new ushort[materialsCount];
        
        for (int i = 0; i < materialsCount; i++)
        {
            Materials[i] = reader.ReadUInt16();
        }

        FacesCount = reader.ReadUInt32() * 3;
        Faces = new uint[FacesCount];
        for (int i = 0; i < FacesCount; i++)
        {
            Faces[i] = reader.ReadUInt32();
        }
    }
}