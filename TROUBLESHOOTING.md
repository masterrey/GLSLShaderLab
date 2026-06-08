# Troubleshooting - GLSLShaderLab

## 1) Aula não inicia

Verifique:
- Shader `.vert` e `.frag` existem em `Shaders/`
- Modelo existe em `Mesh/` (aulas 3D)
- Build sem erros:
  ```bash
  dotnet build
  ```

## 2) Tela preta em aula 2D com persistência

Provável causa: buffers desligados.

Solução:
- Pressione `B` para ativar buffers
- Confirme no título da janela: `Buffers ON`

Observação:
- Aulas curadas de buffer tentam autoativar buffers.

## 3) Compare mode não funciona

Condições para funcionar:
- Aula precisa ter shader de referência
- Buffers devem estar OFF

Se falhar:
- Pressione `B` para desligar buffers
- Pressione `C` novamente

## 4) Textura não aparece

Verifique:
- Pasta `Textures/` existe
- Arquivo é `.png`, `.jpg`, `.jpeg`, `.bmp` ou `.tga`
- Recarregue com `F5`

## 5) Controles 3D não respondem

Causa comum:
- Você está no pipeline 2D (WASD/Z/X não se aplicam)

Solução:
- Reinicie e escolha pipeline **Vertex + Fragment 3D**

## 6) Aula 3D avançada com modelo errado

Solução:
- Em aulas não fixas, troque com `Z`/`X`
- Em aulas introdutórias, o app pode forçar modelo didático (cubo/esfera)

## 7) Erro de compilação de shader

Sintomas:
- Mensagens de `Vertex Shader Log` ou `Fragment Shader Log`

Passos:
1. Revise o arquivo `.vert`/`.frag` da aula
2. Confira uniforms usados (`iTime`, `iResolution`, `iMouse`, `iChannel0`, `model/view/projection`)
3. Recompile:
   ```bash
   dotnet build
   ```

## 8) Como diagnosticar rápido

Use este checklist:
- [ ] Pipeline correto foi escolhido?
- [ ] Aula correta foi selecionada?
- [ ] Buffers ON/OFF está conforme a aula?
- [ ] Compare mode está compatível com o estado atual?
- [ ] Texturas foram recarregadas?
- [ ] Console mostra erros de shader?

## 9) Dica para professores

Para reduzir falhas em sala:
1. Use pipeline 2D + trilha curada
2. Ative `H` e acompanhe objetivo da aula no console
3. Só depois avance para 3D
