#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    // convert from pixel coordinates to normalized coordinates
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    // calculate the aspect ratio
    float prop=iResolution.x/ iResolution.y;
    
    // adjust the UV coordinates based on the aspect ratio
    float x = length(vec2(uv.x*prop,uv.y)-vec2(prop/2.0,0.5));

    // white background
     FragColor = vec4(1,1,1, 1.0);

     // red sphere in center and 0.25 radius of the sphere
    if(x< 0.25){
        FragColor = vec4(1,0,0, 1.0);
    }
}