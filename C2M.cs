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
    public C2MDynamicInstance[] DynamicInstances;

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
        MapVersion = binaryReader.ReadByte();
        Name = binaryReader.ReadUtf8String();
        SkyboxInfo = binaryReader.ReadUtf8String();
        var objectCount = binaryReader.ReadUInt32();
        var objectOffset = binaryReader.ReadUInt64();
        var staticInstanceCount = binaryReader.ReadUInt32();
        var dynamicInstanceCount = binaryReader.ReadUInt32();
        var instanceOffset = binaryReader.ReadUInt64();
        var imagesCount = binaryReader.ReadUInt32();
        var imageOffset = binaryReader.ReadUInt64();
        var materialsCount = binaryReader.ReadUInt32();
        var materialsOffset = binaryReader.ReadUInt64();
        var lightsCount = binaryReader.ReadUInt32();
        var lightsOffset = binaryReader.ReadUInt64();
        var mapEntsOffset = binaryReader.ReadUInt64();

        Log.Information($"C2M Version: {Version}, Map Version: {MapVersion}, Name: {Name}, Skybox Info: {SkyboxInfo}");

        binaryReader.BaseStream.Seek((long)objectOffset, SeekOrigin.Begin);
        Objects = new C2MObject[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            Objects[i] = new C2MObject(binaryReader);
        }
        
        binaryReader.BaseStream.Seek((long)materialsOffset, SeekOrigin.Begin);
        Materials = new C2MMaterial[materialsCount];
        for (int i = 0; i < materialsCount; i++)
        {
            Materials[i] = new C2MMaterial(binaryReader);
        }

        Log.Information($"Loaded {Materials.Length} materials from C2M file.");

        binaryReader.BaseStream.Seek((long)instanceOffset, SeekOrigin.Begin);
        ModelInstances = new C2MInstance[staticInstanceCount];
        for (int i = 0; i < staticInstanceCount; i++)
        {
            ModelInstances[i] = new C2MInstance(binaryReader);
        }

        DynamicInstances = new C2MDynamicInstance[dynamicInstanceCount];
        for (int i = 0; i < dynamicInstanceCount; i++)
        {
            DynamicInstances[i] = new C2MDynamicInstance(binaryReader);
        }

        Log.Information($"Loaded {ModelInstances.Length} model instances from C2M file.");
        Log.Information($"Loaded {DynamicInstances.Length} dynamic instances from C2M file.");

        binaryReader.Close();
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
        foreach (var @object in Objects)
        {
            if(@object.Name == "mapGeometry")
            {
                CastNode mapRoot = new CastNode(CastNodeIdentifier.Root);
                mapRoot.AddNode(@object.ToCast());
                Log.Information($"Added object {@object.Name} to map geometry root node.");
                CastWriter.Save(Path.Join(directory, "mapGeometry.cast"), mapRoot);
            }
            else
            {
                var file = Path.Combine(models, $"{@object.Name}.cast");
                CastNode modelRoot = new CastNode(CastNodeIdentifier.Root);
                modelRoot.AddNode(@object.ToCast());
                CastWriter.Save(file, modelRoot);
                Log.Information($"Saved object {@object.Name} to {file}.");
            }
        }

        CastNode propRoot = new CastNode(CastNodeIdentifier.Root);
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

        foreach(var instance in DynamicInstances)
        {
            var fileNode = new FileNode();
            fileNode.AddString("p", $"models/{instance.Name}.cast");

            var instanceNode = new InstanceNode();
            instanceNode.AddString("n", $"{instance.Name}_{count++}");
            instanceNode.AddValue("rf", fileNode.Hash);
            instanceNode.AddValue("p", instance.Position);
            instanceNode.AddValue("r", new Vector4(instance.Rotation.X, instance.Rotation.Y, instance.Rotation.Z, instance.Rotation.W));
            instanceNode.AddValue("s", instance.Scale);
            instanceNode.AddNode(fileNode);
            propRoot.AddNode(instanceNode);
        }

        CastWriter.Save(Path.Join(directory, "mapInstances.cast"), propRoot);

        var materials = Path.Combine(directory, "materials");
        if (!Directory.Exists(materials))
        {
            Directory.CreateDirectory(materials);
        }

        foreach (var material in Materials)
        {
            var semantic = new StringBuilder();
            semantic.AppendLine($"# Autogenerated material file for {material.Name}");
            foreach (var texture in material.Textures)
            {
                semantic.AppendLine($"{texture.Key}, {texture.Value}");
            }
            File.WriteAllText(Path.Combine(materials, $"{material.Name}.txt"), semantic.ToString());
        }
    }
}