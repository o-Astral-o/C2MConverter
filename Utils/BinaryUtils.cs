using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2MConverter.Utils;

public static class BinaryUtils
{
    public static string ReadUtf8String(this BinaryReader reader)
    {
        reader.BaseStream.Seek(1, SeekOrigin.Current);
        List<char> chars = new List<char>();
        char c;
        // Start reading characters until we hit '\x00'
        while ((c = reader.ReadChar()) != '\0')
        {
            chars.Add(c);
        }
        return new string(chars.ToArray());
    }

    public static string ReadNullTerminatedString(this BinaryReader reader)
    {
        var sb = new StringBuilder();
        char c;
        while ((c = reader.ReadChar()) != '\0')
        {
            sb.Append(c);
        }
        return sb.ToString();
    }
}