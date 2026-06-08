# Legacy (antigo)

Esta pasta centraliza a orientação do fluxo antigo para não misturar com o caminho novo do Studio.

## Ponto de partida do Legacy

- Solução: `../GLSLShaderLab.sln`
- Projeto executável: `../GLSLShaderLab.csproj`
- Entrada principal: `../Program.cs`

```bash
dotnet build GLSLShaderLab.sln
dotnet run --project GLSLShaderLab.csproj
```

## Pastas principais do Legacy

- `../Shaders/` → shaders `.vert` e `.frag`
- `../Mesh/` → modelos 3D
- `../Textures/` → texturas de suporte

## Observação

Para novos desenvolvimentos, use o fluxo **Studio (novo)** em `../src/`.
