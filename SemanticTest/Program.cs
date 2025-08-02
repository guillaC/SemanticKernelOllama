using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticTest;
using Spectre.Console; 

/// <summary>
/// Ce programme utilise Semantic Kernel avec un modèle Mistral local via Ollama.
/// Il inclut une fonction pour lister les processus en cours.
/// </summary>
class Program
{
    const string MODEL = "mistral-small3.1:24b";

    static async Task Main()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434"),
            Timeout = TimeSpan.FromMinutes(5) // Augmente le timeout à 5 minutes
        };

        var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
                modelId: MODEL,
                httpClient : client);

        builder.Plugins.AddFromObject(new KernelFunctions());

        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true) // Auto-invoke, autorise l'appel des fonctions si besoin
        };

        var chatHistory = new ChatHistory(
          @"Tu t'exprimes uniquement en français.
            Tes réponses sont simples, claires et toujours en moins de 100 mots.
            Tu n'utilises jamais de listes, de tirets ou de mise en forme spéciale.
            Tu peux appeler une fonction native seulement si elle est directement liée à la demande de l'utilisateur."
      );

        while (true)
        {
            AnsiConsole.Markup($"[bold darkorange]{Environment.UserName}[/]: ");
            var message = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(message))
            {
                chatHistory.AddUserMessage(message);

                var result = await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Triangle)
                    .SpinnerStyle(Style.Parse("lightpink1"))
                    .StartAsync($"En attente de {MODEL}...", async ctx =>
                    {
                        return await chatService.GetChatMessageContentAsync(
                            chatHistory,
                            kernel: kernel,
                            executionSettings: settings);
                    });

                if (!string.IsNullOrWhiteSpace(result.Content))
                {
                    AnsiConsole.Markup($"[bold lightpink1]{MODEL}[/]: ");
                    AnsiConsole.WriteLine(result.Content);

                    chatHistory.AddAssistantMessage(result.Content);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[bold indianred]Message vide, veuillez réessayer.[/]");
                continue;
            }
        }
    }
}