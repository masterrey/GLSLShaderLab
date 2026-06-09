# GLSLShaderLab — Laboratório Interativo de Programação de Shaders GLSL

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
![Build Release](https://github.com/masterrey/GLSLShaderLab/actions/workflows/release.yml/badge.svg)
[![Latest Release](https://img.shields.io/github/v/release/masterrey/GLSLShaderLab?display_name=tag)](https://github.com/masterrey/GLSLShaderLab/releases)
[![Downloads](https://img.shields.io/github/downloads/masterrey/GLSLShaderLab/total)](https://github.com/masterrey/GLSLShaderLab/releases)
![Platform: .NET 9.0](https://img.shields.io/badge/Platform-.NET%209.0-512BD4)
![Graphics: OpenGL](https://img.shields.io/badge/Graphics-OpenGL%204.5+-412991)

**Ferramenta educacional desenvolvida para a disciplina de Jogos Digitais**  
**Pontifícia Universidade Católica de São Paulo (PUC-SP)**

---

## 📚 Sobre este Projeto

GLSLShaderLab é um ambiente interativo de aprendizagem projetado para ensinar os fundamentos da programação de shaders em **OpenGL Shading Language (GLSL)**. Esta ferramenta oferece uma interface gráfica intuitiva (Studio) e um ambiente de console legado, permitindo que estudantes experienciem, criem e visualizem shaders em tempo real.

### Objetivos Educacionais

- **Compreender o pipeline gráfico**: Conceitos fundamentais de renderização e processamento de vértices/fragmentos
- **Dominar GLSL**: Sintaxe, tipos de dados, funções built-in e técnicas de programação de shaders
- **Explorar técnicas avançadas**: Iluminação, efeitos visuais, animação e processamento de texturas
- **Aplicar na prática**: Desenvolvimento de shaders reais para aplicações gráficas interativas
- **Integração com Jogos**: Compreender o papel crítico dos shaders no desenvolvimento de jogos digitais

### Perfil de Público

- Estudantes de Jogos Digitais (graduação)
- Desenvolvedores gráficos iniciantes
- Pesquisadores interessados em técnicas de renderização
- Educadores em computação gráfica

---

## 🚀 Início Rápido

### Para Usuários (Professores, Alunos, Educadores)

GLSLShaderLab possui **releases automáticos** — não é necessário instalar .NET ou fazer compilação!

#### 1. Baixar a Aplicação

1. Acesse a página de [**Releases**](https://github.com/masterrey/GLSLShaderLab/releases)
2. Baixe o arquivo **`GLSLShaderLab-win-x64.zip`** (versão mais recente)
3. **Descompacte** em qualquer pasta

#### 2. Requisitos (Mínimos)

- **Windows 10/11** (64-bit)
- **OpenGL 4.5+** (placa gráfica compatível)
  - A maioria das placas modernas (NVIDIA, AMD, Intel) suporta
  - Execute a primeira vez; se houver erro de OpenGL, verifique drivers

#### 3. Executar

Abra a pasta descompactada e **clique duas vezes** em:
```
GLSLShaderLab.App.Wpf.exe
```

Pronto! A aplicação está pronta para usar. Comece com os exemplos inclusos em `Shaders/`.

### Interface do App (Studio)

Ao abrir o GLSLShaderLab Studio, você verá quatro áreas principais:

1. **Barra de ferramentas (topo)**
- `Open`, `Save`, `Save As`: abrir e salvar shaders
- `Compile` e `Auto`: compilar manualmente ou automaticamente durante edição
- `Pause`, `Reset Time`, `Fullscreen`: controlar animação e visualização
- `Mode` (2D / 3D), `Model`, `Reset Camera`: fluxo para renderização 2D e 3D
- `Template`: carregar templates de shader para estudo

2. **Editor de Shader (lado esquerdo)**
- Aba **Fragment Shader**: código principal do efeito visual
- Aba **Vertex Shader (3D)**: código de vértice para cenários 3D
- Edição em tempo real com compilação rápida

3. **Preview (lado direito)**
- Área de renderização OpenGL com resultado visual imediato
- Botões `Load iChannel0` a `Load iChannel3` para carregar texturas/canais auxiliares

4. **Diagnostics (parte inferior)**
- Exibe mensagens de compilação, erros e avisos
- Use esta área para depurar rapidamente seu shader

5. **Status Bar (rodapé)**
- Estado atual da aplicação (`Ready`, compilando, etc.)
- FPS em tempo real

---

### Para Desenvolvedores (Contribuidores Open Source)

Se você quer **modificar o código-fonte**, contribuir com bugfixes ou novas funcionalidades:

> Esta seção é destinada somente a contribuidores open source. Para uso em aula/laboratório, utilize apenas o fluxo de release acima.

#### Requisitos

- **.NET 9.0** ou superior
- **OpenGL 4.5+** (placa gráfica compatível)
- **Visual Studio Code** ou **Visual Studio 2022/2024** (recomendado)
- **Git**

#### Setup do Projeto

```bash
# Clonar repositório
git clone https://github.com/masterrey/GLSLShaderLab.git
cd GLSLShaderLab

# Restaurar dependências
dotnet restore

# Compilar (debug)
dotnet build GLSLShaderLab.sln

# Executar em desenvolvimento
dotnet run --project src/GLSLShaderLab.App.Wpf/GLSLShaderLab.App.Wpf.csproj
```

**No VS Code/Visual Studio:**  
Pressione **F5** — a configuração de inicialização padrão (Studio WPF) será ativada automaticamente.

#### Legacy (Console) — Referência Histórica

Para estudar a implementação anterior ou executar exemplos legados:

```bash
dotnet run --project GLSLShaderLab.csproj
```

Documentação completa: [legacy/README.md](legacy/README.md)

## 🏗️ Arquitetura do Projeto

O projeto está organizado em **dois fluxos de trabalho**:

### Studio (Novo) — Stack Moderno

```
GLSLShaderLab.sln
├── src/
│   ├── GLSLShaderLab.App.Wpf/       [UI - Interface WPF]
│   │   ├── MainWindow.xaml          - Janela principal com editor e preview
│   │   └── App.xaml.cs              - Lógica de inicialização
│   ├── GLSLShaderLab.Engine/        [Renderização - OpenGL]
│   │   ├── Rendering/               - Geometria e renderização
│   │   └── Services/                - Compilação e gerenciamento de shaders
│   └── GLSLShaderLab.Core/          [Modelos de Dados]
│       ├── Models/                  - DocumentModel, ShaderTemplate, etc.
│       └── Services/                - Persistência e catálogo de templates
```

**Componentes Principais:**

| Componente | Responsabilidade | Linguagem |
|---|---|---|
| **App.Wpf** | Interface gráfica, editor de texto, viewport OpenGL | C# + XAML |
| **Engine** | Renderização OpenGL, compilação GLSL, gerenciamento de texturas/buffers | C# + OpenGL |
| **Core** | Modelos de dados, templates de shaders, persistência de sessão | C# |

### Legacy (Antigo) — Console Educacional

A estrutura raiz contém a implementação original baseada em console:

```
GLSLShaderLab/
├── Program.cs
├── Shader.cs, BufferManager.cs, etc.
├── Shaders/                         - Exemplos e referências
└── Mesh/                            - Geometrias 3D pré-compiladas
```

---

## 📖 Como Usar

### Para Aprender Shaders

1. **Abra o Studio** (F5 no VS Code)
2. **Selecione um shader de exemplo** do menu ou catálogo
3. **Modifique o código** no editor GLSL
4. **Visualize em tempo real** na viewport OpenGL
5. **Experimente**: parâmetros, texturas, geometrias
6. **Salve seu trabalho** como novo template

### Exemplos Incluídos

A pasta `Shaders/` contém diversos exemplos funcionais:

- **Shaders 2D**: `SimplePaint.glsl`, `Ripple.glsl`, `Clouds.glsl`
- **Shaders 3D**: `basic3d.glsl`, `Simple3d.glsl`, `Normal.glsl`
- **Efeitos**: `AnimatedFlag.glsl`, `BufferDemo.glsl`, `Functions.glsl`

Consulte [SHADER_TUTORIAL.md](SHADER_TUTORIAL.md) para guias passo a passo.

---

## 🔧 Desenvolvimento e Contribuição

### Compilação

```bash
# Debug
dotnet build GLSLShaderLab.sln

# Release
dotnet publish GLSLShaderLab.sln --configuration Release
```

### Estrutura de Contribuição

Para adicionar novos recursos ou corrigir bugs:

1. **Crie uma branch**: `git checkout -b feature/sua-feature`
2. **Implemente e teste**: `dotnet build` e execute os testes
3. **Atualize a documentação**: Adicione exemplos e instruções
4. **Envie um Pull Request**: Descreva as mudanças claramente

### Problemas Conhecidos

Consulte [TROUBLESHOOTING.md](TROUBLESHOOTING.md) para soluções comuns.

---

## 📋 Roadmap Acadêmico

### Passo 2 (Próxima Entrega)

- [ ] Consolidar UX do Studio com foco em fluxo de edição + preview em tempo real
- [ ] Revisar e documentar atalhos de teclado essenciais
- [ ] Implementar feedback visual claro para operações de arquivo (open/save/save as)
- [ ] Validação com testes: `dotnet test GLSLShaderLab.sln`
- [ ] Expandir suporte 3D completo (compatível com versão legacy)

### Critério de Validação

Ao finalizar cada etapa:
- ✓ O que foi concluído
- ✓ Qual é o próximo objetivo
- ✓ Como validar rapidamente

---

## 📚 Recursos Educacionais

- **[SHADER_TUTORIAL.md](SHADER_TUTORIAL.md)** — Tutoriais interativos de GLSL (iniciante a avançado)
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** — Solução de problemas comuns
- **[legacy/README.md](legacy/README.md)** — Referência da versão anterior

### Referências Externas

- [Khronos OpenGL Documentation](https://www.khronos.org/opengl/)
- [GLSL Language Specification](https://www.khronos.org/files/webgl20-reference-guide.pdf)
- [The Book of Shaders](https://thebookofshaders.com/) — Tutorial interativo online
- [Shadertoy](https://www.shadertoy.com/) — Comunidade de shaders e inspiração

---

## 👨‍🏫 Sobre o Desenvolvedor

**Prof. Dr. Reinaldo Ramos**  
Professor — Disciplina de Jogos Digitais  
Escola de Criatividade, Planejamento e Gestão  
Pontifícia Universidade Católica de São Paulo (PUC-SP)

Este projeto foi desenvolvido como ferramenta pedagógica para facilitar o aprendizado prático de programação de shaders em um contexto acadêmico estruturado.

**Perfil profissional:** [PhD Reinaldo Ramos (LinkedIn)](https://www.linkedin.com/safety/go/?url=https%3A%2F%2Ft4interactive.com%2Fphd-reinaldo-ramos&urlhash=Zawb&mt=5WA44W67XjgEpcsxNDligLuYyTV4sbn0mScllpz_gyU5z_leZqUHAnnIoaNhfZcgnJdeRp1KTrj-WIPc4o3_QNjxQg&isSdui=true&lipi=urn%3Ali%3Apage%3Ad_flagship3_profile_view_base%3B9wh%2F6HaaQxCqVHd6iHUWBg%3D%3D)

---

## 📄 Licença

Este projeto é licenciado sob a [Licença MIT](LICENSE.md).

**Atribuição Sugerida:**  
```bibtex
@software{glslshaderlab2024,
  author = {Ramos, Reinaldo},
  title = {GLSLShaderLab: Interactive Laboratory for GLSL Shader Programming},
  year = {2024},
  howpublished = {\url{https://github.com/masterrey/GLSLShaderLab}},
  institution = {PUC-SP}
}
```

---

## 🤝 Suporte

- **Issues**: [GitHub Issues](https://github.com/masterrey/GLSLShaderLab/issues)
- **Discussões**: [GitHub Discussions](https://github.com/masterrey/GLSLShaderLab/discussions)
- **Email**: [contato via instituição]

---

**Última Atualização**: Junho de 2026  
**Versão**: Consulte a badge de Latest Release no topo  
**Status**: Desenvolvimento Acadêmico Ativo
