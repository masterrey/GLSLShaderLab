# Tutorial de Shader por Trilhas

Este guia acompanha o novo fluxo do projeto: primeiro 2D fragment, depois 3D vertex+fragment.

## Trilha 1: Fragment Shader 2D

## Aula 1 — Color & Time Basics
**Objetivo:** entender `gl_FragCoord`, `iResolution`, `iTime`.

Checklist:
- [ ] Converter coordenadas para UV (0..1)
- [ ] Gerar gradiente simples
- [ ] Animar cor com `sin(iTime)`

## Aula 2 — Shape Composition
**Objetivo:** desenhar formas com funções de distância.

Checklist:
- [ ] Criar círculo por `distance`
- [ ] Suavizar borda com `smoothstep`
- [ ] Misturar forma com cor de fundo

## Aula 3 — Mouse Interaction Paint
**Objetivo:** usar `iMouse` e `iMouseClick`.

Checklist:
- [ ] Mostrar indicador do mouse
- [ ] Pintar somente quando click ativo
- [ ] Alterar tamanho/cor do pincel

## Aula 4 — Persistent Paint with Buffer
**Objetivo:** entender persistência entre frames com `iChannel0`.

Checklist:
- [ ] Ler frame anterior em `iChannel0`
- [ ] Misturar conteúdo atual com anterior
- [ ] Testar diferença entre Buffers ON e OFF

## Trilha 2: Vertex + Fragment 3D

## Aula 1 — 3D Lighting Intro
**Objetivo:** conectar atributos de malha e iluminação básica.

Checklist:
- [ ] Usar `aPos`, `aNormal`, `aTexCoord`
- [ ] Aplicar `model`, `view`, `projection`
- [ ] Calcular iluminação ambiente + difusa

## Aula 2 — Specular Lighting
**Objetivo:** adicionar brilho especular.

Checklist:
- [ ] Calcular vetor da câmera (`viewPos`)
- [ ] Usar `reflect` e potência specular
- [ ] Ajustar material/baseColor

## Aula 3 — Vertex Deformation
**Objetivo:** deformar geometria no vertex shader.

Checklist:
- [ ] Modificar posição de vértice com `sin`
- [ ] Manter renderização estável no fragment
- [ ] Explorar amplitude/frequência

## Recursos e sinais visuais

Durante a execução, confira no título da janela:
- Pipeline ativo
- Aula atual
- Buffers ON/OFF
- Quantidade de texturas
- Compare ON/OFF

Use `H` para abrir resumo didático no console.

## Compare mode

- Disponível apenas em aulas com referência.
- Funciona com buffers desligados.
- Use para comparar resultado atual com shader de referência da aula.

## Sugestão de progressão para turmas

1. Completar trilha 2D (ordem curada)
2. Repetir 2D em modo “Todos os shaders”
3. Migrar para trilha 3D
4. Introduzir variações próprias em cada aula
