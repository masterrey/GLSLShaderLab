using System;
using System.Collections.Generic;
using System.IO;
using GLSLShaderLab.Core.Models;
using GLSLShaderLab.Core.Services;
using GLSLShaderLab.Engine.Rendering;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GLSLShaderLab.Engine.Services;

public sealed class ShaderToyRenderer : IDisposable
{
    private int _vao;
    private int _vbo;
    private int _ebo;
    private ShaderProgram? _shader;
    private ShaderProgram? _copyShader;
    private RenderBuffer? _frontBuffer;
    private RenderBuffer? _backBuffer;
    private readonly Dictionary<int, int> _channelTextures = new();
    private bool _initialized;
    private int _width;
    private int _height;
    private float _time;
    private bool _isPaused;
    private float _mouseX;
    private float _mouseY;
    private bool _mouseDown;

    public RenderMode Mode { get; private set; } = RenderMode.TwoD;

    public ShaderCompileMessage LastCompileMessage { get; private set; } = new(true, "Ready");

    public void Initialize(int width, int height, string initialFragment)
    {
        if (_initialized) return;

        _width = Math.Max(1, width);
        _height = Math.Max(1, height);

        CreateFullscreenQuad();
        _frontBuffer = new RenderBuffer(_width, _height);
        _backBuffer = new RenderBuffer(_width, _height);

        var copyFragment = """
#version 330 core
out vec4 FragColor;
uniform sampler2D inputTexture;
uniform vec2 iResolution;
void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution;
    FragColor = texture(inputTexture, uv);
}
""";

        var (copyProgram, copyResult) = ShaderProgram.TryCreate(ShaderTemplateCatalog.FullscreenVertex, copyFragment);
        _copyShader = copyProgram;
        LastCompileMessage = copyResult;

        Compile(initialFragment);
        _initialized = true;
    }

    public ShaderCompileMessage Compile(string fragmentSource)
    {
        if (!_initialized && _copyShader == null)
        {
            LastCompileMessage = new ShaderCompileMessage(false, "Renderer not initialized.");
            return LastCompileMessage;
        }

        var (newProgram, result) = ShaderProgram.TryCreate(ShaderTemplateCatalog.FullscreenVertex, fragmentSource);
        if (newProgram is not null)
        {
            _shader?.Dispose();
            _shader = newProgram;
        }

        LastCompileMessage = result;
        return LastCompileMessage;
    }

    public void SetPaused(bool paused) => _isPaused = paused;

    public void ResetTime() => _time = 0f;

    public void SetMouse(float x, float y, bool isDown)
    {
        _mouseX = x;
        _mouseY = y;
        _mouseDown = isDown;
    }

    public void SetRenderMode(RenderMode mode)
    {
        Mode = mode;
    }

    public bool TrySetChannelTexture(int channel, string? path, out string message)
    {
        if (channel < 0 || channel > 3)
        {
            message = "Channel must be between 0 and 3.";
            return false;
        }

        if (_channelTextures.TryGetValue(channel, out int oldTex))
        {
            GL.DeleteTexture(oldTex);
            _channelTextures.Remove(channel);
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            message = $"Cleared iChannel{channel}.";
            return true;
        }

        if (!File.Exists(path))
        {
            message = $"Texture file not found: {path}";
            return false;
        }

        try
        {
            using var image = Image.Load<Rgba32>(path);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            _channelTextures[channel] = texId;
            message = $"Loaded iChannel{channel}: {Path.GetFileName(path)}";
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public void Resize(int width, int height)
    {
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);

        _frontBuffer?.Resize(_width, _height);
        _backBuffer?.Resize(_width, _height);
        GL.Viewport(0, 0, _width, _height);
    }

    public void Render(double elapsedSeconds)
    {
        if (!_initialized || _shader is null || _copyShader is null || _frontBuffer is null || _backBuffer is null)
        {
            return;
        }

        if (!_isPaused)
        {
            _time += (float)elapsedSeconds;
        }

        RenderToBuffer(_frontBuffer, _backBuffer.TextureId);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, _width, _height);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _copyShader.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _frontBuffer.TextureId);
        _copyShader.SetInt("inputTexture", 0);
        _copyShader.SetVector2("iResolution", _width, _height);

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
    }

    private void RenderToBuffer(RenderBuffer target, int previousFrameTexture)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, target.FramebufferId);
        GL.Viewport(0, 0, _width, _height);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _shader!.Use();
        _shader.SetFloat("iTime", _time);
        _shader.SetVector2("iResolution", _width, _height);
        _shader.SetVector2("iMouse", _mouseX, _height - _mouseY);
        _shader.SetInt("iMouseClick", _mouseDown ? 1 : 0);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);
        _shader.SetInt("iChannel0", 0);

        for (int i = 1; i <= 3; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + i);
            if (_channelTextures.TryGetValue(i, out int channelTex))
            {
                GL.BindTexture(TextureTarget.Texture2D, channelTex);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, previousFrameTexture);
            }

            _shader.SetInt($"iChannel{i}", i);
        }

        if (_channelTextures.TryGetValue(0, out int texture0))
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture0);
            _shader.SetInt("iChannel0", 0);
        }

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    private void CreateFullscreenQuad()
    {
        float[] vertices =
        [
            -1f, -1f, 0f, 0f,
             1f, -1f, 1f, 0f,
             1f,  1f, 1f, 1f,
            -1f,  1f, 0f, 1f
        ];

        uint[] indices = [0, 1, 2, 2, 3, 0];

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        int stride = 4 * sizeof(float);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        _shader?.Dispose();
        _copyShader?.Dispose();

        foreach (var tex in _channelTextures.Values)
        {
            GL.DeleteTexture(tex);
        }

        _channelTextures.Clear();

        _frontBuffer?.Dispose();
        _backBuffer?.Dispose();

        if (_vao != 0) GL.DeleteVertexArray(_vao);
        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        if (_ebo != 0) GL.DeleteBuffer(_ebo);
    }
}
