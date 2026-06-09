using System.Collections.Generic;
using GLSLShaderLab.Core.Models;

namespace GLSLShaderLab.Core.Services;

public static class ShaderTemplateCatalog
{
    public const string FullscreenVertex = """
#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTex;
out vec2 vTex;
void main()
{
    vTex = aTex;
    gl_Position = vec4(aPos, 0.0, 1.0);
}
""";

    public const string ModelVertex = """
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;
out vec3 WorldPos;
out vec3 VertexPos;
out vec3 vertex;

void main()
{
    WorldPos = vec3(model * vec4(aPos, 1.0));
    FragPos = WorldPos;
    Normal = mat3(transpose(inverse(model))) * aNormal;
    TexCoord = aTexCoord;
    VertexPos = aPos;
    vertex = aPos;
    gl_Position = projection * view * vec4(FragPos, 1.0);
}
""";

    public static IReadOnlyList<ShaderTemplate> Templates { get; } =
    [
        new("Empty", EmptyTemplate),
        new("Feedback", FeedbackTemplate),
        new("Noise", NoiseTemplate)
    ];

    public static ShaderDocument CreateDefaultDocument() => new()
    {
        Name = "New Shader",
        VertexSource = ModelVertex,
        FragmentSource = EmptyTemplate
    };

    private const string EmptyTemplate = """
#version 330 core
out vec4 FragColor;

uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform int iMouseClick;
uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec3 col = 0.5 + 0.5*cos(iTime + uv.xyx + vec3(0.0, 2.0, 4.0));
    FragColor = vec4(col, 1.0);
}
""";

    private const string FeedbackTemplate = """
#version 330 core
out vec4 FragColor;

uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform int iMouseClick;
uniform sampler2D iChannel0;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec3 prev = texture(iChannel0, uv).rgb;
    vec3 col = mix(prev * 0.985, vec3(0.0), 0.01);

    if (iMouseClick == 1)
    {
        vec2 m = iMouse / iResolution;
        float d = length(uv - m);
        col += smoothstep(0.12, 0.0, d) * vec3(1.0, 0.4, 0.2);
    }

    col += 0.01 * vec3(sin(iTime * 0.5), cos(iTime * 0.8), sin(iTime));
    FragColor = vec4(col, 1.0);
}
""";

    private const string NoiseTemplate = """
#version 330 core
out vec4 FragColor;

uniform float iTime;
uniform vec2 iResolution;

float hash(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 34.345);
    return fract(p.x * p.y);
}

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    float n = hash(floor(uv * 250.0) + iTime);
    FragColor = vec4(vec3(n), 1.0);
}
""";
}
