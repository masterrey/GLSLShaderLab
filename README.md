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


### Passo 2 (próxima entrega sugerida)

- Consolidar UX do Studio com foco em fluxo de edição + preview.
- Revisar atalhos de teclado essenciais e documentar no README.
- Garantir feedback visual claro para ações de arquivo (open/save/save as).
- Validar com `dotnet build GLSLShaderLab.sln` e `dotnet test GLSLShaderLab.sln`.
- implementar o suporte a 3D completo como na versao legacy

### Regra para próximos passos

- Ao finalizar cada etapa, atualize esta seção com:
  - o que foi concluído;
  - qual é o próximo passo objetivo;
  - como validar rapidamente.

## Estrutura do repositório

- `src/` → stack nova (**Studio**)
- `legacy/` → guia e referências do fluxo antigo
- raiz (`GLSLShaderLab.sln`, `Program.cs`, `Shaders/`, `Mesh/`) → runtime **Legacy**
