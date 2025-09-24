#version 330 core
out vec4 FragColor;
 
in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
in vec3 WorldPos;
 
uniform float iTime;
uniform vec2 iResolution;
uniform vec3 viewPos;
 
vec3 ligthDir = vec3(0.2,-1.0,-0.2);
float ambient - 0.2

void main()
{
    //dot product of normal and light direction to get brightness 
    // clamp between 0 and 1
    float brightness = clamp(0,dot(Normal, -lightDir),0,1);
    //set color to white with brightness and add ambient
    FragColor = vec4(1.0,1.0,1.0,1.0) * (brightness + 0.2);
}