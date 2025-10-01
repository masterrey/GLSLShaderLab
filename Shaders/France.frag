#version 330 core
out vec4 FragColor;

uniform vec3 viewPos;

in vec2 TexCoord;
in vec3 FragPos;
in vec3 Normal;

void main()
{

    vec2 uv = TexCoord;

    vec3 flagColor = vec3(0.0, 0.0, 0.7);       // azul
    if (uv.x > 0.333) flagColor = vec3(1.0, 1.0, 1.0); // branco
    if (uv.x > 0.666) flagColor = vec3(0.9, 0.0, 0.0); // vermelho

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(vec3(0.2, -1.0, -0.2));
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

    float diff = max(dot(norm, lightDir), 0.0);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);

    float ambient = 0.2;
    vec3 lightColor = vec3(1.0);

    vec3 lighting = ambient * lightColor + diff * lightColor + 0.5 * spec * lightColor;
    vec3 finalColor = flagColor * lighting;

    FragColor = vec4(finalColor, 1.0);
}
