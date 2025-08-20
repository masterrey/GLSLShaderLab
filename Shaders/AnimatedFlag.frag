#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    float wave = (cos((iTime + uv.x) * 2) * 0.1) + 0.1;

    FragColor = vec4(0.0, 0.533, 0.8, 1.0);

    if(uv.y < wave) {
        // top half: blue
        FragColor = vec4(0.0, 0.533, 0.8, 1.0);
    }
    if(uv.y < wave+0.2) {
        // top half: blue
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }

    else if(uv.y < wave+0.4) {
        // top half: blue
        FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
    
}
