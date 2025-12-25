using UnityEngine;
using System.Collections.Generic;
using SyntheticLife.Phi.Config;

namespace SyntheticLife.Phi.World
{
    public class FoodSpawner : MonoBehaviour
    {
        [SerializeField] private SpawnerConfig config;
        [SerializeField] private PhiField phiField;

        private List<FoodItem> activeFood = new List<FoodItem>();
        private float nextSpawnTime = 0f;

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("FoodSpawner: No SpawnerConfig assigned!");
                return;
            }

            nextSpawnTime = Time.time + config.spawnInterval;
        }

        private void Update()
        {
            // Remove destroyed food items
            activeFood.RemoveAll(item => item == null);

            // Spawn new food if under limit
            if (activeFood.Count < config.maxFoodItems && Time.time >= nextSpawnTime)
            {
                SpawnFood();
                nextSpawnTime = Time.time + config.spawnInterval;
            }
        }

        private void SpawnFood()
        {
            if (config.foodPrefab == null) return;

            // Random position within arena bounds
            Vector3 spawnPos = new Vector3(
                Random.Range(-config.arenaSize / 2f, config.arenaSize / 2f),
                0.5f,
                Random.Range(-config.arenaSize / 2f, config.arenaSize / 2f)
            );

            // Check if position is valid (not too close to other food)
            bool valid = true;
            foreach (var existingFood in activeFood)
            {
                if (existingFood != null && Vector3.Distance(spawnPos, existingFood.transform.position) < config.minSpawnDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid) return;

            GameObject foodObj = Instantiate(config.foodPrefab, spawnPos, Quaternion.identity);
            FoodItem newFood = foodObj.GetComponent<FoodItem>();
            if (newFood != null)
            {
                float phi = phiField != null ? phiField.SamplePhi(spawnPos) : 0f;
                float nutrientValue = config.baseNutrientValue * (1f + phi * config.phiNutrientMultiplier);
                newFood.Initialize(nutrientValue, phi);
                activeFood.Add(newFood);
            }
        }

        public void RemoveFood(FoodItem food)
        {
            activeFood.Remove(food);
        }
    }
}

