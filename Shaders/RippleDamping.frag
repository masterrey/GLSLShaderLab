#version 330 core
out vec4 FragColor;

uniform vec2  iResolution;
uniform vec2  iMouse;      
uniform int   iMouseClick;  
uniform sampler2D iChannel0;
uniform float iTime;

const float SCALE  = 1.003;
const float EDGE   = 0.01;
const float RADIUS = 0.02;  // raio fixo do ripple
const float RIPPLE_DURATION = 1.0; // tempo que o ripple dura (em segundos)

float softCircle(vec2 uvN, vec2 centerN, float r, float edge) {
    float d = distance(uvN, centerN);
    return 1.0 - smoothstep(r - edge, r, d);
}

void main() {
    vec2 uvN = gl_FragCoord.xy / iResolution.xy;
    vec2 center = iMouse / iResolution;

    float invScale = 1.0 / SCALE + cos(iTime * 100.0) * 0.001;
    vec2 scaledUV = center + (uvN - center) * invScale;

    vec3 colScaled   = texture(iChannel0, clamp(scaledUV, 0.0, 1.0)).rgb;
    vec3 colUnscaled = texture(iChannel0, uvN).rgb;

    vec2 ok = step(vec2(0.0), scaledUV) * step(scaledUV, vec2(1.0));
    float inside = ok.x * ok.y;
    vec3 color = mix(colUnscaled, colScaled, inside);

    if (iMouseClick == 1) {
        float timeSinceClick = mod(iTime, RIPPLE_DURATION);
        float opacity = 1.0 - (timeSinceClick / RIPPLE_DURATION);  // opacidade diminui com o tempo

        float a = softCircle(uvN, center, RADIUS, EDGE) * opacity;
        vec3 paint = 0.5 + 0.5 * cos(iTime + uvN.xyx + vec3(0, 2, 4));
        color = mix(color, paint, a);
    }

    FragColor = vec4(color, 1.0);
}