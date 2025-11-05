#version 330 core
 
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;
 
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float iTime;
 
out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;
 
void main()
{
    vec3 pos = aPos;
 
    float wave_x = sin(iTime * 2.5 + aPos.x * 3.0) * 0.1;
    float wave_y = cos(iTime * 1.5 + aPos.y * 5.0) * 0.05;
 
    float displacement = wave_x + wave_y;
    pos.z += displacement;
 
    Normal = mat3(transpose(inverse(model))) * aNormal;
 
    FragPos = vec3(model * vec4(pos, 1.0));
    TexCoord = aTexCoord;
 
    gl_Position = projection * view * vec4(FragPos, 1.0);
}