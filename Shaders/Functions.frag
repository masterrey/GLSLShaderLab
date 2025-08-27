#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;
uniform int iShapeType;

// Funções de desenho (circle, square, triangle) aqui...

vec4 DrawCircle(vec2 uv, vec3 color, vec2 pos, float radius, float edge) {
    vec2 posn = pos / iResolution.xy;
    float dist = distance(uv, posn);
    float alpha = smoothstep(radius, radius - edge, dist);
    return vec4(color, alpha);
}

vec4 DrawSquare(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution.xy;
    vec2 diff = abs(uv - posn);
    vec2 dist = diff - vec2(size);
    float alpha = 1.0 - smoothstep(0.0, edge, max(dist.x, dist.y));
    return vec4(color, alpha);
}

vec4 DrawTriangle(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution.xy;
    vec2 p = uv - posn;
    float k = sqrt(3.0);
    p.x = abs(p.x) * 2.0;
    p.y = p.y + size / k;

    float d = max(p.x * 0.5 + k * p.y, -k * p.y);
    float alpha = 1.0 - smoothstep(size * 0.5, size * 0.5 + edge, d);

    return vec4(color, alpha);
}

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec3 baseColor = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0, 2, 4));

    vec4 shape;

    if (iShapeType == 0)
        shape = DrawCircle(uv, vec3(0.0, 0.0, 1.0), iMouse, 0.1, 0.01);
    else if (iShapeType == 1)
        shape = DrawSquare(uv, vec3(0.0, 0.0, 1.0), iMouse, 0.1, 0.01);
    else if (iShapeType == 2)
        shape = DrawTriangle(uv, vec3(0.0, 0.0, 1.0), iMouse, 0.1, 0.01);
    else
        shape = vec4(0.0);

    shape.a *= iMouseClick;

    FragColor = mix(vec4(baseColor, 1.0), shape, shape.a);
}
