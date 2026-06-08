using System;
using OpenTK.Graphics.OpenGL4;

namespace GLSLShaderLab.Engine.Rendering;

internal sealed class RenderBuffer : IDisposable
{
    public int TextureId { get; }
    public int FramebufferId { get; }
    public int DepthBufferId { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public RenderBuffer(int width, int height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);

        TextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        FramebufferId = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferId);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureId, 0);

        DepthBufferId = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBufferId);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthBufferId);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"Framebuffer creation failed: {status}");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Resize(int width, int height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBufferId);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(FramebufferId);
        GL.DeleteRenderbuffer(DepthBufferId);
        GL.DeleteTexture(TextureId);
    }
}
