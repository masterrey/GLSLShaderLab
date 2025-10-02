#version 330 core
out vec4 FragColor;

uniform vec2 iResolution;
uniform vec2 iMouse;
uniform int iMouseClick;
uniform sampler2D iChannel0;
uniform float iTime;

const float WAVE_SPEED = 0.5;
const float WAVE_FREQUENCY = 8.0;
const float WAVE_AMPLITUDE = 0.03;


const float BRUSH_SIZE = 0.03;
const float BRUSH_EDGE = 0.01;


float circle(vec2 uv, vec2 center, float size, float edge) {
    float d = length(uv - center);
    return 1.0 - smoothstep(size - edge, size, d);
}


vec3 createWaves(vec2 uv) {

    uv.x *= iResolution.x / iResolution.y;
    
    float time = iTime * WAVE_SPEED;
    
    float waveR = sin(uv.x * WAVE_FREQUENCY + time) * WAVE_AMPLITUDE;
    float waveG = sin(uv.x * WAVE_FREQUENCY + time + 2.0) * WAVE_AMPLITUDE;
    float waveB = sin(uv.x * WAVE_FREQUENCY + time + 4.0) * WAVE_AMPLITUDE;
    
    vec3 color = vec3(
        0.5 + 0.5 * cos(uv.y * 10.0 + waveR + time),
        0.5 + 0.5 * cos(uv.y * 10.0 + waveG + time + 2.0),
        0.5 + 0.5 * cos(uv.y * 10.0 + waveB + time + 4.0)
    );
    
    return color;
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    
    vec3 waveColor = createWaves(uv);

    vec3 paintColor = texture(iChannel0, uv).rgb;
    

    vec3 color = mix(waveColor, paintColor, length(paintColor) * 0.8);

    color *= 0.99;

    if (iMouseClick == 1) {
        vec2 mouseUV = iMouse.xy / iResolution.xy;
        float brush = circle(uv, mouseUV, BRUSH_SIZE, BRUSH_EDGE);
        
        vec3 paint = 0.5 + 0.5 * cos(iTime + uv.xyx * 5.0 + vec3(0, 2, 4));
        
        color += paint * brush * 0.3;
    }
    
    FragColor = vec4(color, 1.0);
}