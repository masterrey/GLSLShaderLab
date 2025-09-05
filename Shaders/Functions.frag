#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;

vec4 DrawCircle(vec2 uv, vec3 color, vec2 pos, float radius, float edge) {
    vec2 posn = pos / iResolution;
    float dist = distance(uv, posn);
    float alpha = smoothstep(radius, radius - edge, dist);
    return vec4(color, alpha);
}

vec4 DrawSquare(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution;
    vec2 diff = abs(uv - posn);
    vec2 dist = diff - vec2(size);
    float alpha = 1.0 - smoothstep(0.0, edge, max(dist.x, dist.y));
    return vec4(color, alpha);
}

vec4 DrawTriangle(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution;
    vec2 p = uv - posn;
    float k = sqrt(3.0);
    p.x = abs(p.x) * 2.0;
    p.y = p.y + size / k;
    float d = max(p.x * 0.5 + k * p.y, -k * p.y);
    float alpha = 1.0 - smoothstep(size * 0.5, size * 0.5 + edge, d);
    return vec4(color, alpha);
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution;
    vec3 baseColor = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0.0, 2.0, 4.0));
    vec4 shape = vec4(0.0);

    // so desenha forma se mouse estiver clicado
    if (iMouseClick > 0.5) {
        // dividir a tela em 3 áreas verticais iguais
        float areaWidth = iResolution.x / 3.0;

        if (iMouse.x < areaWidth) {
            // area 1: círculo azul
            shape = DrawCircle(uv, vec3(0.0, 0.0, 1.0), iMouse, 0.1, 0.01);
        } else if (iMouse.x < 2.0 * areaWidth) {
            // area 2: quadrado verde
            shape = DrawSquare(uv, vec3(0.0, 1.0, 0.0), iMouse, 0.1, 0.01);
        } else {
            // area 3: triangulo vermelho
            shape = DrawTriangle(uv, vec3(1.0, 0.0, 0.0), iMouse, 0.1, 0.01);
        }
    }

    FragColor = mix(vec4(baseColor, 1.0), shape, shape.a);
}
