using UnityEngine;

namespace SyntheticLife.Phi.Config
{
    [CreateAssetMenu(fileName = "SpawnerConfig", menuName = "SyntheticLife/Config/SpawnerConfig")]
    public class SpawnerConfig : ScriptableObject
    {
        [Header("Food Spawning")]
        public int maxFoodItems = 50;
        public float spawnInterval = 2f;
        public float minSpawnDistance = 5f;
        public float maxSpawnDistance = 30f;
        public float baseNutrientValue = 10f;
        public float phiNutrientMultiplier = 0.5f;
        public GameObject foodPrefab;

        [Header("Arena Bounds")]
        public float arenaSize = 50f;
    }
}

