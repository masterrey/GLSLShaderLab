#version 330 core

out vec4 FragColor;

uniform float iTime;
uniform vec2 iResolution;
uniform vec2 iMouse;
uniform float iMouseClick;
uniform float iClickCount;

// Shape distance functions
float circleDistance(vec2 uv, vec2 center) {
    return distance(uv, center);
}

float squareDistance(vec2 uv, vec2 center) {
    vec2 d = abs(uv - center) - vec2(0.1);
    return max(d.x, d.y);
}

float triangleDistance(vec2 uv, vec2 center) {
    vec2 p = uv - center;
    p /= 0.1;
    
    const float k = sqrt(3.0);
    p.x = abs(p.x) - 1.0;
    p.y = p.y + 1.0 / k;
    
    if (p.x + k * p.y > 0.0) {
        p = vec2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    }
    
    p.x -= clamp(p.x, -2.0, 0.0);
    return -length(p) * sign(p.y);
}

float starDistance(vec2 uv, vec2 center) {
    vec2 p = uv - center;
    p /= 0.1;
    
    float angle = atan(p.y, p.x);
    float radius = length(p);
    float spikes = 5.0;
    float r = cos(spikes * angle) * 0.5 + 0.5;
    
    return radius - r;
}

float cubeDistance(vec2 uv, vec2 center) {
    vec2 p = uv - center;
    return max(abs(p.x), abs(p.y)) - 0.1;
}

// Get shape color based on type
vec3 getShapeColor(int shapeIndex) {
    if (shapeIndex == 0) return vec3(0.0, 0.0, 1.0);      // Blue circle
    if (shapeIndex == 1) return vec3(1.0, 0.0, 0.0);      // Red triangle
    if (shapeIndex == 2) return vec3(0.0, 1.0, 0.0);      // Green square
    if (shapeIndex == 3) return vec3(1.0, 1.0, 0.0);      // Yellow star
    if (shapeIndex == 4) return vec3(1.0, 0.0, 1.0);      // Magenta cube
    
    return vec3(1.0);
}

void main() {
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    vec2 mousePos = iMouse / iResolution;
    
    // Create animated background color
    vec3 backgroundColor = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0, 2, 4));
    
    int currentShape = int(mod(iClickCount, 5.0));
    float edgeThreshold = 0.01;
    float outlineSize = 0.005;
    
    float shapeDist = 1.0;
    vec3 shapeFillColor = vec3(0.0);
    bool shouldDrawShape = (iMouseClick > 0.5);
    
    if (shouldDrawShape) {
        // Calculate distance to current shape
        if (currentShape == 0) shapeDist = circleDistance(uv, mousePos);
        else if (currentShape == 1) shapeDist = triangleDistance(uv, mousePos);
        else if (currentShape == 2) shapeDist = squareDistance(uv, mousePos);
        else if (currentShape == 3) shapeDist = starDistance(uv, mousePos);
        else if (currentShape == 4) shapeDist = cubeDistance(uv, mousePos);
        
        shapeFillColor = getShapeColor(currentShape);
    }
    
    // Calculate alpha and outline
    float alpha = smoothstep(edgeThreshold, edgeThreshold - 0.005, shapeDist);
    float outline = smoothstep(edgeThreshold + outlineSize, edgeThreshold, shapeDist) - 
                   smoothstep(edgeThreshold, edgeThreshold - 0.005, shapeDist);
    
    vec3 outlineColor = vec3(0.0);
    vec3 finalColor = backgroundColor;
    
    // Apply outline and shape fill
    finalColor = mix(finalColor, outlineColor, outline);
    finalColor = mix(finalColor, shapeFillColor, alpha);
    
    FragColor = vec4(finalColor, 1.0);
}