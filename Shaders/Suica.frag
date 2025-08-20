#version 330 core
out vec4 fragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    // Time varying pixel color
    vec3 col = 0.5 + 0.5*cos(iTime+uv.xyx+vec3(0,2,4));

    vec4 white = vec4(1.0,1.0,1.0,1.0);
    vec4 red = vec4(0.945,0.094,0.149, 1.0);


    if(uv.y > 0.45 && uv.y < 0.6 && uv.x > 0.4 && uv.x < 0.6) {
        fragColor = white;
        return;
    } else {
        fragColor = red;
    }


    if(uv.x > 0.45 && uv.x < 0.55 && uv.y > 0.35 && uv.y < 0.7) {
        fragColor = white;
        return;
    } else {
        fragColor = red;
    }
    
}


    
    
