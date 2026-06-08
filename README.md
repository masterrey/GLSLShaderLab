# GLSLShaderLab

Laboratório para experimentar shaders GLSL com dois fluxos de trabalho: **Studio (novo)** e **Legacy (antigo)**.

## Comece aqui

Abra `GLSLShaderLab.sln` — a solução principal. Ela contém todos os projetos organizados em pastas:

- 📁 **Studio (novo)** — `App.Wpf`, `Engine`, `Core`
- 📁 **Legacy (antigo)** — `GLSLShaderLab` (console)

### 1) Studio (novo) — ponto de partida oficial

Use este fluxo para desenvolvimento atual.

```bash
dotnet run --project src/GLSLShaderLab.App.Wpf/GLSLShaderLab.App.Wpf.csproj
```

No VS Code, pressione **F5** e a configuração **"Studio (WPF) — novo"** será selecionada automaticamente.

### 2) Legacy (antigo) — fluxo mantido por compatibilidade

Use apenas quando precisar do app antigo baseado em console.

```bash
dotnet run --project GLSLShaderLab.csproj
```

Documentação do fluxo antigo: [`legacy/README.md`](legacy/README.md)

## Mapa rápido de arquitetura (Studio)

- **UI**: `src/GLSLShaderLab.App.Wpf`  
  Janela WPF, editor, preview e interação.
- **Engine**: `src/GLSLShaderLab.Engine`  
  Renderização OpenGL, compilação/reload de shader e canais `iChannel`.
- **Core**: `src/GLSLShaderLab.Core`  
  Modelos de documento, templates e persistência de sessão.

## Estrutura do repositório

- `src/` → stack nova (**Studio**)
- `legacy/` → guia e referências do fluxo antigo
- raiz (`GLSLShaderLab.sln`, `Program.cs`, `Shaders/`, `Mesh/`) → runtime **Legacy**
