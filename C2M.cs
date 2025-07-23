using C2MConverter.Utils;
using Cast.NET.Nodes;
using Cast.NET;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace C2MConverter;

public class C2M
{
    public byte Version;
    public string Name;
    public byte MapVersion;
    public string SkyboxInfo;

    //Object
    public C2MObject[] Objects;
    public C2MMaterial[] Materials;
    public C2MInstance[] ModelInstances;

    public C2M(byte[] data)
    {
        var binaryReader = new BinaryReader(new MemoryStream(data));
        var header = binaryReader.ReadBytes(3);
        if (header[0] != 'C' || header[1] != '2' || header[2] != 'M')
        {
            Log.Error("Invalid C2M file format.");
            return;
        }

        Version = binaryReader.ReadByte();
        if (Version != 1)
        {
            Log.Error($"Unsupported C2M version: {Version}. Only version 1 is supported.");
            return;
        }

        MapVersion = binaryReader.ReadByte();
        Name = binaryReader.ReadUtf8String();
        SkyboxInfo = binaryReader.ReadUtf8String();
        var objectCount = binaryReader.ReadUInt32();

        Log.Information($"C2M file loaded: Version {Version}, Map Version {MapVersion}, Name: {Name}, Skybox: {SkyboxInfo}, Object Count: {objectCount}");

        Objects = new C2MObject[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            Objects[i] = new C2MObject(binaryReader);
        }

        var materialCount = binaryReader.ReadUInt32();
        Materials = new C2MMaterial[materialCount];
        for (int i = 0; i < materialCount; i++)
        {
            Materials[i] = new C2MMaterial(binaryReader);
        }

        var instanceCount = binaryReader.ReadUInt32();
        ModelInstances = new C2MInstance[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            ModelInstances[i] = new C2MInstance(binaryReader);
        }
    }

    //wow....spooky :3
    public void SaveAsCast(string path)
    {
        string directory;
        if (!Directory.Exists(path))
        {
            directory = Path.GetDirectoryName(path);
        }
        else
        {
            directory = path;
        }

        var models = Path.Combine(directory, "models");
        if (!Directory.Exists(models))
        {
            Directory.CreateDirectory(models);
        }

        //Root
        CastNode mapRoot = new CastNode(CastNodeIdentifier.Root);
        CastNode propRoot = new CastNode(CastNodeIdentifier.Root);

        foreach (var @object in Objects)
        {
            if(@object.Name == "mapGeometry")
            {
                mapRoot.AddNode(@object.ToCast());
            }
            else
            {
                var file = Path.Combine(models, $"{@object.Name}.cast");
                CastNode modelRoot = new CastNode(CastNodeIdentifier.Root);
                modelRoot.AddNode(@object.ToCast());
                CastWriter.Save(file, modelRoot);
            }
        }

        var count = 0;
        foreach(var instance in ModelInstances)
        {
            var fileNode = new FileNode();
            fileNode.AddString("p", $"models/{instance.ModelName}.cast");

            var instanceNode = new InstanceNode();
            instanceNode.AddString("n", $"{instance.ModelName}_{count++}");
            instanceNode.AddValue("rf", fileNode.Hash);
            instanceNode.AddValue("p", instance.Position);
            instanceNode.AddValue("r", new Vector4(instance.Rotation.X, instance.Rotation.Y, instance.Rotation.Z, instance.Rotation.W));
            instanceNode.AddValue("s", instance.Scale);
            instanceNode.AddNode(fileNode);
            propRoot.AddNode(instanceNode);
        }

        CastWriter.Save($"map.cast", mapRoot);
        CastWriter.Save($"propRoot.cast", propRoot);
    }
}