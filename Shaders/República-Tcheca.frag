#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    FragColor = vec4(0.769, 0.071, 0.188, 1.0);
    if(uv.y > 0.5){
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
    if(uv.y > 1.0 * uv.x && uv.y < 1.0 - 1.0 * uv.x){
    FragColor = vec4(0.04, 0.138, 0.367, 1.0);
    }

}