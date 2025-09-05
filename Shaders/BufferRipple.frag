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
 
float getWaterHeight(vec2 uv, vec2 center, float time)
{
    float total = 0.0;
    for (int i = 0; i < 5; i++)
    {
        float t = time * 1.5 - float(i) * 1.0;
        total += ripple(uv, center, t);
    }
    return total;
}
 
void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
 
    vec3 baseColor = vec3(0.2, 0.5, 0.8);
    vec3 color = baseColor;
 
    if (iMouseClick > 0)
    {
        vec2 center = iMouse.xy / iResolution.xy;
 
        float h = getWaterHeight(uv, center, iTime);
 
        float dx = getWaterHeight(uv + vec2(1.0 / iResolution.x, 0.0), center, iTime) - h;
        float dy = getWaterHeight(uv + vec2(0.0, 1.0 / iResolution.y), center, iTime) - h;
 
        vec3 normal = normalize(vec3(-dx, 0.2, -dy));
 
        vec2 distortion = vec2(dx, dy);
        vec2 distortedUV = uv + 0.05 * distortion;
 
        vec3 distortedColor = baseColor + 0.1 * vec3(distortion.x, distortion.y, 0.0);
 
        vec3 lightDir = normalize(vec3(-3.0, 10.0, 3.0));
        float light = pow(max(0.0, dot(normal, lightDir)), 60.0);
 
        color = mix(color, distortedColor, 0.5);
        color += vec3(1.0) * light;
    }
 
    FragColor = vec4(color, 1.0);
}