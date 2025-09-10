#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;



// Circle
vec4 DrawCircle(vec2 uv, vec3 color, vec2 pos, float radius, float edge) {
    vec2 posn = pos / iResolution.xy;
    float dist = distance(uv, posn);
    float alpha = smoothstep(radius, radius - edge, dist);
    return vec4(color, alpha);
}


// Square
vec4 DrawSquare(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution.xy;
    vec2 d = abs(uv - posn);
    float alpha = smoothstep(size, size - edge, max(d.x, d.y));
    return vec4(color, alpha);
}


// Triangle
vec4 DrawTriangle(vec2 uv, vec3 color, vec2 pos, float size, float edge) {
    vec2 posn = pos / iResolution.xy;
    vec2 p = uv - posn;

    float k = sqrt(3.0);
    p.x = abs(p.x) - size;
    p.y = p.y + size / k;
    if(p.x + k * p.y > 0.0) p = vec2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * size, 0.0);

    float dist = length(p) * sign(p.y);
    float alpha = smoothstep(0.0, edge, dist);
    return vec4(color, alpha);
}


void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec2 mouseNorm = iMouse.xy ;
    vec3 baseColor = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0,2,4));

    FragColor = vec4(baseColor, 1.0);

     if(iMouseClick > 0.5)
    {
        // Switch forms for 2 and 2 times
        int shapeIndex = int(mod(floor(iTime / 2.0), 3.0));

        vec4 shape;

        if (shapeIndex == 0) 
        {
            shape = DrawCircle(uv, vec3(0.0, 0.0, 1.0), mouseNorm, 0.1, 0.01);
        }
        else if (shapeIndex == 1) 
        {
            shape = DrawSquare(uv, vec3(0.0, 0.0, 1.0), mouseNorm, 0.1, 0.01);
        }
        else 
        {
            shape = DrawTriangle(uv, vec3(0.0, 0.0, 1.0), mouseNorm, 0.1, 0.01);
        }

        FragColor = mix(FragColor, shape, shape.a);

    }

    

}


    
    
