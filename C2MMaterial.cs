using C2MConverter.Utils;
using Serilog;
using System.Reflection.Metadata;

namespace C2MConverter;

public class C2MMaterial
{
    public string Name;
    public string TechSet;
    public string SurfType;
    public byte Blending;
    public byte SortKey;
    public Dictionary<string, string> Textures;

    public C2MMaterial(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        TechSet = reader.ReadUtf8String();
        SurfType = reader.ReadUtf8String();

        Blending = reader.ReadByte();
        SortKey = reader.ReadByte();
        var texturesCount = reader.ReadByte();
        var constantsCount = reader.ReadByte();

        Textures = new Dictionary<string, string>();
        for (int i = 0; i < texturesCount; i++)
        {
            var textureName = reader.ReadUtf8String();
            var textureType = reader.ReadUtf8String();

            if(textureType == "colorMap")
            {
                textureType = "diffuse_map";
            }else if(textureType == "nogMap")
            {
                textureType = "nog_map";
            }
            Textures[textureType] = textureName;
        }

        for(int i = 0; i < constantsCount; i++)
        {
            reader.ReadUInt32();
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadSingle();
            reader.ReadSingle();
        }
    }
}