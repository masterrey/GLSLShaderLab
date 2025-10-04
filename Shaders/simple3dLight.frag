#version 330 core
out vec4 FragColor;
 
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;
 
uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;
uniform sampler2D texture3;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture0;
 
vec3 lightDir = normalize(vec3(0.2, -1.0, -0.2));
float ambient = 0.2;
 
void main()
{
    // intensidade da luz
    float brightness = clamp(dot(normalize(Normal), -lightDir), 0.0, 1.0);

    // texturas
    vec4 texLight  = texture(texture2, TexCoord); // usada quando iluminado
    vec4 texShadow = texture(texture3, TexCoord); // usada quando em sombra
    vec4 texAnimA  = texture(texture1, TexCoord);
    vec4 texAnimB  = texture(texture0, TexCoord);

    // alterna entre textura de luz e sombra
    vec4 baseColor = mix(texShadow, texLight, brightness);

    // mantém sua lógica de mistura animada
    vec4 animMix = mix(texAnimA, texAnimB, 0.5 - 0.5 * cos(iTime));

    // cor final com ambiente
    FragColor = mix(baseColor, animMix, 0.0) * (brightness + ambient);
}
