#version 330 core
out vec4 FragColor;
uniform float iTime;
uniform vec2 iResolution;

void main()
{
    vec2 uv = gl_FragCoord.xy / iResolution.xy;

    // Criação da onda senoidal (bandeira se movendo)
    float wave = 0.5 + sin(iTime + uv.x * 10.0) * 0.05;

    // Cores principais
    vec4 white = vec4(1.0, 1.0, 1.0, 1.0);
    vec4 red   = vec4(0.945, 0.094, 0.149, 1.0);

    // Se estiver abaixo da onda (parte “abaixo do vento”)
    if (uv.y < wave)
    {
        // Sobrepondo faixas conforme sua lógica
        if (uv.y > 0.45 && uv.y < 0.6 && uv.x > 0.4 && uv.x < 0.6)
        {
            FragColor = white;
            return;
        }
        else if (uv.x > 0.45 && uv.x < 0.55 && uv.y > 0.35 && uv.y < 0.7)
        {
            FragColor = white;
            return;
        }
        else
        {
            FragColor = red;
            return;
        }
    }
    else // Acima da onda: fundo animado – aqui, simplesmente vermelho
    {
        if (uv.y > 0.45 && uv.y < 0.6 && uv.x > 0.4 && uv.x < 0.6)
        {
            FragColor = white;
            return;
        }
        else if (uv.x > 0.45 && uv.x < 0.55 && uv.y > 0.35 && uv.y < 0.7)
        {
            FragColor = white;
            return;
        }
        else
        {
            FragColor = red;
            return;
        }
    }
}


    
    
