#version 330 core
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

out vec4 fragColor;

uniform float iTime;
uniform vec3 lightPos;
uniform vec3 viewPos;

vec3 stripeColor(float y) {

    
    if (y < 0.33)
        return vec3(0.0, 0.0, 0.0);    
    
    else if (y < 0.66)
        return vec3(1.0, 0.0, 0.0);    
    
    else
        return vec3(1.0, 0.8, 0.0);
}

void main()
{
    vec3 baseColor = stripeColor(TexCoord.y);

    vec3 norm = normalize(Normal); 

    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0); 

    vec3 ambient = 0.2 * baseColor;
    vec3 diffuse = diff * baseColor;
    vec3 specular = spec * vec3(0.5);

    fragColor = vec4(ambient + diffuse + specular, 1.0);
}