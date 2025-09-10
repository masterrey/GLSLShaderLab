#version 330 core
out vec4 fragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    
    if(uv.y > 0.45 && uv.y < 0.65)
    {
        fragColor = vec4(1.0,1.0,1.0,1.0);
    }
    else if(uv.x > 0.25 && uv.x < 0.4)
    {
        fragColor = vec4(1.0,1.0,1.0,1.0);
    }
    else 
    {
        fragColor = vec4(0.871,0.094,0.024,1.0);
    }
    
}


    
    
