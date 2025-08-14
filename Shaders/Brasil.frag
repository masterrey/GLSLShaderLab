#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    float prop = iResolution.x / iResolution.y;
    float x = length(vec2(uv.x * prop, uv.y) - vec2(prop / 2.0, 0.5));

    FragColor = vec4(0.0, 0.6, 0.2, 1.0);

    float dx = abs(uv.x - 0.5);
    float dy = abs(uv.y - 0.5);

    if (dy < (-dx * 0.8 + 0.3))
    {
        FragColor = vec4(1.0, 0.85, 0.0, 1.0);
    }

    if (x < 0.2)
    { 
        FragColor = vec4(0.0, 0.2, 0.6, 1.0);
    }
}