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
        var length = reader.ReadByte();
        if (length == 0) return string.Empty;

        var bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes).TrimEnd('\0');
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