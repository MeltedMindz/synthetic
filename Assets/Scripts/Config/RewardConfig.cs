using UnityEngine;

namespace SyntheticLife.Phi.Config
{
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "SyntheticLife/Config/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [Header("Living Rewards")]
        public float livingReward = 0.001f;
        public float minEnergyForLivingReward = 0.2f;
        public float minIntegrityForLivingReward = 0.3f;

        [Header("Penalties")]
        public float starvationPenalty = -0.01f;
        public float damagePenalty = -0.05f;
        public float temperatureStressPenalty = -0.02f;
        public float deathPenalty = -10f;

        [Header("Food Rewards")]
        public float foodRewardBase = 0.1f;
        public float foodRewardDiminishingFactor = 0.8f;

        [Header("Reproduction Rewards")]
        public float reproductionReward = 0.5f;
        public float offspringSurvivalReward = 2f;
        public float offspringSurvivalTimeThreshold = 10f;
        public float parentSurvivalReward = 1f;
        public float parentSurvivalTimeThreshold = 15f;

        [Header("Exploration")]
        public bool enableExplorationReward = true;
        public float explorationReward = 0.001f;
        public float explorationGridSize = 2f;
    }
}

