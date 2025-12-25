using UnityEngine;

namespace SyntheticLife.Phi.Config
{
    [CreateAssetMenu(fileName = "CreatureDefaults", menuName = "SyntheticLife/Config/CreatureDefaults")]
    public class CreatureDefaults : ScriptableObject
    {
        [Header("Initial State")]
        public float initialEnergy = 100f;
        public float initialTemperature = 0.5f;
        public float initialIntegrity = 1f;
        public float initialAge = 0f;
        public Genome defaultGenome;

        [Header("Life Stages (seconds)")]
        public float juvenileAge = 10f;
        public float adultAge = 30f;
        public float elderAge = 60f;

        [Header("Growth")]
        public float sizeGrowthRate = 0.01f;
        public float sizeFromAgeMax = 2f;
        public float nutritionThreshold = 0.7f;
        public float stressThreshold = 0.3f;

        [Header("Reproduction")]
        public float reproductionEnergyThreshold = 0.7f;
        public float reproductionIntegrityThreshold = 0.6f;
        public float reproductionCooldown = 15f;
        public float reproductionEnergyCost = 50f;
        public float reproductionVulnerabilityDuration = 5f;

        [Header("Death")]
        public float deathEnergyThreshold = 0f;
        public float deathIntegrityThreshold = 0f;

        [Header("Mutation")]
        public float mutationRate = 0.3f;
        public float mutationStrength = 0.1f;
    }
}

