#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using SyntheticLife.Phi.Config;
using SyntheticLife.Phi.Core;
using SyntheticLife.Phi.World;
using SyntheticLife.Phi.Utils;

namespace SyntheticLife.Phi.Editor
{
    public class SceneSetupHelper : EditorWindow
    {
        [MenuItem("SyntheticLife/Setup Training Scene")]
        static void Init()
        {
            SetupTrainingScene();
        }

        static void SetupTrainingScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena";
            ground.transform.localScale = new Vector3(5, 1, 5);
            ground.transform.position = Vector3.zero;

            // Create folder structure
            GameObject sceneRoot = new GameObject("Environment");
            GameObject configsRoot = new GameObject("Configs");
            GameObject zonesRoot = new GameObject("Zones");

            // Load or create configs
            CreatureDefaults creatureConfig = AssetDatabase.LoadAssetAtPath<CreatureDefaults>("Assets/ScriptableObjects/Configs/CreatureDefaults.asset");
            PhiFieldConfig phiConfig = AssetDatabase.LoadAssetAtPath<PhiFieldConfig>("Assets/ScriptableObjects/Configs/PhiFieldConfig.asset");
            SpawnerConfig spawnerConfig = AssetDatabase.LoadAssetAtPath<SpawnerConfig>("Assets/ScriptableObjects/Configs/SpawnerConfig.asset");
            RewardConfig rewardConfig = AssetDatabase.LoadAssetAtPath<RewardConfig>("Assets/ScriptableObjects/Configs/RewardConfig.asset");
            TelemetryConfig telemetryConfig = AssetDatabase.LoadAssetAtPath<TelemetryConfig>("Assets/ScriptableObjects/Configs/TelemetryConfig.asset");

            if (creatureConfig == null || phiConfig == null || spawnerConfig == null || rewardConfig == null || telemetryConfig == null)
            {
                Debug.LogWarning("Configs not found! Creating them now...");
                CreateDefaultConfigs.Init();
                creatureConfig = AssetDatabase.LoadAssetAtPath<CreatureDefaults>("Assets/ScriptableObjects/Configs/CreatureDefaults.asset");
                phiConfig = AssetDatabase.LoadAssetAtPath<PhiFieldConfig>("Assets/ScriptableObjects/Configs/PhiFieldConfig.asset");
                spawnerConfig = AssetDatabase.LoadAssetAtPath<SpawnerConfig>("Assets/ScriptableObjects/Configs/SpawnerConfig.asset");
                rewardConfig = AssetDatabase.LoadAssetAtPath<RewardConfig>("Assets/ScriptableObjects/Configs/RewardConfig.asset");
                telemetryConfig = AssetDatabase.LoadAssetAtPath<TelemetryConfig>("Assets/ScriptableObjects/Configs/TelemetryConfig.asset");
            }

            // PhiField
            GameObject phiFieldObj = new GameObject("PhiField");
            phiFieldObj.transform.parent = sceneRoot.transform;
            PhiField phiField = phiFieldObj.AddComponent<PhiField>();
            SerializedObject phiFieldSO = new SerializedObject(phiField);
            phiFieldSO.FindProperty("config").objectReferenceValue = phiConfig;
            phiFieldSO.ApplyModifiedProperties();

            // Food Spawner
            GameObject foodSpawnerObj = new GameObject("FoodSpawner");
            foodSpawnerObj.transform.parent = sceneRoot.transform;
            FoodSpawner foodSpawner = foodSpawnerObj.AddComponent<FoodSpawner>();
            SerializedObject foodSpawnerSO = new SerializedObject(foodSpawner);
            foodSpawnerSO.FindProperty("config").objectReferenceValue = spawnerConfig;
            foodSpawnerSO.FindProperty("phiField").objectReferenceValue = phiField;
            foodSpawnerSO.ApplyModifiedProperties();

            // Create food prefab if needed
            GameObject foodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Food.prefab");
            if (foodPrefab == null)
            {
                GameObject food = GameObject.CreatePrimitive(PrimitiveType.Cube);
                food.name = "Food";
                food.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                food.AddComponent<FoodItem>();
                food.tag = "Food";
                
                // Create Prefabs folder
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }
                
                string prefabPath = "Assets/Prefabs/Food.prefab";
                foodPrefab = PrefabUtility.SaveAsPrefabAsset(food, prefabPath);
                DestroyImmediate(food);
            }
            foodSpawnerSO.FindProperty("config").objectReferenceValue = spawnerConfig;
            SerializedObject spawnerConfigSO = new SerializedObject(spawnerConfig);
            spawnerConfigSO.FindProperty("foodPrefab").objectReferenceValue = foodPrefab;
            spawnerConfigSO.ApplyModifiedProperties();

