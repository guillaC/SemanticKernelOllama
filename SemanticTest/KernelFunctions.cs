using Microsoft.SemanticKernel;
using SemanticTest.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticTest
{
    public class KernelFunctions
    {
        JsonSerializerOptions jOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        [KernelFunction, Description("Permet de vérifier l'existance d'un processus, retourne une liste JSON simple des processus contenant un nom donné." +
                                     "Chaque objet JSON contient : nom, PID, titre de la fenêtre, mémoire utilisée, chemin de l'exécutable et heure de démarrage.")]
        public string ObtenirInfosProcessus([Description("Nom du processus à rechercher")] string nomProcessus)
        {
            var processus = Process.GetProcesses().Where(p => p.ProcessName.Contains(nomProcessus, StringComparison.CurrentCultureIgnoreCase))
                                                  .Select(p =>
                                                  {
                                                      string startTime = "Non disponible";
                                                      try { startTime = p.StartTime.ToString("s"); } catch { }

                                                      return new InfosProcessus
                                                      {
                                                          Nom = p.ProcessName,
                                                          PID = p.Id,
                                                          TitreFenetre = p.MainWindowTitle,
                                                          MemoireOctets = p.WorkingSet64,
                                                          HeureDemarrage = startTime
                                                      };
                                                  })
                                                  .ToList();

            if (!processus.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    message = $"Aucun processus contenant '{nomProcessus}' trouvé."
                }, jOptions);
            }

            return JsonSerializer.Serialize(processus, jOptions);
        }
    }
}
