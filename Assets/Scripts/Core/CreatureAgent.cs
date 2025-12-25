using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Linq;
using SyntheticLife.Phi.Core;
using SyntheticLife.Phi.World;
using SyntheticLife.Phi.Config;

namespace SyntheticLife.Phi.Core
{
    public class CreatureAgent : Agent
    {
        [Header("Components")]
        public GrowthSystem growthSystem;
        public Rigidbody rb;
        public PopulationManager populationManager;

        [Header("Configs")]
        public CreatureDefaults creatureConfig;
        public RewardConfig rewardConfig;
        public PhiFieldConfig phiConfig;

        [Header("References")]
        public PhiField phiField;
        public FoodSpawner foodSpawner;
        private List<TemperatureZone> temperatureZones = new List<TemperatureZone>();
        private List<ShelterZone> shelterZones = new List<ShelterZone>();

        [Header("State")]
        public float energy = 100f;
        public float temperature = 0.5f;
        public float integrity = 1f;
        public Genome genome;
        public LifeStage currentStage;

        [Header("Reproduction")]
        public float reproductionCooldown = 0f;
        public GameObject offspringPrefab;
        public int offspringCount = 0;
        private List<CreatureAgent> myOffspring = new List<CreatureAgent>();

        [Header("Movement")]
        public float moveSpeed = 5f;
        public float turnSpeed = 180f;

        [Header("Sensing")]
        public RayPerceptionSensorComponent3D rayPerception;
        public float antennaGain = 1f;
        private float lastPhiSense = 0f;
        private bool scannedLastStep = false;

        [Header("Metrics")]
        private float totalFoodEaten = 0f;
        private int scanCount = 0;
        private float episodeStartTime = 0f;
        private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
        private bool isInShelter = false;
        private bool isVulnerable = false;
        private float vulnerabilityEndTime = 0f;

        private float targetTemperature = 0.5f;
        private Vector3 startPosition;

        public override void Initialize()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (growthSystem == null) growthSystem = GetComponent<GrowthSystem>();

            // Find all zones
            temperatureZones = FindObjectsOfType<TemperatureZone>().ToList();
            shelterZones = FindObjectsOfType<ShelterZone>().ToList();

            if (phiField == null) phiField = FindObjectOfType<PhiField>();
            if (foodSpawner == null) foodSpawner = FindObjectOfType<FoodSpawner>();
            if (populationManager == null) populationManager = FindObjectOfType<PopulationManager>();

            // Initialize genome if not set
            if (genome.baseEmax == 0)
            {
                genome = creatureConfig.defaultGenome;
            }

            // Initialize growth system
            if (growthSystem != null && creatureConfig != null)
            {
                growthSystem.Initialize(creatureConfig, genome);
            }

            // Set initial energy
            if (creatureConfig != null)
            {
                energy = creatureConfig.initialEnergy;
                temperature = creatureConfig.initialTemperature;
                integrity = creatureConfig.initialIntegrity;
            }

            startPosition = transform.position;
            targetTemperature = genome.tempTolerance;
        }

