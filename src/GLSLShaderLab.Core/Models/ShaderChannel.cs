namespace GLSLShaderLab.Core.Models;

public sealed class ShaderChannel
{
    public int Index { get; set; }
    public string? TexturePath { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(TexturePath)
        ? $"iChannel{Index}"
        : $"iChannel{Index} ({System.IO.Path.GetFileName(TexturePath)})";
}
