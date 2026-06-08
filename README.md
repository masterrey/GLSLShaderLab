# GLSLShaderLab

Laboratório interativo para aprender GLSL em duas trilhas:

1. **Fragment Shader 2D (iniciante)**
2. **Vertex + Fragment 3D (intermediário/avançado)**

A aplicação agora inicia com foco didático: primeiro você escolhe a trilha, depois a aula.

## Como usar

1. Execute:
   ```bash
   dotnet run
   ```
2. Escolha o **pipeline de aprendizado**.
3. Escolha o **modo de aulas**:
   - Trilha curada (recomendado)
   - Todos os shaders (inclui experimentais)
4. Escolha uma aula e confirme.
5. No pipeline 3D, em aulas avançadas, escolha modelo quando solicitado.

## Trilhas de aprendizado

## 1) Fragment Shader 2D (iniciante)
Ordem sugerida:
1. Color & Time Basics
2. Shape Composition
3. Pattern Composition
4. Animated Ripples
5. Mouse Interaction Paint
6. Persistent Paint with Buffer
7. Feedback Functions
8. Advanced Buffer Demo
9. Procedural Clouds

Objetivos:
- Entender `gl_FragCoord`, `iResolution`, `iTime`
- Construir formas e padrões 2D
- Trabalhar interação com mouse (`iMouse`, `iMouseClick`)
- Aprender persistência com buffers (`iChannel0`)

## 2) Vertex + Fragment 3D
Ordem sugerida:
1. 3D Lighting Intro
2. Specular Lighting
3. Vertex Deformation
4. Animated Flag
5. Normals Practice

Objetivos:
- Entender estágio de vértice e fragmento
- Usar normais, câmera e iluminação
- Fazer deformação de vértices
- Explorar efeitos avançados em malha 3D

## Controles

Controles gerais:
- `Q / E` Aula anterior / próxima
- `N` Próxima aula recomendada
- `R` Resetar estado/câmera
- `B` Toggle buffers
- `C` Compare mode (quando disponível e buffers OFF)
- `H` Mostrar/ocultar ajuda
- `F5` Recarregar texturas
- `ESC` Sair

Controles extras no pipeline 3D:
- `W A S D` Mover câmera
- `Z / X` Trocar modelo (aulas não fixas)

## Recursos visuais de aprendizado

- Título da janela com badges de estado:
  - Pipeline ativo
  - Aula atual
  - Buffers ON/OFF
  - Quantidade de texturas
  - Compare ON/OFF
- Resumo da aula no console com:
  - Categoria
  - Dificuldade
  - Objetivo
  - Recursos necessários

## Buffers e comparação

- Aulas com persistência podem ativar buffers automaticamente.
- `Compare mode` mostra referência e resultado lado a lado quando a aula possuir shader de referência.
- Compare mode funciona com **buffers desligados**.

## Estrutura principal

- `/tmp/workspace/masterrey/GLSLShaderLab/Program.cs` - fluxo de startup (pipeline -> modo -> aula)
- `/tmp/workspace/masterrey/GLSLShaderLab/ShaderSelector.cs` - metadados de aulas, trilhas e seleção
- `/tmp/workspace/masterrey/GLSLShaderLab/Window.cs` - renderização, controles, compare mode e estado visual
- `/tmp/workspace/masterrey/GLSLShaderLab/ModelSelector.cs` - seleção de modelos e modelo padrão
- `/tmp/workspace/masterrey/GLSLShaderLab/Shaders/` - shaders GLSL
- `/tmp/workspace/masterrey/GLSLShaderLab/Mesh/` - modelos 3D
- `/tmp/workspace/masterrey/GLSLShaderLab/Textures/` - texturas auxiliares

## Requisitos

- .NET 9 SDK
- OpenTK 4.7.6
- AssimpNet 5.0.0-beta1
- SixLabors.ImageSharp 3.1.11

## Dica didática

Para turmas iniciantes, use sempre:
1. Pipeline 2D
2. Trilha curada
3. Aulas em ordem

Depois migre para a trilha 3D.
