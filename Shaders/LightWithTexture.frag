#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;

uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;
uniform sampler2D texture0; 
uniform sampler2D texture1;

vec3 lightDir = vec3(0.2, -1.0, -0.2);
float ambient = 0.2;

void main()
{
    float brightness = clamp(dot(normalize(Normal), normalize(-lightDir)), 0.0, 1.0);

    vec4 texColorLight = texture(texture0, TexCoord); 
    vec4 texColorDark = texture(texture1, TexCoord);

    vec4 finalColor = mix(texColorDark, texColorLight, brightness);

    finalColor *= (brightness + ambient);

    FragColor = finalColor;
}