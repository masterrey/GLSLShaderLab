using System;
using System.Collections.Generic;
using System.IO;
using Assimp;
using OpenTK.Mathematics;

namespace GLSLShaderLab.Engine.Rendering;

internal sealed class ModelGeometry : IDisposable
{
    private readonly List<MeshGeometry> _meshes = [];

    public ModelGeometry(string path)
    {
        LoadModel(path);
    }

    private void LoadModel(string path)
    {
        var importer = new AssimpContext();
        importer.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));

        var scene = importer.ImportFile(path,
            PostProcessSteps.Triangulate |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.GenerateNormals);

        if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
        {
            throw new InvalidOperationException($"Failed to load model: {Path.GetFileName(path)}");
        }

        ProcessNode(scene.RootNode, scene);
    }

    private void ProcessNode(Node node, Scene scene)
    {
        for (int i = 0; i < node.MeshCount; i++)
        {
            var mesh = scene.Meshes[node.MeshIndices[i]];
            _meshes.Add(ProcessMesh(mesh));
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            ProcessNode(node.Children[i], scene);
        }
    }

    private static MeshGeometry ProcessMesh(Assimp.Mesh mesh)
    {
        var vertices = new List<MeshGeometry.Vertex>();
        var indices = new List<uint>();

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            var vertex = new MeshGeometry.Vertex();

            if (mesh.HasVertices)
            {
                vertex.Position = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);
            }

            if (mesh.HasNormals)
            {
                vertex.Normal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
            }
            else
            {
                vertex.Normal = Vector3.UnitY;
            }

            if (mesh.HasTextureCoords(0))
            {
                vertex.TexCoord = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
            }
            else
            {
                vertex.TexCoord = Vector2.Zero;
            }

            vertices.Add(vertex);
        }

        for (int i = 0; i < mesh.FaceCount; i++)
        {
            var face = mesh.Faces[i];
            for (int j = 0; j < face.IndexCount; j++)
            {
                indices.Add((uint)face.Indices[j]);
            }
        }

        return new MeshGeometry(vertices, indices);
    }

    public void Render()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Render();
        }
    }

    public void Dispose()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Dispose();
        }

        _meshes.Clear();
    }
}
