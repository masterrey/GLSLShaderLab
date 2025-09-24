#version 330 core
out vec4 FragColor;

uniform vec2  iResolution;
uniform vec2  iMouse;
uniform int   iMouseClick;
uniform sampler2D iChannel0;
uniform float iTime;

in vec3 FragPos;
in vec3 Normal;
in vec3 WorldPos;
uniform vec3 viewPos;

in vec2 TexCoord;

const float SCALE  = 1.001;
const float RADIUS = 0.2;
const float EDGE   = 0.02;

const float RIPPLE_SPEED = 5.0;
const float RIPPLE_FREQ = 40.0;
const float RIPPLE_AMPLITUDE = 0.13;

vec3 lightDir = vec3(0.2, -1.0, 0.0);
float ambient = 3;

float softCircle(vec2 uv, vec2 center, float r, float edge) {
    float d = distance(uv, center);
    return 1.0 - smoothstep(r - edge, r, d);
}

vec3 proceduralBackground(vec2 uv, float time) {
    float spacing = 0.1;
    vec2 animatedOffset = vec2(sin(time * 0.5), cos(time * 0.3)) * 0.02;
    vec2 uvAnim = uv + animatedOffset;
    vec2 gridUV = mod(uvAnim, spacing) / spacing;
    vec2 cellCenter = vec2(0.5);
    float pulse = 0.3 + 0.05 * sin(time * 4.0 + uv.x * 10.0);
    float d = distance(gridUV, cellCenter);
    float circle = 1.0 - smoothstep(pulse, pulse + 0.02, d);
    vec3 base = vec3(0.1, 0.1, 0.15) + 0.05 * sin(uv.xyx * 20.0 + time);
    vec3 dots = vec3(0.9, 0.2, 0.4) * circle;
    return base + dots;
}

void main() {
    vec2 center = iMouse / iResolution;
    vec2 uvN = TexCoord;

    // Feedback (zoom ripple)
    float invScale = 1.0 / SCALE;
    vec2 scaledUV = center + (uvN - center) * invScale;
    vec3 colScaled   = texture(iChannel0, clamp(scaledUV, 0.0, 1.0)).rgb;
    vec3 colUnscaled = texture(iChannel0, uvN).rgb;
    vec2 ok = step(vec2(0.0), scaledUV) * step(scaledUV, vec2(1.0));
    float inside = ok.x * ok.y;
    vec3 feedbackColor = mix(colUnscaled, colScaled, inside);

    // Procedural ripple only on mouse click
    vec2 rippleUV = uvN;
    if (iMouseClick == 1) {
        float dist = distance(uvN, center);
        float ripple = sin(dist * RIPPLE_FREQ - iTime * RIPPLE_SPEED);
        ripple *= smoothstep(RADIUS, 0.0, dist);  // fade out with distance
        ripple *= RIPPLE_AMPLITUDE;
        vec2 dir = normalize(uvN - center + 0.0001); // avoid NaN
        rippleUV += dir * ripple;
    }

    // Deformed procedural background
    vec3 procedural = proceduralBackground(rippleUV, iTime);

    // Final mix
    vec3 baseColor = mix(feedbackColor, procedural, 0.5);

    float brightness = clamp(dot(Normal, -lightDir), 0.0, 0.1);
    baseColor *= (brightness + ambient);

    FragColor = vec4(baseColor, 1.0);
}
