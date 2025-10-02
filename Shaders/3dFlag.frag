#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;
in vec3 LocalPos;

uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;

vec3 lightDir = vec3(0.2,-1.0,-0.2);
float ambient = 0.2;

void main()
{

    float brightness = clamp(dot(Normal, -lightDir), 0.0, 1.0);


    float stripe = (LocalPos.x + 1.0) * 0.5;

    if (stripe < 0.33)
        FragColor = vec4(0.027, 0.412, 0.157, 1.0) * (brightness + ambient); // green
    else if (stripe < 0.66)
        FragColor = vec4(1.0, 1.0, 1.0, 1.0) * (brightness + ambient);       // white
    else
        FragColor = vec4(0.871, 0.094, 0.024, 1.0) * (brightness + ambient); // red
    
}