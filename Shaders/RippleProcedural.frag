#version 330 core
out vec4 FragColor;

uniform vec2  iResolution;
uniform vec2  iMouse;       
uniform float iMouseClick;  
uniform sampler2D iChannel0; 
uniform float iTime;

const float SCALE  = 1.003;
const float RADIUS = 0.02; 
const float EDGE   = 0.01; 

float softCircle(vec2 uvN, vec2 centerN, float r, float edge) {
    float d = distance(uvN, centerN);
    return 1.0 - smoothstep(r - edge, r, d);
}

void main() {
    vec2 uvN = gl_FragCoord.xy / iResolution.xy;
    vec2 center = iMouse / iResolution;

    float invScale = 1.0 / SCALE + cos(iTime * 100.0) * 0.001;
    vec2 scaledUV = center + (uvN - center) * invScale;

    // Fundo procedural original
    vec3 procBg = 0.5 + 0.5 * cos(iTime + uvN.xyx * 10.0 + vec3(0, 2, 4));

    // Fundo procedural deformado pelo ripple (usando scaledUV)
    vec3 procBgRippled = 0.5 + 0.5 * cos(iTime + scaledUV.xyx * 10.0 + vec3(0, 2, 4));

    float rippleArea = 0.0;
    if (iMouseClick == 1) {
        rippleArea = softCircle(uvN, center, RADIUS, EDGE);
    }

    // Mistura entre o fundo normal e o deformado pelo ripple
    vec3 color = mix(procBg, procBgRippled, rippleArea);

    FragColor = vec4(color, 1.0);
}
