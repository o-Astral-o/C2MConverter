using C2MConverter.Utils;
using Serilog;
using System.Numerics;
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
        if (Name.StartsWith("*"))
        {
            Name = Name.TrimStart('*');
        }
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

            if (textureType.Contains("colorMap"))
            {
                textureType = textureType.Replace("colorMap", "diffuse_map");
            }
            else if (textureType.Contains("nogMap"))
            {
                textureType = textureType.Replace("nogMap", "nog_map");
            }
            else if (textureType.Contains("revealMap"))
            {
                textureType = textureType.Replace("revealMap", "reveal_map");
            }
            else if (textureType.Contains("opacityMap"))
            {
                textureType = textureType.Replace("opacityMap", "opacity_map");
            }

            Textures[textureType] = textureName;
        }

        for(int i = 0; i < constantsCount; i++)
        {
            var constantName = reader.ReadUtf8String();
            var constantHash = reader.ReadUInt32();
            var constantType = reader.ReadByte();
            var constantValue = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}