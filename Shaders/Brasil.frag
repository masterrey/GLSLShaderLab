#version 330 core
out vec4 fragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;
    

    uv = uv * 2.0 - 1.0; // Movemos a uv para deixar centralizado
    uv.x *= iResolution.x / iResolution.y; // Corrigimos o aspecto da tela
    
    fragColor = vec4(0.251, 0.549,0.255,1.0); // Pintar de verde
    
    
    // Losango amarelo
    float angle = radians(180.0); 
    mat2 rot = mat2(cos(angle), sin(angle),
                    sin(angle), -cos(angle));
    
    vec2 ruv = rot * uv;
    
    float halfW = 0.750; 
    float halfH = 0.550;  

    float k = abs(ruv.x) / halfW + abs(ruv.y) / halfH;

    if (k < 1.75) 
    {
        fragColor = vec4(0.992, 0.875, 0.004, 1.0); 
    }
   
    // Círculo azul
    float dx = uv.x;
    float dy = uv.y;
    float d = dx*dx + dy*dy;

    if(d < 0.35)
    {
        fragColor = vec4(0.047, 0.322, 0.514, 1.0); 
    }
    
    // Faixa branca

    float angleWhite = radians(13.0);
    mat2 rotationMatrix = mat2(cos(angleWhite), -sin(angleWhite),
    sin(angleWhite), cos(angleWhite));
        
    uv *= rotationMatrix;

        
    if (abs(uv.y) < 0.045 && abs(uv.x) < 0.599) 
    {
       fragColor = vec4( 1.0, 1.0, 1.0, 1.0); 
    }
    
}


    
    