            // Temperature Zones
            GameObject hotZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hotZone.name = "HotZone";
            hotZone.transform.position = new Vector3(15, 0.5f, 0);
            hotZone.transform.localScale = new Vector3(10, 1, 10);
            hotZone.transform.parent = zonesRoot.transform;
            hotZone.tag = "TemperatureZone";
            TemperatureZone hotZoneComp = hotZone.AddComponent<TemperatureZone>();
            hotZoneComp.temperatureValue = 0.8f;
            var hotRenderer = hotZone.GetComponent<Renderer>();
            hotRenderer.material.color = new Color(1f, 0.3f, 0.3f, 0.5f);

            GameObject coldZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coldZone.name = "ColdZone";
            coldZone.transform.position = new Vector3(-15, 0.5f, 0);
            coldZone.transform.localScale = new Vector3(10, 1, 10);
            coldZone.transform.parent = zonesRoot.transform;
            coldZone.tag = "TemperatureZone";
            TemperatureZone coldZoneComp = coldZone.AddComponent<TemperatureZone>();
            coldZoneComp.temperatureValue = 0.2f;
            var coldRenderer = coldZone.GetComponent<Renderer>();
            coldRenderer.material.color = new Color(0.3f, 0.3f, 1f, 0.5f);

            // Hazard
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = "Hazard1";
            hazard.transform.position = new Vector3(10, 0.5f, 10);
            hazard.transform.localScale = new Vector3(5, 1, 5);
            hazard.transform.parent = zonesRoot.transform;
            hazard.tag = "Hazard";
            hazard.AddComponent<HazardZone>();
            var hazardRenderer = hazard.GetComponent<Renderer>();
            hazardRenderer.material.color = new Color(1f, 0f, 0f, 0.5f);

            // Shelter
            GameObject shelter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelter.name = "Shelter";
            shelter.transform.position = new Vector3(0, 1, 0);
            shelter.transform.localScale = new Vector3(8, 2, 8);
            shelter.transform.parent = zonesRoot.transform;
            shelter.tag = "Shelter";
            shelter.AddComponent<ShelterZone>();
            var shelterRenderer = shelter.GetComponent<Renderer>();
            shelterRenderer.material.color = new Color(0f, 1f, 0f, 0.5f);

            // Telemetry Manager
            GameObject telemetryManagerObj = new GameObject("TelemetryManager");
            TelemetryManager telemetryManager = telemetryManagerObj.AddComponent<TelemetryManager>();
            SerializedObject telemetryManagerSO = new SerializedObject(telemetryManager);
            telemetryManagerSO.FindProperty("config").objectReferenceValue = telemetryConfig;
            telemetryManagerSO.ApplyModifiedProperties();

            // Population Manager
            GameObject popManager = new GameObject("PopulationManager");
            PopulationManager pm = popManager.AddComponent<PopulationManager>();
            pm.maxCreatures = 20;
            SerializedObject pmSO = new SerializedObject(pm);
            pmSO.FindProperty("creatureDefaults").objectReferenceValue = creatureConfig;
            pmSO.ApplyModifiedProperties();

            // Create creature prefab
            GameObject creaturePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab");
            if (creaturePrefab == null)
            {
                creaturePrefab = CreateCreaturePrefab(creatureConfig, rewardConfig, phiConfig, phiField, foodSpawner, pm, telemetryManager);
            }
            
            // Assign prefab to PopulationManager
            if (creaturePrefab != null)
            {
                SerializedObject pmPrefabSO = new SerializedObject(pm);
                pmPrefabSO.FindProperty("creaturePrefab").objectReferenceValue = creaturePrefab;
                pmPrefabSO.ApplyModifiedProperties();
            }

            // Add initial creature
            if (creaturePrefab != null)
            {
                GameObject creature = PrefabUtility.InstantiatePrefab(creaturePrefab) as GameObject;
                creature.transform.position = new Vector3(0, 1, 0);
            }

            // Save scene
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/TrainingArena.unity");

