namespace GLSLShaderLab.Core.Models;

public sealed record ShaderCompileMessage(bool Success, string Message, string? Stage = null, int? Line = null);
