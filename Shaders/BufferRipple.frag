#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform int iMouseClick;
 
float ripple(vec2 uv, vec2 center, float time)
{
    float dist = distance(uv, center);
    float wave = 0.03 * sin(40.0 * dist - 6.28 * time);
    float ripple = smoothstep(0.01, 0.015, dist + wave) - smoothstep(0.015, 0.02, dist + wave);
    return ripple;
}
 
void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec3 color = vec3(0.2, 0.5, 0.8); // Fundo azul
 
    if(iMouseClick > 0)
    {
        float totalRipples = 0.0;
        vec2 center = iMouse.xy / iResolution.xy;
 
        for (int i = 0; i < 5; i++)
        {
            float t = iTime * 1.5 - float(i) * 1.0;
            totalRipples += ripple(uv, center, t);
        }
 
        color += vec3(1.0, 1.0, 1.0) * totalRipples;
    }
 
    FragColor = vec4(color, 1.0);
}