#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    FragColor = vec4(0.85, 0.16, 0.11, 1.0);
    
    if(uv.y < 0.62 && uv.y > 0.41 && uv.x > 0.24 && uv.x < 0.72){
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }

    if(uv.y < 0.84 && uv.y > 0.18 && uv.x > 0.41 && uv.x < 0.55 ){
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
}