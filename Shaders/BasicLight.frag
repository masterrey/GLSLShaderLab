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

const float RIPPLE_SPEED     = 5.0;
const float RIPPLE_FREQ      = 40.0;
const float RIPPLE_AMPLITUDE = 0.13;

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

    // ===== ZOOM FEEDBACK (efeito de zoom suave com ripple) =====
    float invScale = 1.0 / SCALE;
    vec2 scaledUV = center + (uvN - center) * invScale;
    vec3 colScaled   = texture(iChannel0, clamp(scaledUV, 0.0, 1.0)).rgb;
    vec3 colUnscaled = texture(iChannel0, uvN).rgb;
    vec2 ok = step(vec2(0.0), scaledUV) * step(scaledUV, vec2(1.0));
    float inside = ok.x * ok.y;
    vec3 feedbackColor = mix(colUnscaled, colScaled, inside);

    // ===== RIPPLE COM O CLIQUE =====
    vec2 rippleUV = uvN;
    if (iMouseClick == 1) {
        float dist = distance(uvN, center);
        float ripple = sin(dist * RIPPLE_FREQ - iTime * RIPPLE_SPEED);
        ripple *= smoothstep(RADIUS, 0.0, dist);  // fade out
        ripple *= RIPPLE_AMPLITUDE;
        vec2 dir = normalize(uvN - center + 0.0001); // evita NaN
        rippleUV += dir * ripple;
    }

    // ===== BACKGROUND DEFORMADO =====
    vec3 procedural = proceduralBackground(rippleUV, iTime);

    // ===== MISTURA FINAL DA COR BASE =====
    vec3 baseColor = mix(feedbackColor, procedural, 0.5);

    vec3 norm = normalize(Normal);

    vec2 mouseNDC = (iMouse / iResolution) * 2.0 - 1.0; // NDC [-1,1]
    vec3 lightPos = normalize(vec3(-mouseNDC.x, -mouseNDC.y, 1.0)) * 5.0;

    vec3 lightDir = normalize(lightPos - FragPos);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

    // Componente difusa
    float diff = max(dot(norm, lightDir), 0.0);

    // Componente especular
    float specStrength = 1.0;
    float shininess = 32.0;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);

    // Componentes finais
    float ambient = 0.2;
    vec3 lightColor = vec3(1.0); // luz branca
    vec3 lighting = ambient * lightColor + diff * lightColor + specStrength * spec * lightColor;

    vec3 finalColor = baseColor * lighting;

    FragColor = vec4(finalColor, 1.0);
}