        public override void OnEpisodeBegin()
        {
            // Reset state
            energy = creatureConfig != null ? creatureConfig.initialEnergy : 100f;
            temperature = creatureConfig != null ? creatureConfig.initialTemperature : 0.5f;
            integrity = creatureConfig != null ? creatureConfig.initialIntegrity : 1f;
            reproductionCooldown = 0f;
            vulnerabilityEndTime = 0f;
            isVulnerable = false;

            // Reset position
            transform.position = startPosition + new Vector3(
                Random.Range(-5f, 5f),
                0f,
                Random.Range(-5f, 5f)
            );
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Reset growth if needed (for new creatures)
            if (growthSystem != null && creatureConfig != null)
            {
                growthSystem.Initialize(creatureConfig, genome);
            }

            // Reset metrics
            totalFoodEaten = 0f;
            scanCount = 0;
            episodeStartTime = Time.time;
            visitedCells.Clear();
            myOffspring.Clear();

            // Register with population manager
            if (populationManager != null)
            {
                populationManager.RegisterCreature(this);
            }

            // Clean up dead offspring
            myOffspring.RemoveAll(o => o == null);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (creatureConfig == null || growthSystem == null) return;

            // Normalized energy (0-1)
            float maxEnergy = growthSystem.maxEnergy > 0 ? growthSystem.maxEnergy : 100f;
            sensor.AddObservation(Mathf.Clamp01(energy / maxEnergy));

            // Normalized temperature (0-1)
            sensor.AddObservation(temperature);

            // Normalized integrity (0-1)
            sensor.AddObservation(integrity);

            // Age normalized (0-1)
            float maxAge = creatureConfig.elderAge;
            sensor.AddObservation(Mathf.Clamp01(growthSystem.age / maxAge));

            // Size normalized (0-1)
            sensor.AddObservation(Mathf.Clamp01((growthSystem.size - 1f) / creatureConfig.sizeFromAgeMax));

            // Life stage one-hot (3 values)
            sensor.AddObservation(growthSystem.stage == LifeStage.Juvenile ? 1f : 0f);
            sensor.AddObservation(growthSystem.stage == LifeStage.Adult ? 1f : 0f);
            sensor.AddObservation(growthSystem.stage == LifeStage.Elder ? 1f : 0f);

            // Phi sense (noisy)
            float phiNoise = scannedLastStep ? 0.05f : 0.2f;
            phiNoise /= (antennaGain * growthSystem.size);
            lastPhiSense = phiField != null ? phiField.SamplePhiNoisy(transform.position, phiNoise) : 0f;
            sensor.AddObservation(lastPhiSense);

            // Phi gradient (noisy)
            if (phiField != null && scannedLastStep)
            {
                Vector2 grad = phiField.SamplePhiGradient(transform.position);
                sensor.AddObservation(grad.x);
                sensor.AddObservation(grad.y);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            // Velocity / heading
            if (rb != null)
            {
                Vector3 vel = rb.velocity;
                sensor.AddObservation(vel.x / moveSpeed);
                sensor.AddObservation(vel.z / moveSpeed);
                sensor.AddObservation(transform.forward.x);
                sensor.AddObservation(transform.forward.z);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            // Reproduction cooldown normalized
            sensor.AddObservation(Mathf.Clamp01(reproductionCooldown / creatureConfig.reproductionCooldown));

            // In shelter
            sensor.AddObservation(isInShelter ? 1f : 0f);

            // Ray perception is handled automatically by RayPerceptionSensorComponent3D
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (creatureConfig == null || growthSystem == null) return;

            float deltaTime = Time.fixedDeltaTime;

            // Movement actions
            float moveForward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float turn = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

            // Discrete actions
            bool scanAction = actions.DiscreteActions[0] == 1;
            bool reproduceAction = actions.DiscreteActions[1] == 1;

            // Apply movement
            if (rb != null)
            {
                Vector3 moveDir = transform.forward * moveForward * moveSpeed * deltaTime;
                rb.MovePosition(transform.position + moveDir);
                transform.Rotate(0, turn * turnSpeed * deltaTime, 0);

                // Energy cost for movement
                float moveCost = Mathf.Abs(moveForward) * growthSystem.locomotionCostMultiplier * deltaTime;
                energy -= moveCost;
            }

            // Scan action
            scannedLastStep = false;
            if (scanAction && energy > growthSystem.scanCostMultiplier)
            {
                energy -= growthSystem.scanCostMultiplier;
                scanCount++;
                scannedLastStep = true;

                // Trigger back action if enabled
                if (phiField != null && phiConfig != null && phiConfig.enableBackAction)
                {
                    phiField.TriggerBackAction(transform.position);
                }
            }

            // Reproduction action
            if (reproduceAction && CanReproduce())
            {
                Reproduce();
            }

            // Update homeostasis
            UpdateHomeostasis(deltaTime);

            // Update growth
            float ambientTemp = GetAmbientTemperature();
            growthSystem.UpdateGrowth(deltaTime, energy, integrity, ambientTemp, targetTemperature);

            // Update cooldowns
            if (reproductionCooldown > 0f)
            {
                reproductionCooldown -= deltaTime;
            }

            if (isVulnerable && Time.time > vulnerabilityEndTime)
            {
                isVulnerable = false;
            }

            // Check for death
            if (IsDead())
            {
                AddReward(rewardConfig != null ? rewardConfig.deathPenalty : -10f);
                EndEpisode();
                return;
            }

            // Apply rewards
            UpdateRewards(deltaTime);

            // Track visited cells for exploration reward
            if (rewardConfig != null && rewardConfig.enableExplorationReward)
            {
                Vector2Int cell = new Vector2Int(
                    Mathf.FloorToInt(transform.position.x / rewardConfig.explorationGridSize),
                    Mathf.FloorToInt(transform.position.z / rewardConfig.explorationGridSize)
                );
                if (!visitedCells.Contains(cell))
                {
                    visitedCells.Add(cell);
                    AddReward(rewardConfig.explorationReward);
                }
            }
        }

        private void UpdateHomeostasis(float deltaTime)
        {
            // Update temperature
            float ambientTemp = GetAmbientTemperature();
            float tempDrift = (ambientTemp - temperature) * 0.1f;
            temperature = Mathf.Clamp(temperature + tempDrift * deltaTime, 0f, 1f);

            // Temperature stress
            float tempStress = Mathf.Abs(temperature - targetTemperature);
            if (tempStress > 0.3f)
            {
                float stressDamage = (tempStress - 0.3f) * 0.1f * deltaTime;
                integrity -= stressDamage;
            }

            // Update energy (basal metabolism)
            float basalRate = growthSystem.basalMetabolismRate;
            if (isInShelter)
            {
                basalRate *= 0.5f; // Reduced metabolism in shelter
            }
            energy -= basalRate * deltaTime;

            // Update integrity (healing)
            if (integrity < 1f && energy > growthSystem.maxEnergy * 0.3f)
            {
                float healingRate = 0.05f * deltaTime;
                if (isInShelter)
                {
                    healingRate *= 2f; // Faster healing in shelter
                }
                integrity = Mathf.Min(1f, integrity + healingRate);
            }

            // Clamp values
            energy = Mathf.Clamp(energy, 0f, growthSystem.maxEnergy);
            integrity = Mathf.Clamp01(integrity);
        }

        private float GetAmbientTemperature()
        {
            float ambient = 0.5f; // Default neutral

            foreach (var zone in temperatureZones)
            {
                float zoneTemp = zone.GetTemperatureAt(transform.position);
                // Take strongest influence
                if (Mathf.Abs(zoneTemp - 0.5f) > Mathf.Abs(ambient - 0.5f))
                {
                    ambient = zoneTemp;
                }
            }

            return ambient;
        }

        private void UpdateRewards(float deltaTime)
        {
            if (rewardConfig == null || growthSystem == null) return;

            float maxEnergy = growthSystem.maxEnergy > 0 ? growthSystem.maxEnergy : 100f;

            // Living reward (only if healthy)
            if (energy / maxEnergy > rewardConfig.minEnergyForLivingReward &&
                integrity > rewardConfig.minIntegrityForLivingReward)
            {
                AddReward(rewardConfig.livingReward);
            }

            // Starvation penalty
            if (energy / maxEnergy < 0.2f)
            {
                AddReward(rewardConfig.starvationPenalty * deltaTime);
            }

            // Temperature stress penalty
            float tempStress = Mathf.Abs(temperature - targetTemperature);
            if (tempStress > 0.3f)
            {
                AddReward(rewardConfig.temperatureStressPenalty * deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Food consumption
            FoodItem food = other.GetComponent<FoodItem>();
            if (food != null)
            {
                float nutrientValue = food.nutrientValue;
                float phiMultiplier = 1f + phiField.SamplePhi(transform.position) * genome.phiAffinity;
                nutrientValue *= phiMultiplier;

                energy = Mathf.Min(growthSystem.maxEnergy, energy + nutrientValue);
                totalFoodEaten += nutrientValue;

                // Diminishing returns reward
                float reward = rewardConfig != null ? rewardConfig.foodRewardBase : 0.1f;
                reward *= Mathf.Pow(rewardConfig != null ? rewardConfig.foodRewardDiminishingFactor : 0.8f, totalFoodEaten / 10f);
                AddReward(reward);

                // Remove food
                Destroy(food.gameObject);
                if (foodSpawner != null)
                {
                    foodSpawner.RemoveFood(food);
                }
            }

            // Shelter detection
            ShelterZone shelter = other.GetComponent<ShelterZone>();
            if (shelter != null)
            {
                isInShelter = true;
            }

            // Hazard damage
            HazardZone hazard = other.GetComponent<HazardZone>();
            if (hazard != null)
            {
                integrity -= hazard.GetDamageRate() * Time.fixedDeltaTime;
                AddReward(rewardConfig != null ? rewardConfig.damagePenalty * Time.fixedDeltaTime : -0.05f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ShelterZone shelter = other.GetComponent<ShelterZone>();
            if (shelter != null)
            {
                isInShelter = false;
            }
        }

        private bool CanReproduce()
        {
            if (creatureConfig == null || growthSystem == null) return false;

            return growthSystem.CanReproduce() &&
                   energy >= growthSystem.maxEnergy * creatureConfig.reproductionEnergyThreshold &&
                   integrity >= creatureConfig.reproductionIntegrityThreshold &&
                   reproductionCooldown <= 0f &&
                   !isVulnerable &&
                   (populationManager == null || populationManager.CanSpawn());
        }

        private void Reproduce()
        {
            if (creatureConfig == null || offspringPrefab == null) return;
            if (populationManager != null && !populationManager.CanSpawn()) return;

            // Energy cost
            energy -= creatureConfig.reproductionEnergyCost;

            // Cooldown
            reproductionCooldown = creatureConfig.reproductionCooldown;

            // Vulnerability period
            isVulnerable = true;
            vulnerabilityEndTime = Time.time + creatureConfig.reproductionVulnerabilityDuration;

            // Spawn offspring
            Vector3 spawnPos = transform.position + transform.right * 2f;
            GameObject offspringObj = Instantiate(offspringPrefab, spawnPos, Quaternion.identity);
            CreatureAgent offspring = offspringObj.GetComponent<CreatureAgent>();

            if (offspring != null)
            {
                // Mutate genome
                Genome childGenome = Genome.Mutate(
                    genome,
                    creatureConfig.mutationRate,
                    creatureConfig.mutationStrength
                );

                offspring.genome = childGenome;
                offspring.growthSystem.SetGenome(childGenome);
                offspring.antennaGain = childGenome.antennaGain;

                myOffspring.Add(offspring);
                offspringCount++;

                // Register with population manager
                if (populationManager != null)
                {
                    populationManager.RegisterCreature(offspring);
                }
            }

            // Small immediate reward (but main reward comes from survival)
            if (rewardConfig != null)
            {
                AddReward(rewardConfig.reproductionReward);
            }
        }

        private bool IsDead()
        {
            if (creatureConfig == null) return false;
            return energy <= creatureConfig.deathEnergyThreshold || 
                   integrity <= creatureConfig.deathIntegrityThreshold;
        }

        public float GetIntegrity()
        {
            return integrity;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActions = actionsOut.ContinuousActions;
            var discreteActions = actionsOut.DiscreteActions;

            // WASD movement
            float move = 0f;
            float turn = 0f;

            if (Input.GetKey(KeyCode.W)) move = 1f;
            if (Input.GetKey(KeyCode.S)) move = -1f;
            if (Input.GetKey(KeyCode.A)) turn = -1f;
            if (Input.GetKey(KeyCode.D)) turn = 1f;

            continuousActions[0] = move;
            continuousActions[1] = turn;

            // Space to scan, R to reproduce
            discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
            discreteActions[1] = Input.GetKey(KeyCode.R) ? 1 : 0;
        }
    }
}

