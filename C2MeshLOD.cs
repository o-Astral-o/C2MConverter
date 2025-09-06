using Cast.NET;
using Cast.NET.Nodes;
using C2MConverter.Utils;
using Serilog;
using System.Numerics;
using System;

namespace C2MConverter;

public class C2MeshLOD
{
    public string Name;
    public bool IsXModel;

    public Vector3[] Vertices;
    public Vector3[] Normals;
    public List<Vector2[]> UVs;
    public RBGA[] Colors;

    public C2MSurface[] Surfaces;

    public C2MeshLOD(BinaryReader reader)
    {
        var vertexCount = reader.ReadUInt32();
        var surfaceCount = reader.ReadUInt32();
        var faceCount = reader.ReadUInt32();
        var lodDistance = reader.ReadSingle();

        Vertices = new Vector3[vertexCount];
        Normals = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        for (int i = 0; i < vertexCount; i++)
        {
            Normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        UVs = new List<Vector2[]>();
        for(int i = 0; i < vertexCount; i++)
        {
            var uvSetCount = reader.ReadUInt32();
            var uvs = new Vector2[uvSetCount];
            for (int j = 0; j < uvSetCount; j++)
            {
                uvs[j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            UVs.Add(uvs);
        }

        Colors = new RBGA[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Colors[i] = new RBGA(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        Surfaces = new C2MSurface[surfaceCount];
        for (int i = 0; i < surfaceCount; i++)
        {
            Surfaces[i] = new C2MSurface(reader);
        }
    }
}