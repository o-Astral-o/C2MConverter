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

            //basically you get a list of used vertice in the original object
            //then remap them to your new one
            var indexes = surface.Faces.Distinct().ToArray();
            var count = indexes.Count();

            CastArrayProperty<Vector3> positions = meshNode.AddArray<Vector3>("vp", count);
            CastArrayProperty<Vector3> normals = meshNode.AddArray<Vector3>("vn", count);
            var uvLayersCount = 1;

            Dictionary<uint, ushort> RemappedIndex = new();

            for(ushort i = 0; i < count; i++)
            {
                var originalIndice = indexes[i];
                RemappedIndex[originalIndice] = i;

                positions.Values.Add(Vertices[originalIndice]);
                normals.Values.Add(Normals[originalIndice]);
                var layersCount = UVs[(int)originalIndice].Count();
                if (layersCount > uvLayersCount) uvLayersCount = layersCount;

                for (int layer = 0; layer < layersCount; layer++)
                {
                    var layerKey = $"u{layer}";
                    if (meshNode.TryGetArrayProperty<Vector2>(layerKey, out CastArrayProperty<Vector2> uv))
                    {
                        uv.Values.Add(UVs[(int)originalIndice][layer]);
                    }
                    else
                    {
                        uv = meshNode.AddArray<Vector2>(layerKey, count);
                        uv.Values.Add(UVs[(int)originalIndice][layer]);
                    }
                }
            }

            meshNode.AddValue("ul", (uint)uvLayersCount);

            CastArrayProperty<ushort> faceIndices = meshNode.AddArray<ushort>("f", (int)surface.FacesCount);
            for (int i = 0; i < surface.FacesCount; i += 3)
            {
                var a = RemappedIndex[surface.Faces[i]];
                var b = RemappedIndex[surface.Faces[i + 1]];
                var c = RemappedIndex[surface.Faces[i + 2]];

                //cast follows CCW winding order
                faceIndices.Values.Add(a);
                faceIndices.Values.Add(b);
                faceIndices.Values.Add(c);
            }

            objectGeo.AddNode(meshNode);
        }
        return objectGeo;
    }
}