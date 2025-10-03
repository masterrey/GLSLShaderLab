#version 330 core
out vec4 FragColor;

uniform vec2  iResolution;
uniform vec2  iMouse; 
uniform int   iMouseClick;
uniform sampler2D iChannel0;
uniform float iTime; 

const float SCALE  = 1.001;
const float RADIUS = 0.03;
const float EDGE   = 0.02;

// funcao que cria uma mascara circular suave no ponto centerN, 
// retorna 1 no centro e cai suavemente ate 0 no raio r com borda edge
float softCircle(vec2 uvN, vec2 centerN, float r, float edge) {
    float d = distance(uvN, centerN);
    return 1.0 - smoothstep(r - edge, r, d);
}

void main() {
    vec2 uvN = gl_FragCoord.xy / iResolution.xy;

    vec2 mouseNorm = iMouse / iResolution;
    float orbitRadius = 0.05;
    float speed = 1.0;
    
    // calcula o centro do ripple orbitando ao redor do mouse
    vec2 center = mouseNorm + vec2(cos(iTime * speed), sin(iTime * speed)) * orbitRadius;
    
    float invScale = 1.0 / SCALE;
    vec2 scaledUV = center + (uvN - center) * invScale;
    
    vec3 colScaled = texture(iChannel0, clamp(scaledUV, 0.0, 1.0)).rgb;
    vec3 colUnscaled = texture(iChannel0, uvN).rgb;

    vec2 ok = step(vec2(0.0), scaledUV) * step(scaledUV, vec2(1.0));
    float inside = ok.x * ok.y;
    vec3 color = mix(colUnscaled, colScaled, inside);
    
    // se o mouse estiver clicado, pinta um efeito circular no centro do ripple
    if (iMouseClick == 1) {
        float a = softCircle(uvN, center, RADIUS, EDGE);   // calcula a mascara suave da bolinha
        vec3 paint = 0.5 + 0.5 * cos(iTime + uvN.xyx + vec3(0, 2, 4));
        color = mix(color, paint, a);
    }

    FragColor = vec4(color, 1.0);  // define a cor final do pixel
}