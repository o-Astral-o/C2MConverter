using Cast.NET;
using Cast.NET.Nodes;
using C2MConverter.Utils;
using Serilog;
using System.Numerics;
using System;

namespace C2MConverter;

public class C2MObject
{
    public string Name;
    public bool IsXModel;

    public Vector3[] Vertices;
    public Vector3[] Normals;
    public List<Vector2[]> UVs;
    public RBGA[] Colors;

    public C2MSurface[] Surfaces;

    public C2MObject(BinaryReader reader)
    {
        Name = reader.ReadUtf8String();
        IsXModel = reader.ReadBoolean();

        var vertexCount = reader.ReadUInt32();
        Vertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        var normalCount = reader.ReadUInt32();
        Normals = new Vector3[normalCount];
        for (int i = 0; i < normalCount; i++)
        {
            Normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        var uvCount = reader.ReadUInt32();
        UVs = new List<Vector2[]>();
        for(int i = 0; i < uvCount; i++)
        {
            var uvSetCount = reader.ReadUInt32();
            var uvs = new Vector2[uvSetCount];
            for (int j = 0; j < uvSetCount; j++)
            {
                uvs[j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            UVs.Add(uvs);
        }

        var colorCount = reader.ReadUInt32();
        Colors = new RBGA[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            Colors[i] = new RBGA(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        var surfaceCount = reader.ReadUInt32();
        Surfaces = new C2MSurface[surfaceCount];
        for (int i = 0; i < surfaceCount; i++)
        {
            Surfaces[i] = new C2MSurface(reader);
        }
    }

    public ModelNode ToCast()
    {
        ModelNode objectGeo = new ModelNode();
        objectGeo.AddString("n", Name);
        objectGeo.AddNode(new SkeletonNode());
        
        // I don't like this, C2M handles their surface differently
        // it's a little bit annoying to deal with but whatever
        foreach(var surface in Surfaces)
        {
            var materialNode = new MaterialNode(surface.Materials.First(), "pbr");
            var meshNode = new MeshNode();
            meshNode.AddString("n", surface.Name);
            meshNode.AddValue<ulong>("m", materialNode.Hash);
            objectGeo.AddNode(materialNode);

            Dictionary<uint, ushort> RemappedIndex = new();
            CastArrayProperty<Vector3> positions = meshNode.AddArray<Vector3>("vp");
            CastArrayProperty<Vector3> normals = meshNode.AddArray<Vector3>("vn");
            CastArrayProperty<Vector2> uv0 = meshNode.AddArray<Vector2>("u0");

            ushort current = 0;
            foreach (var index in surface.Faces)
            {
                if (!RemappedIndex.ContainsKey(index))
                {
                    RemappedIndex[index] = current++;
                    positions.Values.Add(Vertices[index]);
                    normals.Values.Add(Normals[index]);
                    uv0.Values.Add(UVs[(int)index][0]);
                }
            }

            meshNode.AddValue("ul", (uint)1);

            CastArrayProperty<ushort> faceIndices = meshNode.AddArray<ushort>("f");
            for (int i = 0; i < surface.FacesCount; i += 3)
            {
                faceIndices.Values.Add(RemappedIndex[surface.Faces[i]]);
                faceIndices.Values.Add(RemappedIndex[surface.Faces[i + 1]]);
                faceIndices.Values.Add(RemappedIndex[surface.Faces[i + 2]]);
            }

            objectGeo.AddNode(meshNode);
        }
        return objectGeo;
    }
}