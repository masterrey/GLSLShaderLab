#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
 
void main() {
    vec3 green = vec3(0.0, 0.5, 0.0);
    vec3 yellow = vec3(0.95, 0.95, 0.0);
    vec3 blue = vec3(0.0, 0.0, 0.5);
    vec3 white = vec3(1.0, 1.0, 1.0);
    vec3 color = green;
 
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec2 center = vec2(0.5, 0.5);
 
    vec2 adjusted_uv_losango = uv;
    adjusted_uv_losango.x *= iResolution.x / iResolution.y;
 
    float dist_losango = abs(adjusted_uv_losango.x - center.x * (iResolution.x / iResolution.y)) / 0.5 + abs(adjusted_uv_losango.y - center.y) / 0.35;
    if (dist_losango < 1.0) {
        color = yellow;
    }
 
    vec2 adjusted_uv_circle = uv;
    adjusted_uv_circle.x *= iResolution.x / iResolution.y;
    float dist_circulo = distance(adjusted_uv_circle, vec2(0.5 * (iResolution.x / iResolution.y), 0.5));
    if (dist_circulo < 0.22) {
        color = blue;
    }
 
    vec2 faixa_uv = adjusted_uv_circle - vec2(0.5 * (iResolution.x / iResolution.y), 0.4);
    
    float raio_externo_faixa = 0.245;
    float raio_interno_faixa = 0.21;
    float offset_y_faixa = 0.04;
 
    float dist_externo_faixa = distance(faixa_uv, vec2(0.0, -offset_y_faixa));
    float dist_interno_faixa = distance(faixa_uv, vec2(0.0, -offset_y_faixa));
    
    if (dist_circulo < 0.22 && dist_externo_faixa < raio_externo_faixa && dist_interno_faixa > raio_interno_faixa) {
        color = white;
    }
 
    FragColor = vec4(color, 1.0);
}
 
 