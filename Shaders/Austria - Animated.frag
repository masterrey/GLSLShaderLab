#version 330 core
out vec4 fragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    
    float wave = cos(iTime + uv.x * 4.0) * 0.8;; // Normalizam, pegamos o cos que variava na tela toda e deixamos apenas na tela
    
    float fs = 2.0 + wave * 1.8;
   
    if(uv.y > (0.1 * wave + 0.2))
    {
        fragColor = vec4(0.812, 0.035, 0.129, 1.0) * fs;
    } 

    if(uv.y > (0.1 * wave + 0.4))
    {
        fragColor  = vec4(1.0, 1.0, 1.0, 1.0) * fs;
    } 

    if(uv.y > (0.1 * wave + 0.7))
    {
        fragColor = vec4(0.812, 0.035, 0.129, 1.0) * fs;
    } 

    if(uv.y < (0.1 * wave + 0.2))
    {
        fragColor = vec4(0);
    }

    if(uv.y > (0.1 * wave + 0.9))
    {
        fragColor = vec4(0);
    }

    if(uv.x < 0.1)
    {
        fragColor = vec4(0);
    }

    if(uv.x > 0.9)
    {
        fragColor = vec4(0);
    }
    
}


    
    
