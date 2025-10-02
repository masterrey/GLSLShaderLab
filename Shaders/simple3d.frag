#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;

uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;

uniform sampler2D texture3; // base
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture0; // textura que ta sobrepondo

vec3 lightDir = vec3(0.2, -1.0, -0.2);
float ambient = 0.2;

void main()
{
    float brightness = clamp(dot(Normal, -lightDir), 0.0, 1.0);

    vec4 baseTex = texture(texture1, TexCoord); // textura base
    vec4 tex1 = texture(texture1, TexCoord);
    vec4 overlayTex = texture(texture3, TexCoord); // textura que vai aparecer na luz

    vec4 color = mix(baseTex, tex1, 1.0 - cos(iTime) * 0.5);

    color *= (brightness + ambient);
    FragColor = mix(color, overlayTex, brightness);
}
