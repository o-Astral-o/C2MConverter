using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2MConverter.Utils;

public class RBGA
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
    public RBGA(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    public override string ToString()
    {
        return $"R: {R}, G: {G}, B: {B}, A: {A}";
    }
}