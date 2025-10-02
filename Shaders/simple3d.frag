#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;

uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;

uniform sampler2D texture1;         // textura base
uniform sampler2D projectedTexture; // textura projetada pela luz

uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;

vec3 lightDir = normalize(vec3(0.2, -1.0, -0.2));
float ambient = 0.2;

void main()
{
    float brightness = clamp(dot(Normal, -lightDir), 0.0, 1.0);

    vec4 lightSpacePos = lightProjectionMatrix * lightViewMatrix * vec4(WorldPos, 1.0);

    vec3 projCoords = lightSpacePos.xyz / lightSpacePos.w;

    vec2 lightTexCoords = projCoords.xy * 0.5 + 0.5;

    bool inProjection = lightTexCoords.x >= 0.0 && lightTexCoords.x <= 1.0 &&
                        lightTexCoords.y >= 0.0 && lightTexCoords.y <= 1.0;

    vec4 projectedTexColor = vec4(1.0);

    if(inProjection) {
        projectedTexColor = texture(projectedTexture, lightTexCoords);
    }

    vec4 baseTex = texture(texture1, TexCoord);

    FragColor = baseTex * (brightness + ambient) * projectedTexColor;
}
