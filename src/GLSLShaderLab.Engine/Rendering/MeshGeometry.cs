using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GLSLShaderLab.Engine.Rendering;

internal sealed class MeshGeometry : IDisposable
{
    internal struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
    }

    private readonly List<Vertex> _vertices;
    private readonly List<uint> _indices;
    private int _vao;
    private int _vbo;
    private int _ebo;

    public MeshGeometry(List<Vertex> vertices, List<uint> indices)
    {
        _vertices = vertices;
        _indices = indices;
        SetupMesh();
    }

    private void SetupMesh()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        var vertexData = new float[_vertices.Count * 8];
        for (int i = 0; i < _vertices.Count; i++)
        {
            var vertex = _vertices[i];
            int offset = i * 8;
            vertexData[offset + 0] = vertex.Position.X;
            vertexData[offset + 1] = vertex.Position.Y;
            vertexData[offset + 2] = vertex.Position.Z;
            vertexData[offset + 3] = vertex.Normal.X;
            vertexData[offset + 4] = vertex.Normal.Y;
            vertexData[offset + 5] = vertex.Normal.Z;
            vertexData[offset + 6] = vertex.TexCoord.X;
            vertexData[offset + 7] = vertex.TexCoord.Y;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(0);
    }

    public void Render()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (_vao != 0) GL.DeleteVertexArray(_vao);
        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        if (_ebo != 0) GL.DeleteBuffer(_ebo);
        _vao = 0;
        _vbo = 0;
        _ebo = 0;
    }
}
