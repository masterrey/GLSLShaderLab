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

float softCircle(vec2 uv, vec2 center, float radius, float edge) {
    float d = distance(uv, center);
    return 1.0 - smoothstep(radius - edge, radius, d);
}

vec2 mirrorX(vec2 uv) {
    return vec2(abs(uv.x - 0.5) + 0.5, uv.y);
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    vec2 center = iMouse / iResolution;
    vec2 uvMirrored = mirrorX(uv);
    vec2 centerMirrored = mirrorX(center);

    float invScale = 1.0 / SCALE;
    vec2 zoomedUV = centerMirrored + (uvMirrored - centerMirrored) * invScale;

    vec3 col = texture(iChannel0, clamp(zoomedUV, 0.0, 1.0)).rgb;

    if (iMouseClick == 1) {
        vec2 m = iMouse / iResolution;
        vec2 mMirror = mirrorX(m);

        // Agora pintamos em ambos os lados:
        float a1 = softCircle(uv, m, RADIUS, EDGE);
        float a2 = softCircle(uv, mMirror, RADIUS, EDGE);
        float a = max(a1, a2);

        vec3 paint = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0,2,4));
        col = mix(col, paint, a);
    }

    FragColor = vec4(col, 1.0);
}