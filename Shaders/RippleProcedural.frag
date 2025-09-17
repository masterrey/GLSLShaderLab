#version 330 core
out vec4 FragColor;

uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;
uniform float iTime;

// Fundo procedural simples, tipo ondas suaves 
vec3 proceduralBackground(vec2 uv) {
    float wave = 0.5 + 0.5 * sin(10.0 * uv.x + iTime * 0.5) * cos(10.0 * uv.y + iTime * 0.5);
    return mix(vec3(0.1, 0.15, 0.3), vec3(0.3, 0.45, 0.7), wave);
}

float circleMask(vec2 uv, vec2 center, float radius, float edge) {
    float d = distance(uv, center);
    return 1.0 - smoothstep(radius - edge, radius, d);
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec3 color = proceduralBackground(uv);

    if(iMouseClick > 0.5) {
        vec2 mouseUV = iMouse / iResolution;
        float mask = circleMask(uv, mouseUV, 0.1, 0.02);
        if(mask > 0.0) {
            float dist = distance(uv, mouseUV);
            float ripple = 0.05 * sin(40.0 * dist - iTime * 5.0) * mask; // velocidade e intensidade menores

            vec2 dir = normalize(uv - mouseUV);
            vec2 rippleUV = uv + dir * ripple;

            color = proceduralBackground(rippleUV);
        }
    }

    FragColor = vec4(color, 1.0);
}
