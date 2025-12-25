#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SyntheticLife.Phi.Config;
using SyntheticLife.Phi.Core;

namespace SyntheticLife.Phi.Editor
{
    public class CreateDefaultConfigs : EditorWindow
    {
        [MenuItem("SyntheticLife/Create Default Configs")]
        static void Init()
        {
            CreateConfigs();
        }

        static void CreateConfigs()
        {
            // Create folder if needed
            string configPath = "Assets/ScriptableObjects/Configs";
            if (!AssetDatabase.IsValidFolder(configPath))
            {
                string parentPath = "Assets/ScriptableObjects";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
                }
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Configs");
            }

            // CreatureDefaults
            CreatureDefaults creatureDefaults = ScriptableObject.CreateInstance<CreatureDefaults>();
            creatureDefaults.initialEnergy = 100f;
            creatureDefaults.initialTemperature = 0.5f;
            creatureDefaults.initialIntegrity = 1f;
            creatureDefaults.initialAge = 0f;
            creatureDefaults.defaultGenome = new Genome
            {
                baseEmax = 100f,
                basalMetabolism = 1f,
                moveEfficiency = 1f,
                tempTolerance = 0.5f,
                antennaGain = 1f,
                scanCost = 2f,
                scanRadius = 5f,
                phiAffinity = 0f
            };
            creatureDefaults.juvenileAge = 10f;
            creatureDefaults.adultAge = 30f;
            creatureDefaults.elderAge = 60f;
            creatureDefaults.sizeGrowthRate = 0.01f;
            creatureDefaults.sizeFromAgeMax = 2f;
            creatureDefaults.nutritionThreshold = 0.7f;
            creatureDefaults.stressThreshold = 0.3f;
            creatureDefaults.reproductionEnergyThreshold = 0.7f;
            creatureDefaults.reproductionIntegrityThreshold = 0.6f;
            creatureDefaults.reproductionCooldown = 15f;
            creatureDefaults.reproductionEnergyCost = 50f;
            creatureDefaults.reproductionVulnerabilityDuration = 5f;
            creatureDefaults.deathEnergyThreshold = 0f;
            creatureDefaults.deathIntegrityThreshold = 0f;
            creatureDefaults.mutationRate = 0.3f;
            creatureDefaults.mutationStrength = 0.1f;
            AssetDatabase.CreateAsset(creatureDefaults, $"{configPath}/CreatureDefaults.asset");

            // PhiFieldConfig
            PhiFieldConfig phiConfig = ScriptableObject.CreateInstance<PhiFieldConfig>();
            phiConfig.scale = 0.1f;
            phiConfig.speedX = 0.01f;
            phiConfig.speedY = 0.01f;
            phiConfig.amplitude = 1f;
            phiConfig.seed = 42;
            phiConfig.showVisualization = false;
            phiConfig.enableBackAction = false;
            phiConfig.backActionRadius = 5f;
            phiConfig.backActionDuration = 2f;
            phiConfig.backActionDamping = 0.5f;
            AssetDatabase.CreateAsset(phiConfig, $"{configPath}/PhiFieldConfig.asset");

            // SpawnerConfig
            SpawnerConfig spawnerConfig = ScriptableObject.CreateInstance<SpawnerConfig>();
            spawnerConfig.maxFoodItems = 50;
            spawnerConfig.spawnInterval = 2f;
            spawnerConfig.minSpawnDistance = 5f;
            spawnerConfig.maxSpawnDistance = 30f;
            spawnerConfig.baseNutrientValue = 10f;
            spawnerConfig.phiNutrientMultiplier = 0.5f;
            spawnerConfig.arenaSize = 50f;
            AssetDatabase.CreateAsset(spawnerConfig, $"{configPath}/SpawnerConfig.asset");

            // RewardConfig
            RewardConfig rewardConfig = ScriptableObject.CreateInstance<RewardConfig>();
            rewardConfig.livingReward = 0.001f;
            rewardConfig.minEnergyForLivingReward = 0.2f;
            rewardConfig.minIntegrityForLivingReward = 0.3f;
            rewardConfig.starvationPenalty = -0.01f;
            rewardConfig.damagePenalty = -0.05f;
            rewardConfig.temperatureStressPenalty = -0.02f;
            rewardConfig.deathPenalty = -10f;
            rewardConfig.foodRewardBase = 0.1f;
            rewardConfig.foodRewardDiminishingFactor = 0.8f;
            rewardConfig.reproductionReward = 0.5f;
            rewardConfig.offspringSurvivalReward = 2f;
            rewardConfig.offspringSurvivalTimeThreshold = 10f;
            rewardConfig.parentSurvivalReward = 1f;
            rewardConfig.parentSurvivalTimeThreshold = 15f;
            rewardConfig.enableExplorationReward = true;
            rewardConfig.explorationReward = 0.001f;
            rewardConfig.explorationGridSize = 2f;
            AssetDatabase.CreateAsset(rewardConfig, $"{configPath}/RewardConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Default configs created at Assets/ScriptableObjects/Configs/");
        }
    }
}
#endif

