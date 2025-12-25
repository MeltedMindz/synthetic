using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.MLAgents;
using SyntheticLife.Phi.Config;
using SyntheticLife.Phi.Core;

namespace SyntheticLife.Phi.Utils
{
    public class TelemetryManager : MonoBehaviour
    {
        private static TelemetryManager _instance;
        public static TelemetryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("TelemetryManager");
                    _instance = go.AddComponent<TelemetryManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Config")]
        public TelemetryConfig config;

        private string runId;
        private string runPath;
        private int episodeCounter = 0;
        private Dictionary<int, EpisodeData> activeEpisodes = new Dictionary<int, EpisodeData>();
        private Queue<string> eventBuffer = new Queue<string>();
        private float lastWorldSnapshot = 0f;
        private System.Random runRNG;
        private int birthCounter = 0;
        private Dictionary<int, AgentLineage> lineages = new Dictionary<int, AgentLineage>();

        private struct EpisodeData
        {
            public int episodeIndex;
            public int agentId;
            public float startTime;
            public float startSimTime;
            public int startStep;
            public int foodEatenCount;
            public float totalNutrientsGained;
            public int scanCount;
            public int reproduceAttempts;
            public int reproduceSuccessCount;
            public int damageEvents;
            public float totalDamage;
            public LifeStage stageMaxReached;
            public float totalEnergy;
            public float totalTemp;
            public float totalIntegrity;
            public int energySamples;
            public int tempSamples;
            public int integritySamples;
            public string causeOfDeath;
            public Genome genomeAtStart;
            public Genome genomeAtEnd;
        }

        private struct AgentLineage
        {
            public int agentId;
            public int parentId;
            public int generation;
            public int birthIndex;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (config == null || !config.enableTelemetry) return;

            InitializeRun();
        }

        private void InitializeRun()
        {
            runId = $"run_{DateTime.Now:yyyyMMdd_HHmmss}";
            runPath = Path.Combine(Application.persistentDataPath, "Runs", runId);
            Directory.CreateDirectory(runPath);
            Directory.CreateDirectory(Path.Combine(runPath, "telemetry"));
            Directory.CreateDirectory(Path.Combine(runPath, "telemetry", "trajectories"));
            Directory.CreateDirectory(Path.Combine(runPath, "world"));
            Directory.CreateDirectory(Path.Combine(runPath, "replay"));

            // Create run seed
            int runSeed = Environment.TickCount;
            runRNG = new System.Random(runSeed);

            // Write manifest
            WriteManifest(runSeed);

            // Initialize episode CSV
            if (config.logEpisodes)
            {
                WriteEpisodeCSVHeader();
            }
        }

        private void WriteManifest(int runSeed)
        {
            var manifest = new
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                runId = runId,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                unityVersion = Application.unityVersion,
                mlAgentsPackageVersion = "2.0.1",
                randomSeed = runSeed,
                configs = new
                {
                    creatureDefaults = GetConfigJSON("CreatureDefaults"),
                    phiFieldConfig = GetConfigJSON("PhiFieldConfig"),
                    rewardConfig = GetConfigJSON("RewardConfig"),
                    spawnerConfig = GetConfigJSON("SpawnerConfig"),
                    telemetryConfig = GetConfigJSON("TelemetryConfig")
                },
                yamlConfig = GetYAMLConfigInfo()
            };

            string json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(Path.Combine(runPath, "manifest.json"), json);
        }

        private string GetConfigJSON(string configName)
        {
            // This would need to serialize the ScriptableObjects
            // For now, return a placeholder
            return "{}";
        }

        private string GetYAMLConfigInfo()
        {
            // Check if YAML config path is available
            return "ppo_life_v1.yaml";
        }

        private void WriteEpisodeCSVHeader()
        {
            string header = "runId,episodeIndex,agentId,episodeStartTime,episodeEndTime,durationSeconds,survivalSteps," +
                           "foodEatenCount,totalNutrientsGained,scanCount,reproduceAttempts,reproduceSuccessCount," +
                           "damageEvents,totalDamage,stageMaxReached,finalStage,meanEnergy,meanTemp,meanIntegrity," +
                           "causeOfDeath,genomeSignature,hungerUrgencyBias,dangerAversionBias,scanCuriosityBias," +
                           "explorationNoiseBias,reproductionConservatism\n";
            File.WriteAllText(Path.Combine(runPath, "telemetry", "episodes.csv"), header);
        }

