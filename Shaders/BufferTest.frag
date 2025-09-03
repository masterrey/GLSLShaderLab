#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform int iMouseClick;
 
// Buffer test - simple feedback system
uniform sampler2D iChannel0;
 
void Circle(vec2 uv, vec2 center, float radius, vec3 input, out vec3 output)
{
        float dist = distance(uv, center);
        float circle = 1.0 - smoothstep(0.02, radius, dist);
        vec3 paintColor = vec3(1.0, 0.5, 0.0);
        output = mix(input, paintColor, circle);
}
 
void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec2 mouse = iMouse.xy / iResolution.xy;

    vec3 previous = texture(iChannel0, uv).rgb;
    vec3 color = previous;

    if (iMouseClick > 0.5) 
    {
        Circle(uv, mouse, 0.05, color,color);
    }
    
   
    FragColor = vec4(color, 1.0);
}