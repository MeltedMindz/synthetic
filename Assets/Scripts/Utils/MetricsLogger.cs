using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyntheticLife.Phi.Core;

namespace SyntheticLife.Phi.Utils
{
    public class MetricsLogger : MonoBehaviour
    {
        [Header("Settings")]
        public bool enableLogging = true;
        public string logFileName = "metrics.csv";
        public float logInterval = 1f;

        private List<EpisodeMetrics> episodeMetrics = new List<EpisodeMetrics>();
        private float lastLogTime = 0f;
        private string logPath;

        private struct EpisodeMetrics
        {
            public int episode;
            public float survivalTime;
            public float totalFoodEaten;
            public int scanCount;
            public int reproductionCount;
            public float avgEnergy;
            public float avgTemperature;
            public float avgIntegrity;
            public LifeStage maxStage;
            public int offspringCount;
            public float avgOffspringSurvivalTime;
        }

        private void Start()
        {
            if (enableLogging)
            {
                logPath = Path.Combine(Application.persistentDataPath, logFileName);
                WriteCSVHeader();
            }
        }

        private void Update()
        {
            if (enableLogging && Time.time - lastLogTime >= logInterval)
            {
                CollectMetrics();
                lastLogTime = Time.time;
            }
        }

        private void CollectMetrics()
        {
            CreatureAgent[] creatures = FindObjectsOfType<CreatureAgent>();
            
            foreach (var creature in creatures)
            {
                // This would need to be called from CreatureAgent when episode ends
                // For now, we'll log aggregate stats
            }
        }

        public void LogEpisodeEnd(CreatureAgent agent, float survivalTime, float foodEaten, int scans, int reproductions, 
            float avgEnergy, float avgTemp, float avgIntegrity, LifeStage stage, int offspring)
        {
            if (!enableLogging) return;

            EpisodeMetrics metrics = new EpisodeMetrics
            {
                episode = episodeMetrics.Count,
                survivalTime = survivalTime,
                totalFoodEaten = foodEaten,
                scanCount = scans,
                reproductionCount = reproductions,
                avgEnergy = avgEnergy,
                avgTemperature = avgTemp,
                avgIntegrity = avgIntegrity,
                maxStage = stage,
                offspringCount = offspring,
                avgOffspringSurvivalTime = 0f // Would need tracking
            };

            episodeMetrics.Add(metrics);
            WriteMetricsToCSV(metrics);
        }

        private void WriteCSVHeader()
        {
            string header = "Episode,SurvivalTime,FoodEaten,Scans,Reproductions,AvgEnergy,AvgTemp,AvgIntegrity,MaxStage,OffspringCount,AvgOffspringSurvivalTime\n";
            File.AppendAllText(logPath, header);
        }

        private void WriteMetricsToCSV(EpisodeMetrics metrics)
        {
            string line = $"{metrics.episode},{metrics.survivalTime},{metrics.totalFoodEaten},{metrics.scanCount}," +
                         $"{metrics.reproductionCount},{metrics.avgEnergy},{metrics.avgTemperature},{metrics.avgIntegrity}," +
                         $"{metrics.maxStage},{metrics.offspringCount},{metrics.avgOffspringSurvivalTime}\n";
            File.AppendAllText(logPath, line);
        }
    }
}

