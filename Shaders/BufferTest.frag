#version 330 core
out vec4 FragColor;

uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;

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
    // Read previous frame
    vec3 previous = texture(iChannel0, uv).rgb;
    // Start with previous frame (for persistence)
    vec3 color = previous;
    // Fade slightly over time (optional)
    //color *= 0.99;
    // Add new content when clicking
    if (iMouseClick > 0.5) {
        Circle(uv, mouse, 0.05, color,color);
    }else{
        // Always show mouse position
        float mouseDist = distance(uv, mouse);
        float mouseIndicator = 1.0 - smoothstep(0.002, 0.005, mouseDist);
        color = mix(color, vec3(0.0, 1.0, 1.0), mouseIndicator * 0.8);
