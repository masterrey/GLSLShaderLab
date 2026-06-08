using System;

namespace GLSLShaderLab
{
    class Program
    {
        static void Main(string[] args)
        {
            var shaderSelector = new ShaderSelector();
            var modelSelector = new ModelSelector();

            var pipeline = shaderSelector.SelectPipeline();
            var lessonMode = shaderSelector.SelectLessonMode();
            var selectedLesson = shaderSelector.SelectLesson(pipeline, lessonMode);

            if (selectedLesson == null)
            {
                Console.WriteLine("Nenhuma aula selecionada. Encerrando programa.");
                return;
            }

            var lessonsForPipeline = shaderSelector.GetLessonsForPipeline(pipeline, lessonMode);

            ModelSelector.ModelInfo? selectedModel;
            if (pipeline == ShaderSelector.LearningPipeline.Fragment2D)
            {
                selectedModel = modelSelector.GetDefaultModel();
                if (selectedModel == null)
                {
                    Console.WriteLine("Nenhum modelo disponível para inicializar a sessão. Encerrando programa.");
                    return;
                }
            }
            else
            {
                if (selectedLesson.UseFixedStarterModel && !string.IsNullOrWhiteSpace(selectedLesson.RecommendedModelFileName))
                {
                    selectedModel = modelSelector.TryGetModelByFileName(selectedLesson.RecommendedModelFileName)
                                  ?? modelSelector.GetDefaultModel();

                    if (selectedModel != null)
                    {
                        Console.WriteLine($"Modelo didático fixo selecionado: {selectedModel.Name}");
                    }
                    else
                    {
                        Console.WriteLine("Não foi possível encontrar modelo padrão para aula 3D. Encerrando programa.");
                        return;
                    }
                }
                else
                {
                    selectedModel = modelSelector.SelectModel(showAdvancedPrompt: true);
                    if (selectedModel == null)
                    {
                        Console.WriteLine("Nenhum modelo selecionado. Encerrando programa.");
                        return;
                    }
                }
            }

            using (var window = new Window(
                       1280,
                       720,
                       "GLSL Shader Lab",
                       selectedLesson,
                       selectedModel,
                       lessonsForPipeline,
                       pipeline))
            {
                window.Run();
            }
        }
    }
}
