#version 330 core
out vec4 FragColor;
// uniform is a external variable that is set by the application
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    float wave = (cos((iTime + uv.x) * 2) * 0.2) + 0.2;
    
    float prop=iResolution.x/iResolution.y;
    float x = length(vec2(uv.x*prop,uv.y)-vec2(prop/2.0,0.5));

    FragColor = vec4(1,1,1, 1.0);


     //cor do fundo background
    if(uv.y < wave){
    FragColor = vec4(1,1,1, 1.0);
    }
    
    // circulo vermelho no meio
    if(x < wave) { 
    FragColor = vec4(1,0,0,1.0);
    }

}
