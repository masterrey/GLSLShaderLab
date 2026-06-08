using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GLSLShaderLab
{
    public class ShaderSelector
    {
        public enum LearningPipeline
        {
            Fragment2D,
            VertexFragment3D
        }

        public enum LessonDifficulty
        {
            Intro,
            Practice,
            Advanced
        }

        public enum LessonSelectionMode
        {
            CuratedCurriculum,
            AllShaders
        }

        public class LessonFeatures
        {
            public bool FragmentOnly2D { get; set; }
            public bool UsesVertexStage { get; set; }
            public bool UsesMouseInput { get; set; }
            public bool UsesBuffers { get; set; }
            public bool UsesTextures { get; set; }
            public bool UsesModelData { get; set; }

            public string ToBadgeText()
            {
                var flags = new List<string>();
                if (FragmentOnly2D) flags.Add("Fragment-Only");
                if (UsesVertexStage) flags.Add("Vertex Stage");
                if (UsesMouseInput) flags.Add("Mouse");
                if (UsesBuffers) flags.Add("Buffers");
                if (UsesTextures) flags.Add("Textures");
                if (UsesModelData) flags.Add("3D Model");
                return flags.Count > 0 ? string.Join(", ", flags) : "No special requirements";
            }
        }

        public class ShaderInfo
        {
            public string Name { get; set; }
            public string VertexPath { get; set; }
            public string FragmentPath { get; set; }

            public ShaderInfo(string name, string vertexPath, string fragmentPath)
            {
                Name = name;
                VertexPath = vertexPath;
                FragmentPath = fragmentPath;
            }

            public override string ToString() =>
                $"{Name} (Vertex: {Path.GetFileName(VertexPath)}, Fragment: {Path.GetFileName(FragmentPath)})";
        }

        public class ShaderLesson
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public LearningPipeline Pipeline { get; set; }
            public LessonDifficulty Difficulty { get; set; }
            public int RecommendedOrder { get; set; }
            public string Description { get; set; } = string.Empty;
            public string LearningObjective { get; set; } = string.Empty;
            public LessonFeatures Features { get; set; } = new();
            public bool Curated { get; set; } = true;
            public bool AutoEnableBuffers { get; set; }
            public bool UseFixedStarterModel { get; set; }
            public string? RecommendedModelFileName { get; set; }
            public string? ReferenceLessonId { get; set; }
            public required ShaderInfo Shader { get; set; }

            public string PipelineLabel => Pipeline == LearningPipeline.Fragment2D ? "2D Fragment" : "3D Vertex+Fragment";
            public bool HasReference => !string.IsNullOrWhiteSpace(ReferenceLessonId);
        }

        private readonly List<ShaderInfo> _availableShaders;
        private readonly List<ShaderLesson> _allLessons;

        private const string ShadersPath = "Shaders";

        public ShaderSelector()
        {
            _availableShaders = ScanForShaders();
            _allLessons = BuildLessons();
        }

        private List<ShaderInfo> ScanForShaders()
        {
            var shaders = new List<ShaderInfo>();

            if (!Directory.Exists(ShadersPath))
            {
                Console.WriteLine($"Diretório {ShadersPath} não encontrado!");
                return shaders;
            }

            var vertFiles = Directory.GetFiles(ShadersPath, "*.vert", SearchOption.AllDirectories);

            foreach (var vertFile in vertFiles)
            {
                var baseName = Path.GetFileNameWithoutExtension(vertFile);
                var directory = Path.GetDirectoryName(vertFile);
                var fragFile = Path.Combine(directory!, $"{baseName}.frag");

                if (!File.Exists(fragFile))
                {
                    Console.WriteLine($"Aviso: Arquivo .frag correspondente não encontrado para {vertFile}");
                    continue;
                }

                var displayName = Path.GetRelativePath(ShadersPath, directory!) == "."
                    ? baseName
                    : $"{Path.GetRelativePath(ShadersPath, directory!)}/{baseName}";

                shaders.Add(new ShaderInfo(displayName, vertFile, fragFile));
            }

            shaders.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return shaders;
        }

        private List<ShaderLesson> BuildLessons()
        {
            var byName = _availableShaders.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);
            var lessons = new List<ShaderLesson>();

            void AddCurated(
                string shaderName,
                string id,
                string title,
                string category,
                LearningPipeline pipeline,
                LessonDifficulty difficulty,
                int order,
                string description,
                string objective,
                LessonFeatures features,
                bool autoEnableBuffers = false,
                bool useFixedStarterModel = false,
                string? recommendedModelFileName = null,
                string? referenceLessonId = null)
            {
                if (!byName.TryGetValue(shaderName, out var shader)) return;

                lessons.Add(new ShaderLesson
                {
                    Id = id,
                    Title = title,
                    Category = category,
                    Pipeline = pipeline,
                    Difficulty = difficulty,
                    RecommendedOrder = order,
                    Description = description,
                    LearningObjective = objective,
                    Features = features,
                    Curated = true,
                    AutoEnableBuffers = autoEnableBuffers,
                    UseFixedStarterModel = useFixedStarterModel,
                    RecommendedModelFileName = recommendedModelFileName,
                    ReferenceLessonId = referenceLessonId,
                    Shader = shader
                });
            }

            // 2D Fragment pipeline
            AddCurated("shader", "2d-color-basics", "Color & Time Basics", "2D Basics", LearningPipeline.Fragment2D,
                LessonDifficulty.Intro, 1,
                "Introduz cores, tempo e variação procedural em tela cheia.",
                "Entender gl_FragCoord, iResolution e iTime.",
                new LessonFeatures { FragmentOnly2D = true });

            AddCurated("Japao", "2d-shapes-japao", "Shape Composition", "2D Basics", LearningPipeline.Fragment2D,
                LessonDifficulty.Intro, 2,
                "Mostra composição de formas simples em shader fragment.",
                "Construir formas 2D por distância e máscaras.",
                new LessonFeatures { FragmentOnly2D = true });

            AddCurated("Grece", "2d-patterns-grece", "Pattern Composition", "2D Basics", LearningPipeline.Fragment2D,
                LessonDifficulty.Practice, 3,
                "Explora padrões repetitivos e composição geométrica.",
                "Praticar operações de repetição e mistura de cores.",
                new LessonFeatures { FragmentOnly2D = true });

            AddCurated("Ripple", "2d-ripple", "Animated Ripples", "2D Animation", LearningPipeline.Fragment2D,
                LessonDifficulty.Practice, 4,
                "Introduz animação procedural com ondas temporais.",
                "Controlar animação por tempo e coordenadas.",
                new LessonFeatures { FragmentOnly2D = true });

            AddCurated("SimplePaint", "2d-mouse-paint", "Mouse Interaction Paint", "2D Interaction", LearningPipeline.Fragment2D,
                LessonDifficulty.Practice, 5,
                "Pintura simples guiada por mouse.",
                "Usar iMouse e iMouseClick para interação.",
                new LessonFeatures { FragmentOnly2D = true, UsesMouseInput = true },
                referenceLessonId: "2d-buffer-paint");

            AddCurated("PaintTutorial", "2d-buffer-paint", "Persistent Paint with Buffer", "2D Buffers", LearningPipeline.Fragment2D,
                LessonDifficulty.Practice, 6,
                "Pintura persistente com feedback de frame anterior.",
                "Entender ping-pong buffer via iChannel0.",
                new LessonFeatures { FragmentOnly2D = true, UsesMouseInput = true, UsesBuffers = true },
                autoEnableBuffers: true,
                referenceLessonId: "2d-mouse-paint");

            AddCurated("Functions", "2d-feedback-functions", "Feedback Functions", "2D Buffers", LearningPipeline.Fragment2D,
                LessonDifficulty.Advanced, 7,
                "Explora efeitos acumulativos e feedback visual.",
                "Aplicar funções matemáticas com feedback entre frames.",
                new LessonFeatures { FragmentOnly2D = true, UsesBuffers = true },
                autoEnableBuffers: true);

            AddCurated("BufferDemo", "2d-buffer-demo", "Advanced Buffer Demo", "Advanced Effects", LearningPipeline.Fragment2D,
                LessonDifficulty.Advanced, 8,
                "Demonstra um fluxo mais avançado com buffers.",
                "Consolidar pipeline de buffers e passagens.",
                new LessonFeatures { FragmentOnly2D = true, UsesBuffers = true },
                autoEnableBuffers: true);

            AddCurated("Clouds", "2d-clouds", "Procedural Clouds", "Advanced Effects", LearningPipeline.Fragment2D,
                LessonDifficulty.Advanced, 9,
                "Shader procedural de nuvens em tela cheia.",
                "Estudar noise procedural e marching simplificado.",
                new LessonFeatures { FragmentOnly2D = true });

            // 3D Vertex + Fragment pipeline
            AddCurated("Simple3d", "3d-intro-light", "3D Lighting Intro", "3D Lighting", LearningPipeline.VertexFragment3D,
                LessonDifficulty.Intro, 1,
                "Primeiro shader 3D com normais e luz difusa.",
                "Entender normal, posição e iluminação básica.",
                new LessonFeatures { UsesVertexStage = true, UsesModelData = true },
                useFixedStarterModel: true,
                recommendedModelFileName: "cube.glb");

            AddCurated("basic3d", "3d-specular", "Specular Lighting", "3D Lighting", LearningPipeline.VertexFragment3D,
                LessonDifficulty.Practice, 2,
                "Adiciona iluminação especular e variação de cor.",
                "Compreender difusa + especular + viewPos.",
                new LessonFeatures { UsesVertexStage = true, UsesModelData = true },
                useFixedStarterModel: true,
                recommendedModelFileName: "cube.glb");

            AddCurated("waves", "3d-vertex-waves", "Vertex Deformation", "3D Deformation", LearningPipeline.VertexFragment3D,
                LessonDifficulty.Practice, 3,
                "Deforma vértices em tempo real com funções seno.",
                "Aprender manipulação de geometria no vertex shader.",
                new LessonFeatures { UsesVertexStage = true, UsesModelData = true },
                useFixedStarterModel: true,
                recommendedModelFileName: "sphere.glb");

            AddCurated("AnimatedFlag", "3d-advanced-flag", "Animated Flag", "Advanced Effects", LearningPipeline.VertexFragment3D,
                LessonDifficulty.Advanced, 4,
                "Efeito avançado de animação em malha 3D.",
                "Consolidar deformação, iluminação e animação.",
                new LessonFeatures { UsesVertexStage = true, UsesModelData = true },
                useFixedStarterModel: false);

            AddCurated("Normal", "3d-normal-practice", "Normals Practice", "3D Lighting", LearningPipeline.VertexFragment3D,
                LessonDifficulty.Advanced, 5,
                "Prática focada em resposta de normais no fragment.",
                "Ajustar iluminação em função das normais.",
                new LessonFeatures { UsesVertexStage = true, UsesModelData = true },
                useFixedStarterModel: false);

            var curatedShaderNames = new HashSet<string>(lessons.Select(l => l.Shader.Name), StringComparer.OrdinalIgnoreCase);
            foreach (var shader in _availableShaders)
            {
                if (curatedShaderNames.Contains(shader.Name)) continue;

                var isLikely3D = IsLikely3DShader(shader.Name);
                lessons.Add(new ShaderLesson
                {
                    Id = $"experimental-{shader.Name.ToLowerInvariant().Replace('/', '-')}",
                    Title = shader.Name,
                    Category = "Experimental",
                    Pipeline = isLikely3D ? LearningPipeline.VertexFragment3D : LearningPipeline.Fragment2D,
                    Difficulty = LessonDifficulty.Advanced,
                    RecommendedOrder = int.MaxValue,
                    Description = "Shader experimental fora da trilha principal.",
                    LearningObjective = "Exploração livre.",
                    Features = new LessonFeatures
                    {
                        FragmentOnly2D = !isLikely3D,
                        UsesVertexStage = isLikely3D,
                        UsesModelData = isLikely3D
                    },
                    Curated = false,
                    Shader = shader,
                    AutoEnableBuffers = shader.Name.Contains("Paint", StringComparison.OrdinalIgnoreCase) ||
                                        shader.Name.Contains("Buffer", StringComparison.OrdinalIgnoreCase) ||
                                        shader.Name.Contains("Functions", StringComparison.OrdinalIgnoreCase)
                });
            }

            return lessons
                .OrderBy(l => l.Pipeline)
                .ThenBy(l => l.RecommendedOrder)
                .ThenBy(l => l.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool IsLikely3DShader(string shaderName)
        {
            return shaderName.Contains("3d", StringComparison.OrdinalIgnoreCase)
                || shaderName.Contains("wave", StringComparison.OrdinalIgnoreCase)
                || shaderName.Contains("normal", StringComparison.OrdinalIgnoreCase)
                || shaderName.Contains("flag", StringComparison.OrdinalIgnoreCase)
                || shaderName.Contains("basic", StringComparison.OrdinalIgnoreCase)
                || shaderName.Contains("simple3d", StringComparison.OrdinalIgnoreCase);
        }

        public LearningPipeline SelectPipeline()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== PIPELINE DE APRENDIZADO ===");
                Console.WriteLine();
                Console.WriteLine("  1. Fragment Shader 2D (Iniciante)");
                Console.WriteLine("  2. Vertex + Fragment 3D (Intermediário/Avançado)");
                Console.WriteLine();
                Console.Write("Escolha um pipeline (1-2): ");

                var input = Console.ReadLine()?.Trim();
                if (input == "1") return LearningPipeline.Fragment2D;
                if (input == "2") return LearningPipeline.VertexFragment3D;

                Console.WriteLine("Opção inválida. Pressione qualquer tecla para tentar novamente...");
                Console.ReadKey();
            }
        }

        public LessonSelectionMode SelectLessonMode()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Modo de seleção de aulas:");
                Console.WriteLine("  1. Trilha recomendada (curada)");
                Console.WriteLine("  2. Todos os shaders (inclui experimentais)");
                Console.Write("Escolha (1-2): ");

                var input = Console.ReadLine()?.Trim();
                if (input == "1") return LessonSelectionMode.CuratedCurriculum;
                if (input == "2") return LessonSelectionMode.AllShaders;

                Console.WriteLine("Opção inválida.");
            }
        }

        public List<ShaderLesson> GetLessonsForPipeline(LearningPipeline pipeline, LessonSelectionMode mode)
        {
            return _allLessons
                .Where(l => l.Pipeline == pipeline)
                .Where(l => mode == LessonSelectionMode.AllShaders || l.Curated)
                .OrderBy(l => l.RecommendedOrder)
                .ThenBy(l => l.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public ShaderLesson? SelectLesson(LearningPipeline pipeline, LessonSelectionMode mode)
        {
            var lessons = GetLessonsForPipeline(pipeline, mode);
            if (lessons.Count == 0)
            {
                Console.WriteLine("Nenhuma aula disponível para o pipeline selecionado.");
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey();
                return null;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SELETOR DE AULAS ===");
                Console.WriteLine();
                Console.WriteLine($"Pipeline: {(pipeline == LearningPipeline.Fragment2D ? "2D Fragment" : "3D Vertex+Fragment")}");
                Console.WriteLine($"Modo: {(mode == LessonSelectionMode.CuratedCurriculum ? "Trilha Curada" : "Todos os Shaders")}");
                Console.WriteLine($"Aulas disponíveis: {lessons.Count}");
                Console.WriteLine();

                var groups = lessons.GroupBy(l => l.Category).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
                int index = 1;
                var indexMap = new Dictionary<int, ShaderLesson>();

                foreach (var group in groups)
                {
                    Console.WriteLine($"[{group.Key}]");
                    foreach (var lesson in group)
                    {
                        var curatedTag = lesson.Curated ? "" : " [Experimental]";
                        Console.WriteLine($"  {index,2}. {lesson.Title} ({lesson.Difficulty}){curatedTag}");
                        indexMap[index] = lesson;
                        index++;
                    }
                    Console.WriteLine();
                }

                Console.Write("Selecione uma aula (número): ");
                var input = Console.ReadLine()?.Trim();
                if (!int.TryParse(input, out var selected) || !indexMap.TryGetValue(selected, out var lessonSelected))
                {
                    Console.WriteLine("Seleção inválida! Pressione qualquer tecla para tentar novamente...");
                    Console.ReadKey();
                    continue;
                }

                PrintLessonSummary(lessonSelected);
                Console.Write("Iniciar esta aula? (s/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirm == "s" || confirm == "sim" || confirm == "y")
                    return lessonSelected;
            }
        }

        public void PrintLessonSummary(ShaderLesson lesson)
        {
            Console.WriteLine();
            Console.WriteLine("=== RESUMO DA AULA ===");
            Console.WriteLine($"Título: {lesson.Title}");
            Console.WriteLine($"Pipeline: {lesson.PipelineLabel}");
            Console.WriteLine($"Categoria: {lesson.Category}");
            Console.WriteLine($"Dificuldade: {lesson.Difficulty}");
            Console.WriteLine($"Ordem recomendada: {lesson.RecommendedOrder}");
            Console.WriteLine($"Descrição: {lesson.Description}");
            Console.WriteLine($"Objetivo: {lesson.LearningObjective}");
            Console.WriteLine($"Recursos: {lesson.Features.ToBadgeText()}");
            Console.WriteLine($"Shader: {lesson.Shader.Name}");
            if (lesson.AutoEnableBuffers) Console.WriteLine("Buffers: Ativação recomendada/automática");
            if (lesson.UseFixedStarterModel && !string.IsNullOrWhiteSpace(lesson.RecommendedModelFileName))
                Console.WriteLine($"Modelo sugerido: {lesson.RecommendedModelFileName} (fixo para início)");
            if (lesson.HasReference) Console.WriteLine($"Comparação disponível com: {lesson.ReferenceLessonId}");
            Console.WriteLine();
        }

        public ShaderLesson? FindLessonById(string lessonId)
        {
            return _allLessons.FirstOrDefault(l => string.Equals(l.Id, lessonId, StringComparison.OrdinalIgnoreCase));
        }

        public List<ShaderInfo> GetAvailableShaders() => _availableShaders;
    }
}
