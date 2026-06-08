using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace GLSLShaderLab
{
    public class Window : GameWindow
    {
        private Shader? _shader;
        private Shader? _referenceShader;
        private Model? _model;
        private Shader? _copyShader;
        private BufferManager? _bufferManager;

        private float _time;
        private bool _showHelp;
        private bool _useBuffers;
        private bool _compareMode;

        private ShaderSelector.ShaderLesson _activeLesson;
        private readonly ShaderSelector.LearningPipeline _activePipeline;
        private readonly List<ShaderSelector.ShaderLesson> _pipelineLessons;
        private int _currentLessonIndex;

        // Camera properties
        private Vector3 _cameraPos = new Vector3(0.0f, 0.0f, 3.0f);
        private Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private Vector3 _cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
        private float _fov = 45.0f;

        // Model transformation
        private float _rotationY;

        // Fullscreen quad
        private int _vao;
        private int _vbo;
        private int _ebo;

        // Render mode
        private enum RenderMode { Fullscreen2D, Model3D }
        private RenderMode _renderMode;

        // Textures
        private readonly List<int> _loadedTextures = new();
        private readonly List<string> _loadedTextureNames = new();
        private string _resolvedTextureDir = "";

        public Window(
            int width,
            int height,
            string title,
            ShaderSelector.ShaderLesson selectedLesson,
            ModelSelector.ModelInfo selectedModel,
            List<ShaderSelector.ShaderLesson> pipelineLessons,
            ShaderSelector.LearningPipeline pipeline)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Size = new Vector2i(width, height),
                Title = title
            })
        {
            _activeLesson = selectedLesson;
            _activePipeline = pipeline;
            _pipelineLessons = pipelineLessons;
            _currentLessonIndex = Math.Max(0, _pipelineLessons.FindIndex(l => l.Id == selectedLesson.Id));
            _renderMode = pipeline == ShaderSelector.LearningPipeline.Fragment2D
                ? RenderMode.Fullscreen2D
                : RenderMode.Model3D;

            _model = new Model(selectedModel.FilePath);
            _useBuffers = _activeLesson.AutoEnableBuffers;

            CursorState = CursorState.Normal;
            UpdateWindowTitle();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            float[] quadVertices = {
                -1.0f, -1.0f, 0.0f, 0.0f,
                 1.0f, -1.0f, 1.0f, 0.0f,
                 1.0f,  1.0f, 1.0f, 1.0f,
                -1.0f,  1.0f, 0.0f, 1.0f
            };

            uint[] quadIndices = { 0, 1, 2, 2, 3, 0 };

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, quadIndices.Length * sizeof(uint), quadIndices, BufferUsageHint.StaticDraw);

            int stride = 4 * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

            _bufferManager = new BufferManager(Size.X, Size.Y);

            try { _copyShader = new Shader("Shaders/copy.vert", "Shaders/copy.frag"); }
            catch (Exception ex) { Console.WriteLine($"Erro ao carregar copy shader: {ex.Message}"); }

            LoadLesson(_activeLesson);

            _resolvedTextureDir = ResolveTexturesFolder();
            LoadTexturesFromFolder(_resolvedTextureDir, includeSubdirs: false);

            PrintContextHelp();
            UpdateWindowTitle();
        }

        private void PrintContextHelp()
        {
            Console.WriteLine("Controles:");
            Console.WriteLine(" Q/E : Aula anterior/próxima");
            Console.WriteLine(" N   : Próxima aula recomendada");
            Console.WriteLine(" R   : Resetar estado/câmera");
            Console.WriteLine(" B   : Toggle buffers");
            Console.WriteLine(" C   : Compare mode (quando disponível e sem buffers)");
            Console.WriteLine(" H   : Mostrar/ocultar ajuda detalhada");
            Console.WriteLine(" F5  : Recarregar texturas");
            Console.WriteLine(" ESC : Sair");

            if (_activePipeline == ShaderSelector.LearningPipeline.VertexFragment3D)
            {
                Console.WriteLine(" WASD : Mover câmera (3D)");
                Console.WriteLine(" Z/X  : Trocar modelo (aulas não fixas)");
            }

            Console.WriteLine();
            Console.WriteLine("Aula atual:");
            PrintLessonSummaryToConsole(_activeLesson);
        }

        private void PrintLessonSummaryToConsole(ShaderSelector.ShaderLesson lesson)
        {
            Console.WriteLine($" - {lesson.Title} [{lesson.PipelineLabel}]");
            Console.WriteLine($" - Categoria: {lesson.Category} | Dificuldade: {lesson.Difficulty} | Ordem: {lesson.RecommendedOrder}");
            Console.WriteLine($" - Objetivo: {lesson.LearningObjective}");
            Console.WriteLine($" - Recursos: {lesson.Features.ToBadgeText()}");
            if (lesson.HasReference) Console.WriteLine(" - Compare mode disponível para esta aula.");
            Console.WriteLine();
        }

        private string ResolveTexturesFolder()
        {
            var candidates = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "Textures"),
                Path.Combine(AppContext.BaseDirectory, "textures"),
                Path.Combine(Directory.GetCurrentDirectory(), "Textures"),
                Path.Combine(Directory.GetCurrentDirectory(), "textures"),
                Path.Combine(AppContext.BaseDirectory, "Assets", "Textures"),
                Path.Combine(AppContext.BaseDirectory, "Resources", "Textures"),
            };

            foreach (var c in candidates)
                if (Directory.Exists(c)) return c;

            return Path.Combine(Directory.GetCurrentDirectory(), "Textures");
        }

        private void ReleaseLoadedTextures()
        {
            foreach (var tex in _loadedTextures)
            {
                if (tex != 0) GL.DeleteTexture(tex);
            }
            _loadedTextures.Clear();
            _loadedTextureNames.Clear();
        }

        private void LoadTexturesFromFolder(string folderPath, bool includeSubdirs = false)
        {
            ReleaseLoadedTextures();

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"[TEXTURE] Pasta não encontrada: {folderPath}");
                return;
            }

            var searchOption = includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp", ".tga" };

            int found = 0, ok = 0, fail = 0;

            foreach (var file in Directory.EnumerateFiles(folderPath, "*.*", searchOption))
            {
                var ext = Path.GetExtension(file);
                if (!allowed.Contains(ext)) continue;

                found++;
                int texId = LoadTexture(file);
                if (texId != 0)
                {
                    _loadedTextures.Add(texId);
                    _loadedTextureNames.Add(Path.GetFileName(file));
                    ok++;
                }
                else fail++;
            }

            Console.WriteLine($"[TEXTURE] varridos: {found}, carregados: {ok}, falhas: {fail}");
        }

        private int LoadTexture(string path)
        {
            try
            {
                using var image = Image.Load<Rgba32>(path);
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                var pixels = new byte[image.Width * image.Height * 4];
                image.CopyPixelDataTo(pixels);

                int texId = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texId);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, 0);
                return texId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEXTURE] Falha: {path} - {ex.Message}");
                return 0;
            }
        }

        private void SetCommonUniforms(Shader shader)
        {
            shader.SetFloat("iTime", _time);
            shader.SetVector2("iResolution", new Vector2(Size.X, Size.Y));
            shader.SetVector2("iMouse", new Vector2(MouseState.X, Size.Y - MouseState.Y));
            shader.SetInt("iMouseClick", MouseState.IsButtonDown(MouseButton.Left) ? 1 : 0);
            shader.SetVector3("viewPos", _cameraPos);

            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out int maxUnits);
            int startUnit = 1;
            int canBind = Math.Min(_loadedTextures.Count, Math.Max(0, maxUnits - startUnit));

            for (int i = 0; i < canBind; i++)
            {
                int unit = startUnit + i;
                GL.ActiveTexture(TextureUnit.Texture0 + unit);
                GL.BindTexture(TextureTarget.Texture2D, _loadedTextures[i]);
                shader.SetInt($"texture{i}", unit);
            }
        }

        private void LoadLesson(ShaderSelector.ShaderLesson lesson)
        {
            _activeLesson = lesson;
            _useBuffers = lesson.AutoEnableBuffers || _useBuffers;

            try
            {
                _shader?.Dispose();
                _shader = new Shader(lesson.Shader.VertexPath, lesson.Shader.FragmentPath);
                TryLoadReferenceShader(lesson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar aula {lesson.Title}: {ex.Message}");
            }

            if (_activePipeline == ShaderSelector.LearningPipeline.Fragment2D)
            {
                _renderMode = RenderMode.Fullscreen2D;
            }

            UpdateWindowTitle();

            if (_showHelp)
            {
                Console.WriteLine("Aula carregada:");
                PrintLessonSummaryToConsole(lesson);
            }
        }

        private void TryLoadReferenceShader(ShaderSelector.ShaderLesson lesson)
        {
            _referenceShader?.Dispose();
            _referenceShader = null;

            if (!lesson.HasReference) return;

            var referenceLesson = _pipelineLessons.FirstOrDefault(l => l.Id == lesson.ReferenceLessonId);
            if (referenceLesson == null) return;

            try
            {
                _referenceShader = new Shader(referenceLesson.Shader.VertexPath, referenceLesson.Shader.FragmentPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Não foi possível carregar shader de referência: {ex.Message}");
                _referenceShader = null;
            }
        }

        private void UpdateWindowTitle()
        {
            var pipelineBadge = _activePipeline == ShaderSelector.LearningPipeline.Fragment2D ? "2D Fragment" : "3D Vertex+Fragment";
            var bufferBadge = _useBuffers ? "Buffers ON" : "Buffers OFF";
            var textureBadge = _loadedTextures.Count > 0 ? $"Textures:{_loadedTextures.Count}" : "No Textures";
            var compareBadge = _compareMode ? "Compare ON" : "Compare OFF";

            Title = $"GLSL Shader Lab | {pipelineBadge} | Lesson: {_activeLesson.Title} | {bufferBadge} | {textureBadge} | {compareBadge}";
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Keys.Escape:
                    Close();
                    return;
                case Keys.Q:
                    CycleLesson(-1);
                    return;
                case Keys.E:
                    CycleLesson(1);
                    return;
                case Keys.N:
                    JumpToNextRecommended();
                    return;
                case Keys.R:
                    ResetState();
                    return;
                case Keys.B:
                    _useBuffers = !_useBuffers;
                    if (_useBuffers && _compareMode)
                    {
                        _compareMode = false;
                        Console.WriteLine("Compare mode desativado: buffers ativos não suportam comparação lado a lado.");
                    }
                    UpdateWindowTitle();
                    Console.WriteLine("Buffer system: " + (_useBuffers ? "ON" : "OFF"));
                    return;
                case Keys.C:
                    ToggleCompareMode();
                    return;
                case Keys.H:
                    _showHelp = !_showHelp;
                    Console.WriteLine(_showHelp ? "Ajuda ativada" : "Ajuda desativada");
                    if (_showHelp)
                    {
                        PrintContextHelp();
                    }
                    return;
                case Keys.F5:
                    Console.WriteLine("[TEXTURE] Recarregando...");
                    _resolvedTextureDir = ResolveTexturesFolder();
                    LoadTexturesFromFolder(_resolvedTextureDir, includeSubdirs: false);
                    UpdateWindowTitle();
                    return;
                case Keys.Z:
                    if (_activePipeline == ShaderSelector.LearningPipeline.VertexFragment3D && !_activeLesson.UseFixedStarterModel)
                        ChangeModel(-1);
                    return;
                case Keys.X:
                    if (_activePipeline == ShaderSelector.LearningPipeline.VertexFragment3D && !_activeLesson.UseFixedStarterModel)
                        ChangeModel(1);
                    return;
            }
        }

        private void ToggleCompareMode()
        {
            if (_useBuffers)
            {
                Console.WriteLine("Compare mode requer buffers OFF.");
                return;
            }

            if (_referenceShader == null)
            {
                Console.WriteLine("Compare mode indisponível: esta aula não possui shader de referência.");
                return;
            }

            _compareMode = !_compareMode;
            UpdateWindowTitle();
            Console.WriteLine($"Compare mode: {(_compareMode ? "ON" : "OFF")}");
        }

        private void CycleLesson(int direction)
        {
            if (_pipelineLessons.Count == 0) return;

            _currentLessonIndex = (_currentLessonIndex + direction + _pipelineLessons.Count) % _pipelineLessons.Count;
            LoadLesson(_pipelineLessons[_currentLessonIndex]);
        }

        private void JumpToNextRecommended()
        {
            if (_pipelineLessons.Count == 0) return;
            var ordered = _pipelineLessons
                .OrderBy(l => l.RecommendedOrder)
                .ThenBy(l => l.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var currentPos = ordered.FindIndex(l => l.Id == _activeLesson.Id);
            if (currentPos < 0) return;

            var next = ordered[(currentPos + 1) % ordered.Count];
            _currentLessonIndex = _pipelineLessons.FindIndex(l => l.Id == next.Id);
            if (_currentLessonIndex < 0) _currentLessonIndex = 0;
            LoadLesson(_pipelineLessons[_currentLessonIndex]);
            Console.WriteLine($"Próxima aula recomendada: {_activeLesson.Title}");
        }

        private void ChangeModel(int direction)
        {
            var modelSelector = new ModelSelector();
            var models = modelSelector.GetAvailableModels();
            if (models.Count == 0) return;

            var currentPath = _model?.Name ?? string.Empty;
            int idx = models.FindIndex(m => string.Equals(Path.GetFileNameWithoutExtension(m.FilePath), currentPath, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) idx = 0;

            idx = (idx + direction + models.Count) % models.Count;
            LoadModel(models[idx]);
        }

        private void LoadModel(ModelSelector.ModelInfo modelInfo)
        {
            try
            {
                _model?.Dispose();
                _model = new Model(modelInfo.FilePath);
                Console.WriteLine($"Modelo carregado: {modelInfo.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar modelo {modelInfo.Name}: {ex.Message}");
            }
        }

        private void ResetState()
        {
            _cameraPos = new Vector3(0.0f, 0.0f, 3.0f);
            _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
            _cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
            _fov = 45.0f;
            _rotationY = 0.0f;
            Console.WriteLine("Estado resetado.");
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (_activePipeline == ShaderSelector.LearningPipeline.VertexFragment3D)
            {
                var input = KeyboardState;
                var speed = 2.5f * (float)args.Time;
                if (input.IsKeyDown(Keys.W)) _cameraPos += _cameraFront * speed;
                if (input.IsKeyDown(Keys.S)) _cameraPos -= _cameraFront * speed;
                if (input.IsKeyDown(Keys.A)) _cameraPos -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * speed;
                if (input.IsKeyDown(Keys.D)) _cameraPos += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * speed;
            }

            _rotationY += (float)args.Time * 30.0f;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            _time += (float)args.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_shader == null)
            {
                SwapBuffers();
                return;
            }

            if (_compareMode && _referenceShader != null)
            {
                RenderComparisonFrame();
            }
            else
            {
                RenderSingleFrame(_shader, 0, 0, Size.X, Size.Y);
            }

            SwapBuffers();
        }

        private void RenderComparisonFrame()
        {
            int leftWidth = Size.X / 2;
            int rightWidth = Size.X - leftWidth;

            RenderSingleFrame(_referenceShader!, 0, 0, leftWidth, Size.Y);
            RenderSingleFrame(_shader!, leftWidth, 0, rightWidth, Size.Y);
        }

        private void RenderSingleFrame(Shader shaderToRender, int viewportX, int viewportY, int viewportW, int viewportH)
        {
            GL.Viewport(viewportX, viewportY, viewportW, viewportH);

            if (_useBuffers && _bufferManager != null)
            {
                if (_renderMode == RenderMode.Fullscreen2D)
                {
                    RenderWithBuffers2D(shaderToRender);
                }
                else
                {
                    RenderWithBuffers3D(shaderToRender);
                }
                return;
            }

            shaderToRender.Use();
            SetCommonUniforms(shaderToRender);

            if (_renderMode == RenderMode.Fullscreen2D)
            {
                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                return;
            }

            var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotationY));
            var view = Matrix4.LookAt(_cameraPos, _cameraPos + _cameraFront, _cameraUp);
            var projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(_fov),
                viewportW / (float)Math.Max(1, viewportH),
                0.1f,
                100.0f);

            shaderToRender.SetMatrix4("model", model);
            shaderToRender.SetMatrix4("view", view);
            shaderToRender.SetMatrix4("projection", projection);
            _model?.Render();
        }

        private void RenderWithBuffers2D(Shader shaderToRender)
        {
            if (_bufferManager == null) return;

            _bufferManager.BindCurrentBufferForWriting();

            shaderToRender.Use();
            SetCommonUniforms(shaderToRender);
            _bufferManager.BindBuffersForReading(shaderToRender);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            _bufferManager.UnbindBuffers();

            GL.Viewport(0, 0, Size.X, Size.Y);
            if (_copyShader != null)
            {
                _copyShader.Use();
                _copyShader.SetVector2("iResolution", new Vector2(Size.X, Size.Y));
                var currentBuffer = _bufferManager.GetCurrentBuffer();
                currentBuffer.BindForReading(TextureUnit.Texture0);
                _copyShader.SetTexture("inputTexture", 0);

                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            _bufferManager.SwapBuffers();
        }

        private void RenderWithBuffers3D(Shader shaderToRender)
        {
            if (_bufferManager == null) return;

            _bufferManager.BindCurrentBufferForWriting();

            shaderToRender.Use();
            SetCommonUniforms(shaderToRender);

            var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotationY));
            var view = Matrix4.LookAt(_cameraPos, _cameraPos + _cameraFront, _cameraUp);
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), Size.X / (float)Size.Y, 0.1f, 100.0f);

            _bufferManager.BindBuffersForReading(shaderToRender);

            shaderToRender.SetMatrix4("model", model);
            shaderToRender.SetMatrix4("view", view);
            shaderToRender.SetMatrix4("projection", projection);
            _model?.Render();

            _bufferManager.UnbindBuffers();

            GL.Viewport(0, 0, Size.X, Size.Y);
            if (_copyShader != null)
            {
                _copyShader.Use();
                _copyShader.SetVector2("iResolution", new Vector2(Size.X, Size.Y));
                var currentBuffer = _bufferManager.GetCurrentBuffer();
                currentBuffer.BindForReading(TextureUnit.Texture0);
                _copyShader.SetTexture("inputTexture", 0);

                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            _bufferManager.SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _bufferManager?.Resize(Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            _shader?.Dispose();
            _referenceShader?.Dispose();
            _model?.Dispose();
            _copyShader?.Dispose();
            _bufferManager?.Dispose();

            ReleaseLoadedTextures();

            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_ebo != 0) GL.DeleteBuffer(_ebo);
            base.OnUnload();
        }
    }
}
