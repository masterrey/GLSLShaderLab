#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = fragCoord/iResolution.xy;
    // Output to screen
    fragColor = vec4(0.004, 0.137, 0.267, 1.0);
    if(uv.x > 0.333){
    fragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
    if(uv.x > 0.666){
    fragColor = vec4(0.769, 0.071, 0.188, 1.0);
    }

}
