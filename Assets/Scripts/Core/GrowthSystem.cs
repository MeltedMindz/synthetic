using UnityEngine;

namespace SyntheticLife.Phi.Core
{
    public enum LifeStage
    {
        Juvenile,
        Adult,
        Elder
    }

    public class GrowthSystem : MonoBehaviour
    {
        [Header("Current State")]
        public float age = 0f;
        public LifeStage stage = LifeStage.Juvenile;
        public float size = 1f;
        public float nutritionHistory = 0f;
        public float stressHistory = 0f;

        [Header("Derived Stats")]
        public float maxEnergy = 100f;
        public float basalMetabolismRate = 1f;
        public float locomotionCostMultiplier = 1f;
        public float scanCostMultiplier = 1f;

        private CreatureDefaults defaults;
        private Genome genome;
        private float lastNutritionUpdate = 0f;
        private float lastStressUpdate = 0f;

        public void Initialize(CreatureDefaults config, Genome initialGenome)
        {
            defaults = config;
            genome = initialGenome;
            age = config.initialAge;
            size = 1f;
            nutritionHistory = 0.5f;
            stressHistory = 0.5f;
            UpdateDerivedStats();
        }

        public void UpdateGrowth(float deltaTime, float currentEnergy, float currentIntegrity, float ambientTemp, float targetTemp)
        {
            // Age progression influenced by growthRate instinct
            age += deltaTime * genome.growthRate;

            // Compute max age from instinct parameter
            float maxAge = Mathf.Lerp(defaults.maxAgeMin, defaults.maxAgeMax, genome.maxAgeNorm);

            // Age clamping (senescence)
            if (age > maxAge)
            {
                age = maxAge;
            }

            // Update nutrition history
            if (currentEnergy > defaults.initialEnergy * defaults.nutritionThreshold)
            {
                nutritionHistory = Mathf.Lerp(nutritionHistory, 1f, deltaTime * 0.1f);
            }
            else
            {
                nutritionHistory = Mathf.Lerp(nutritionHistory, 0f, deltaTime * 0.1f);
            }

            // Update stress history
            float tempStress = Mathf.Abs(ambientTemp - targetTemp);
            if (currentIntegrity < 0.8f || tempStress > defaults.stressThreshold)
            {
                stressHistory = Mathf.Lerp(stressHistory, 1f, deltaTime * 0.2f);
            }
            else
            {
                stressHistory = Mathf.Lerp(stressHistory, 0f, deltaTime * 0.1f);
            }

            // Stage transitions (using maturityAgeNorm from instinct)
            UpdateLifeStage(maxAge);

            // Size growth (based on age and nutrition, influenced by growthRate)
            float growthMultiplier = genome.growthRate;
            float targetSize = 1f + (age / maxAge) * defaults.sizeFromAgeMax * nutritionHistory * growthMultiplier;
            size = Mathf.Lerp(size, targetSize, deltaTime * defaults.sizeGrowthRate * genome.growthRate);
            size = Mathf.Clamp(size, 1f, 1f + defaults.sizeFromAgeMax);

            UpdateDerivedStats();
        }

        private void UpdateLifeStage(float maxAge)
        {
            LifeStage newStage = stage;

            // Compute maturity age from instinct parameter
            float maturityAge = Mathf.Lerp(defaults.maturityAgeMin, defaults.maturityAgeMax, genome.maturityAgeNorm);

            // Can only advance stages, not regress
            if (stage == LifeStage.Juvenile && age >= maturityAge && nutritionHistory > defaults.nutritionThreshold)
            {
                newStage = LifeStage.Adult;
            }
            else if (stage == LifeStage.Adult && age >= maturityAge * 1.5f)
            {
                newStage = LifeStage.Elder;
            }

            stage = newStage;
        }

        public float GetMaxAge()
        {
            return Mathf.Lerp(defaults.maxAgeMin, defaults.maxAgeMax, genome.maxAgeNorm);
        }

        public float GetMaturityAge()
        {
            return Mathf.Lerp(defaults.maturityAgeMin, defaults.maturityAgeMax, genome.maturityAgeNorm);
        }

        private void UpdateDerivedStats()
        {
            // Energy capacity scales with size and genome
            maxEnergy = genome.baseEmax * size;

            // Basal metabolism scales with size
            basalMetabolismRate = genome.basalMetabolism * Mathf.Pow(size, 0.75f);

            // Movement costs scale with size (larger creatures cost more to move)
            // Note: moveEfficiency from genome already influences this
            locomotionCostMultiplier = genome.moveEfficiency * Mathf.Pow(size, 1.5f);

            // Scan costs scale with size (scanCost from genome already set)
            scanCostMultiplier = genome.scanCost * size;
        }

        public Genome GetGenome()
        {
            return genome;
        }

        public bool CanReproduce()
        {
            return stage == LifeStage.Adult && 
                   nutritionHistory > defaults.nutritionThreshold &&
                   stressHistory < defaults.stressThreshold;
        }

        public void SetGenome(Genome newGenome)
        {
            genome = newGenome;
            UpdateDerivedStats();
        }
    }
}

