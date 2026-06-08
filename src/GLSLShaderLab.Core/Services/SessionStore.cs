using System;
using System.IO;
using System.Text.Json;
using GLSLShaderLab.Core.Models;

namespace GLSLShaderLab.Core.Services;

public sealed class SessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string SessionFilePath { get; }

    public SessionStore(string? sessionFilePath = null)
    {
        SessionFilePath = sessionFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GLSLShaderLab",
            "session.json");
    }

    public ShaderDocument LoadOrDefault()
    {
        try
        {
            if (!File.Exists(SessionFilePath))
            {
                return ShaderTemplateCatalog.CreateDefaultDocument();
            }

            var json = File.ReadAllText(SessionFilePath);
            var document = JsonSerializer.Deserialize<ShaderDocument>(json, JsonOptions);
            return document ?? ShaderTemplateCatalog.CreateDefaultDocument();
        }
        catch
        {
            return ShaderTemplateCatalog.CreateDefaultDocument();
        }
    }

    public void Save(ShaderDocument document)
    {
        var dir = Path.GetDirectoryName(SessionFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(SessionFilePath, json);
    }
}
