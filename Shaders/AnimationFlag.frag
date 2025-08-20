#version 330 core
out vec4 FragColor;
// uniform is a external variable that is set by the application
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    float wave = (cos((iTime + uv.x) * 2) * 0.2) + 0.2;
    FragColor = vec4(0.89, 0.10, 0.27, 1.0);

    if (uv.y < wave) {
    // Top half: white
    FragColor = vec4(0.89, 0.10, 0.27, 1.0);
    }

    if (uv.y < wave + 0.2) {
    // Top half: blue
    FragColor = vec4(0.0, 0.60, 0.89, 1.0);
    }
    
    else if (uv.y <wave + 0.45) {
    // Top half: blue
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }

}
