using C2MConverter.Utils;
using Serilog;

namespace C2MConverter;

public class C2MMaterial
{
    public string Name;
    public string TechSet;
    public byte SortKey;
    public Dictionary<string, string> Textures;

    public C2MMaterial(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        //bro what the fuck is going on????
        TechSet = reader.ReadNullTerminatedString();
        SortKey = reader.ReadByte();
        var texturesCount = reader.ReadByte();
        Textures = new Dictionary<string, string>();
        for (int i = 0; i < texturesCount; i++)
        {
            var textureName = reader.ReadUtf8String();
            var texturePath = reader.ReadUtf8String();
            Textures[textureName] = texturePath;
        }

        //can't believe the fact that i have access to the source code
        //i still don't know what is this and whys there a null byte here
        var unk0 = reader.ReadByte();
    }
}