        public void LogEpisodeStart(int agentId, int stepIndex, Genome genome)
        {
            if (!config.enableTelemetry) return;

            int episodeIndex = episodeCounter++;
            var episode = new EpisodeData
            {
                episodeIndex = episodeIndex,
                agentId = agentId,
                startTime = Time.time,
                startSimTime = Time.time,
                startStep = stepIndex,
                stageMaxReached = LifeStage.Juvenile,
                genomeAtStart = genome
            };
            activeEpisodes[agentId] = episode;
        }

        public void LogEvent(string eventType, int agentId, int stepIndex, Dictionary<string, object> data)
        {
            if (!config.enableTelemetry || !config.logEvents) return;

            var eventObj = new Dictionary<string, object>
            {
                ["eventType"] = eventType,
                ["runId"] = runId,
                ["episodeIndex"] = activeEpisodes.ContainsKey(agentId) ? activeEpisodes[agentId].episodeIndex : -1,
                ["simTime"] = Time.time,
                ["stepIndex"] = stepIndex,
                ["agentId"] = agentId
            };

            if (lineages.ContainsKey(agentId))
            {
                eventObj["parentId"] = lineages[agentId].parentId;
                eventObj["generation"] = lineages[agentId].generation;
            }

            foreach (var kvp in data)
            {
                eventObj[kvp.Key] = kvp.Value;
            }

            string json = SerializeEvent(eventObj);
            eventBuffer.Enqueue(json);

            if (eventBuffer.Count >= config.eventsBufferSize)
            {
                FlushEvents();
            }

            // Handle specific event types
            if (eventType == "BIRTH")
            {
                HandleBirthEvent(agentId, data);
            }
            else if (eventType == "DEATH")
            {
                HandleDeathEvent(agentId, data);
            }
            else if (eventType == "EAT")
            {
                HandleEatEvent(agentId, data);
            }
            else if (eventType == "SCAN")
            {
                HandleScanEvent(agentId);
            }
            else if (eventType == "REPRODUCE_ATTEMPT")
            {
                HandleReproduceAttempt(agentId);
            }
            else if (eventType == "REPRODUCE_SUCCESS")
            {
                HandleReproduceSuccess(agentId);
            }
            else if (eventType == "DAMAGE")
            {
                HandleDamageEvent(agentId, data);
            }
            else if (eventType == "STAGE_CHANGE")
            {
                HandleStageChange(agentId, data);
            }
        }

        private void HandleBirthEvent(int agentId, Dictionary<string, object> data)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                int parentId = data.ContainsKey("parentId") ? (int)data["parentId"] : -1;
                int generation = data.ContainsKey("generation") ? (int)data["generation"] : 0;

