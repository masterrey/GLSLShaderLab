#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
in vec2 TexCoord;

void main()
{
    //vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec2 uv = TexCoord;
    FragColor = vec4(0,0,0.7,1.0);
    
    if(uv.x > 0.333){
    FragColor = vec4(1.0,1.0,1.0,1.0);
    }
    if(uv.x > 0.666){
    FragColor = vec4(0.9,0.0,0.0,1.0);
    }
}