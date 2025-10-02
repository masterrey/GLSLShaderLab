#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    
    if(uv.x < 0.3)
    {
        FragColor = vec4(0.027,0.412,0.157,1.0);
    }
    else if(uv.x > 0.7)
    {
        FragColor = vec4(0.871,0.094,0.024,1.0);
    }
    else
    {
        FragColor = vec4(1.0,1.0,1.0,1.0);
    }
   
}


    
    