                lineages[agentId] = new AgentLineage
                {
                    agentId = agentId,
                    parentId = parentId,
                    generation = generation,
                    birthIndex = birthCounter++
                };
            }
        }

        private void HandleDeathEvent(int agentId, Dictionary<string, object> data)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.causeOfDeath = data.ContainsKey("cause") ? data["cause"].ToString() : "unknown";
                if (data.ContainsKey("genome"))
                {
                    episode.genomeAtEnd = (Genome)data["genome"];
                }
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleEatEvent(int agentId, Dictionary<string, object> data)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.foodEatenCount++;
                if (data.ContainsKey("nutrients"))
                {
                    episode.totalNutrientsGained += Convert.ToSingle(data["nutrients"]);
                }
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleScanEvent(int agentId)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.scanCount++;
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleReproduceAttempt(int agentId)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.reproduceAttempts++;
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleReproduceSuccess(int agentId)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.reproduceSuccessCount++;
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleDamageEvent(int agentId, Dictionary<string, object> data)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                episode.damageEvents++;
                if (data.ContainsKey("damage"))
                {
                    episode.totalDamage += Convert.ToSingle(data["damage"]);
                }
                activeEpisodes[agentId] = episode;
            }
        }

        private void HandleStageChange(int agentId, Dictionary<string, object> data)
        {
            if (activeEpisodes.ContainsKey(agentId))
            {
                var episode = activeEpisodes[agentId];
                if (data.ContainsKey("stage"))
                {
                    LifeStage newStage = (LifeStage)data["stage"];
                    if (newStage > episode.stageMaxReached)
                    {
                        episode.stageMaxReached = newStage;
                    }
                }
                activeEpisodes[agentId] = episode;
            }
        }

        public void UpdateEpisodeMetrics(int agentId, float energy, float temp, float integrity)
        {
            if (!config.enableTelemetry || !activeEpisodes.ContainsKey(agentId)) return;

            var episode = activeEpisodes[agentId];
            episode.totalEnergy += energy;
            episode.energySamples++;
            episode.totalTemp += temp;
            episode.tempSamples++;
            episode.totalIntegrity += integrity;
            episode.integritySamples++;
            activeEpisodes[agentId] = episode;
        }

        public void LogEpisodeEnd(int agentId, int stepIndex, LifeStage finalStage)
        {
            if (!config.enableTelemetry || !config.logEpisodes || !activeEpisodes.ContainsKey(agentId)) return;

            var episode = activeEpisodes[agentId];
            float endTime = Time.time;
            float duration = endTime - episode.startTime;
            int survivalSteps = stepIndex - episode.startStep;

            float meanEnergy = episode.energySamples > 0 ? episode.totalEnergy / episode.energySamples : 0f;
            float meanTemp = episode.tempSamples > 0 ? episode.totalTemp / episode.tempSamples : 0f;
            float meanIntegrity = episode.integritySamples > 0 ? episode.totalIntegrity / episode.integritySamples : 0f;

            Genome finalGenome = episode.genomeAtEnd.Equals(default(Genome)) ? episode.genomeAtStart : episode.genomeAtEnd;

            string line = $"{runId},{episode.episodeIndex},{episode.agentId}," +
                         $"{episode.startTime:F3},{endTime:F3},{duration:F3},{survivalSteps}," +
                         $"{episode.foodEatenCount},{episode.totalNutrientsGained:F2}," +
                         $"{episode.scanCount},{episode.reproduceAttempts},{episode.reproduceSuccessCount}," +
                         $"{episode.damageEvents},{episode.totalDamage:F3}," +
                         $"{episode.stageMaxReached},{finalStage}," +
                         $"{meanEnergy:F2},{meanTemp:F2},{meanIntegrity:F2}," +
                         $"{episode.causeOfDeath},{finalGenome.GetHash()}," +
                         $"{finalGenome.hungerUrgencyBias:F3},{finalGenome.dangerAversionBias:F3}," +
                         $"{finalGenome.scanCuriosityBias:F3},{finalGenome.explorationNoiseBias:F3}," +
                         $"{finalGenome.reproductionConservatism:F3}\n";

            File.AppendAllText(Path.Combine(runPath, "telemetry", "episodes.csv"), line);
            activeEpisodes.Remove(agentId);
        }

        public void LogTrajectory(int agentId, int stepIndex, Vector3 position, Vector3 velocity, 
            float energy, float temp, float integrity, float age, float size, LifeStage stage, 
            float phiSense, bool scanFlag, bool reproduceFlag)
        {
            if (!config.enableTelemetry || !config.logTrajectories) return;
            if (stepIndex % config.trajectorySampleInterval != 0) return;

            string filePath = Path.Combine(runPath, "telemetry", "trajectories", $"agent_{agentId}.csv");
            bool fileExists = File.Exists(filePath);

            if (!fileExists)
            {
                string header = "time,step,agentId,x,z,vx,vz,energy,temp,integrity,age,size,stage,phiSense,scanFlag,reproduceFlag\n";
                File.WriteAllText(filePath, header);
            }

            string line = $"{Time.time:F3},{stepIndex},{agentId}," +
                         $"{position.x:F3},{position.z:F3}," +
                         $"{velocity.x:F3},{velocity.z:F3}," +
                         $"{energy:F2},{temp:F2},{integrity:F2}," +
                         $"{age:F2},{size:F2},{stage}," +
                         $"{phiSense:F3},{scanFlag},{reproduceFlag}\n";
            File.AppendAllText(filePath, line);
        }

        private void Update()
        {
            if (!config.enableTelemetry) return;

            // Periodic world snapshots
            if (config.logWorldSnapshots && Time.time - lastWorldSnapshot >= config.worldSnapshotInterval)
            {
                LogWorldSnapshot();
                lastWorldSnapshot = Time.time;
            }
        }

        private void LogWorldSnapshot()
        {
            // Phi field snapshot would need access to PhiField
            // For now, create placeholder
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string json = JsonUtility.ToJson(new { timestamp = timestamp, resources = new { } }, true);
            File.WriteAllText(Path.Combine(runPath, "world", $"resources_{timestamp}.json"), json);
        }

        private void OnApplicationQuit()
        {
            FlushEvents();
            if (config.generateAutoReport)
            {
                GenerateReport();
            }
        }

        private void FlushEvents()
        {
            if (!config.enableTelemetry || !config.logEvents) return;

            string eventsPath = Path.Combine(runPath, "telemetry", "events.jsonl");
            using (StreamWriter writer = new StreamWriter(eventsPath, true))
            {
                while (eventBuffer.Count > 0)
                {
                    writer.WriteLine(eventBuffer.Dequeue());
                }
            }
        }

        private string SerializeEvent(Dictionary<string, object> eventObj)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (var kvp in eventObj)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append($"\"{kvp.Key}\":");

                if (kvp.Value is string)
                    sb.Append($"\"{EscapeJsonString(kvp.Value.ToString())}\"");
                else if (kvp.Value is float || kvp.Value is double)
                    sb.Append(((float)kvp.Value).ToString("F6"));
                else if (kvp.Value is int)
                    sb.Append(kvp.Value);
                else if (kvp.Value is bool)
                    sb.Append(kvp.Value.ToString().ToLower());
                else if (kvp.Value is Genome)
                {
                    // Serialize genome as nested object
                    Genome g = (Genome)kvp.Value;
                    sb.Append("{");
                    sb.Append($"\"hungerUrgencyBias\":{g.hungerUrgencyBias:F6},");
                    sb.Append($"\"dangerAversionBias\":{g.dangerAversionBias:F6},");
                    sb.Append($"\"scanCuriosityBias\":{g.scanCuriosityBias:F6},");
                    sb.Append($"\"explorationNoiseBias\":{g.explorationNoiseBias:F6},");
                    sb.Append($"\"reproductionConservatism\":{g.reproductionConservatism:F6},");
                    sb.Append($"\"growthRate\":{g.growthRate:F6},");
                    sb.Append($"\"maturityAgeNorm\":{g.maturityAgeNorm:F6},");
                    sb.Append($"\"maxAgeNorm\":{g.maxAgeNorm:F6},");
                    sb.Append($"\"baseEmax\":{g.baseEmax:F6},");
                    sb.Append($"\"basalMetabolism\":{g.basalMetabolism:F6},");
                    sb.Append($"\"moveEfficiency\":{g.moveEfficiency:F6},");
                    sb.Append($"\"tempTolerance\":{g.tempTolerance:F6},");
                    sb.Append($"\"antennaGain\":{g.antennaGain:F6},");
                    sb.Append($"\"scanCost\":{g.scanCost:F6},");
                    sb.Append($"\"scanRadius\":{g.scanRadius:F6},");
                    sb.Append($"\"phiAffinity\":{g.phiAffinity:F6}");
                    sb.Append("}");
                }
                else if (kvp.Value is LifeStage)
                    sb.Append($"\"{kvp.Value}\"");
                else
                    sb.Append($"\"{EscapeJsonString(kvp.Value.ToString())}\"");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string EscapeJsonString(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private void GenerateReport()
        {
            string reportPath = Path.Combine(runPath, "report.md");
            var report = new StringBuilder();
            report.AppendLine($"# Run Report: {runId}");
            report.AppendLine($"Generated: {DateTime.Now}");
            report.AppendLine();
            report.AppendLine("## Summary");
            report.AppendLine("(Report generation would analyze episodes.csv)");
            report.AppendLine();
            report.AppendLine("## Files");
            report.AppendLine($"- Episodes: telemetry/episodes.csv");
            report.AppendLine($"- Events: telemetry/events.jsonl");
            report.AppendLine($"- Trajectories: telemetry/trajectories/");
            report.AppendLine($"- World: world/");
            report.AppendLine($"- Replay: replay/");

            File.WriteAllText(reportPath, report.ToString());
        }

        public int GetNextBirthIndex()
        {
            return birthCounter++;
        }

        public System.Random GetRunRNG()
        {
            return runRNG;
        }
    }
}

