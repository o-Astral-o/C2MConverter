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
        var vertexCount = reader.ReadUInt32();
        var surfaceCount = reader.ReadUInt32();
        var faceCount = reader.ReadUInt32();
        var lodCount = reader.ReadUInt32();
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

        //todo lod handling :(
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
            var materialNode = new MaterialNode(surface.Name, "pbr");
            var meshNode = new MeshNode();
            meshNode.AddString("n", surface.Name);
            meshNode.AddValue<ulong>("m", materialNode.Hash);
            objectGeo.AddNode(materialNode);

            Dictionary<uint, ushort> RemappedIndex = new();
            CastArrayProperty<Vector3> positions = meshNode.AddArray<Vector3>("vp");
            CastArrayProperty<Vector3> normals = meshNode.AddArray<Vector3>("vn");
            CastArrayProperty<uint> colors = meshNode.AddArray<uint>("c0");

            uint uvLayersCount = 1;

            ushort current = 0;
            foreach (var index in surface.Faces)
            {
                if (!RemappedIndex.ContainsKey(index))
                {
                    RemappedIndex[index] = current++;
                    positions.Values.Add(Vertices[index]);
                    normals.Values.Add(Normals[index]);
                    colors.Values.Add(Colors[index].PackRGBA());
                    var uvs = UVs[(int)index];
                    var layersCount = (uint)uvs.Count();
                    if (layersCount > uvLayersCount) uvLayersCount = layersCount;
                    for (int layer = 0; layer < layersCount; layer++)
                    {
                        var layerKey = $"u{layer}";
                        var reinvertUv = new Vector2(uvs[layer].X, 1.0f - uvs[layer].Y);
                        if (meshNode.TryGetArrayProperty<Vector2>(layerKey, out CastArrayProperty<Vector2> uv))
                        {
                            uv.Values.Add(reinvertUv);
                        }
                        else
                        {
                            uv = meshNode.AddArray<Vector2>(layerKey);
                            uv.Values.Add(reinvertUv);
                        }
                    }
                }
            }

            meshNode.AddValue("ul", (uint)uvLayersCount);
            meshNode.AddValue("cl", (uint)1);

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