            Debug.Log("Scene setup complete! All components configured including Creature prefab.");
            EditorUtility.DisplayDialog("Scene Setup", 
                "Training scene created!\n\n" +
                "✅ TelemetryManager added and configured\n" +
                "✅ PopulationManager configured with CreatureDefaults\n" +
                "✅ Creature prefab created and configured\n\n" +
                "The scene is ready for training!\n" +
                "You can start ML-Agents training now.", "OK");
        }

        static GameObject CreateCreaturePrefab(CreatureDefaults creatureConfig, RewardConfig rewardConfig, 
            PhiFieldConfig phiConfig, PhiField phiField, FoodSpawner foodSpawner, PopulationManager popManager, 
            TelemetryManager telemetryManager)
        {
            // Create Prefabs folder if needed
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create Capsule GameObject
            GameObject creature = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            creature.name = "Creature";
            creature.transform.position = Vector3.zero;
            creature.tag = "Creature";

            // Setup Rigidbody
            Rigidbody rb = creature.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.drag = 5f;

            // Add GrowthSystem
            GrowthSystem growthSystem = creature.AddComponent<GrowthSystem>();

            // Add CreatureAgent
            CreatureAgent agent = creature.AddComponent<CreatureAgent>();
            SerializedObject agentSO = new SerializedObject(agent);
            
            // Assign references
            agentSO.FindProperty("growthSystem").objectReferenceValue = growthSystem;
            agentSO.FindProperty("rb").objectReferenceValue = rb;
            agentSO.FindProperty("populationManager").objectReferenceValue = popManager;
            agentSO.FindProperty("creatureConfig").objectReferenceValue = creatureConfig;
            agentSO.FindProperty("rewardConfig").objectReferenceValue = rewardConfig;
            agentSO.FindProperty("phiConfig").objectReferenceValue = phiConfig;
            agentSO.FindProperty("phiField").objectReferenceValue = phiField;
            agentSO.FindProperty("foodSpawner").objectReferenceValue = foodSpawner;
            agentSO.FindProperty("moveSpeed").floatValue = 5f;
            agentSO.FindProperty("turnSpeed").floatValue = 180f;
            agentSO.ApplyModifiedProperties();

            // Add Behavior Parameters (ML-Agents)
            var behaviorParams = creature.AddComponent<BehaviorParameters>();
            
            // Set behavior name via SerializedObject (safer than direct property access)
            SerializedObject bpSO = new SerializedObject(behaviorParams);
            var behaviorNameProp = bpSO.FindProperty("m_BehaviorName");
            if (behaviorNameProp != null)
            {
                behaviorNameProp.stringValue = "Creature";
                bpSO.ApplyModifiedProperties();
            }
            
            // Note: Brain parameters (observation size, action spec) are complex nested structures
            // that are better configured via Unity Inspector. The BehaviorParameters component
            // will use default values that can be adjusted manually in the Inspector.
            // The critical part (behavior name) is set above.

            // Add Ray Perception Sensor 3D
            var raySensor = creature.AddComponent<RayPerceptionSensorComponent3D>();
            SerializedObject raySO = new SerializedObject(raySensor);
            raySO.FindProperty("m_SensorName").stringValue = "RayPerceptionSensor3D";
            raySO.FindProperty("m_RaysPerDirection").intValue = 12;
            raySO.FindProperty("m_MaxRayDegrees").floatValue = 90f;
            raySO.FindProperty("m_SphereCastRadius").floatValue = 0.5f;
            raySO.FindProperty("m_RayLength").floatValue = 10f;
            raySO.FindProperty("m_RayLayerMask").intValue = -1; // All layers
            raySO.FindProperty("m_DetectableTags").arraySize = 5;
            raySO.FindProperty("m_DetectableTags").GetArrayElementAtIndex(0).stringValue = "Food";
            raySO.FindProperty("m_DetectableTags").GetArrayElementAtIndex(1).stringValue = "Hazard";
            raySO.FindProperty("m_DetectableTags").GetArrayElementAtIndex(2).stringValue = "Shelter";
            raySO.FindProperty("m_DetectableTags").GetArrayElementAtIndex(3).stringValue = "Creature";
            raySO.FindProperty("m_DetectableTags").GetArrayElementAtIndex(4).stringValue = "TemperatureZone";
            raySO.ApplyModifiedProperties();

            // Assign ray sensor to agent
            var rayProp = agentSO.FindProperty("rayPerception");
            if (rayProp != null)
            {
                rayProp.objectReferenceValue = raySensor;
                agentSO.ApplyModifiedProperties();
            }

            // Save as prefab first
            string prefabPath = "Assets/Prefabs/Creature.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(creature, prefabPath);
            
            // Now set offspring prefab reference on the prefab asset
            var prefabAgent = prefab.GetComponent<CreatureAgent>();
            if (prefabAgent != null)
            {
                SerializedObject prefabAgentSO = new SerializedObject(prefabAgent);
                var offspringProp = prefabAgentSO.FindProperty("offspringPrefab");
                if (offspringProp != null)
                {
                    offspringProp.objectReferenceValue = prefab;
                    prefabAgentSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                }
            }
            
            // Clean up scene instance
            DestroyImmediate(creature);

            Debug.Log($"Creature prefab created at {prefabPath}");
            return prefab;
        }
    }
}
#endif

