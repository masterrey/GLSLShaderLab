#version 330 core
out vec4 fragColor;

in vec2 TexCoord;

uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = TexCoord;
    vec3 col = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0, 2, 4));
    fragColor = vec4(col, 1.0);
}
