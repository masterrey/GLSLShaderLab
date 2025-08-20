#version 330 core
out vec4 FragColor;
// uniform is a external variable that is set by the application
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    float wave = (cos((iTime + uv.x) * 2) * 0.2) + 0.2;
    
    float fs = 0.9 + wave * 1.2;


    if (uv.y > (0.1 * wave + 0.2)) {
    // Top half: white
    FragColor = vec4(0.0, 0.60, 0.89, 1.0)*fs;
    }

    if (uv.y > (0.1 * wave + 0.4)) {
    // Top half: blue
    FragColor = vec4(1.0,1.0,1.0, 1.0)*fs;
    }
    
    if (uv.y > (0.1 * wave + 0.7)) {
    // Top half: blue
    FragColor = vec4(1.0, 0.0, 0.27, 1.0)*fs;
    }
    //corte vertical
    if(uv.y < (0.1 * wave + 0.2)){
        FragColor = vec4(0);
    }
 
    //vertical cut 
    if(uv.y > (0.1 * wave + 0.9)){
     FragColor = vec4(0);
    }
    if(uv.x < 0.1){
     FragColor = vec4(0);
    }
    if(uv.x > 0.9){
     FragColor = vec4(0);
    }

}
