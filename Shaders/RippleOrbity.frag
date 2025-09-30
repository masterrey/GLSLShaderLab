#version 330 core
out vec4 FragColor;

uniform vec2  iResolution;
uniform vec2  iMouse;       
uniform float iMouseClick;  
uniform sampler2D iChannel0; 
uniform float iTime;

const float SCALE  = 1.003;
const float RADIUS = 0.04;  // maior raio
const float EDGE   = 0.01; 

float softCircle(vec2 uvN, vec2 centerN, float r, float edge) {
    float d = distance(uvN, centerN);
    return 1.0 - smoothstep(r - edge, r, d);
}

void main() {
    vec2 uvN = gl_FragCoord.xy / iResolution.xy;

    // Centro rotacionando em circulo ao redor do meio da tela
    vec2 center = vec2(0.5, 0.5) + 0.3 * vec2(cos(iTime), sin(iTime));

    float invScale = 1.0 / SCALE + cos(iTime * 100.0) * 0.001;
    vec2 scaledUV = center + (uvN - center) * invScale;

    vec3 colScaled   = texture(iChannel0, clamp(scaledUV, 0.0, 1.0)).rgb;
    vec3 colUnscaled = texture(iChannel0, uvN).rgb;

    vec2 ok = step(vec2(0.0), scaledUV) * step(scaledUV, vec2(1.0));
    float inside = ok.x * ok.y;
    vec3 color = mix(colUnscaled, colScaled, inside);

    // Ripple local aparece enquanto o clique está pressionado
    if (iMouseClick == 1) {
        vec2 mN = iMouse / iResolution;
        float a = softCircle(uvN, mN, RADIUS, EDGE);
        // paint mais vibrante
        vec3 paint = 0.5 + 0.7 * cos(iTime + uvN.xyx * 10.0 + vec3(0, 2, 4));
        // mistura mais pesada pra destacar
        color = mix(color, paint, a * 0.8);
    }

    FragColor = vec4(color, 1.0);
}
