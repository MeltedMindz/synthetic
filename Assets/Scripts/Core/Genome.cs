using System;
using UnityEngine;

namespace SyntheticLife.Phi.Core
{
    [Serializable]
    public struct GenomeRanges
    {
        [Header("Instinct Biases")]
        public Vector2 hungerUrgencyBiasRange;
        public Vector2 dangerAversionBiasRange;
        public Vector2 scanCuriosityBiasRange;
        public Vector2 explorationNoiseBiasRange;
        public Vector2 reproductionConservatismRange;

        [Header("Life History")]
        public Vector2 growthRateRange;
        public Vector2 maturityAgeNormRange;
        public Vector2 maxAgeNormRange;

        [Header("Physiology")]
        public Vector2 baseEmaxRange;
        public Vector2 basalMetabolismRange;
        public Vector2 moveEfficiencyRange;
        public Vector2 tempToleranceRange;
        public Vector2 antennaGainRange;
        public Vector2 scanCostRange;
        public Vector2 scanRadiusRange;
        public Vector2 phiAffinityRange;

        public static GenomeRanges Default()
        {
            return new GenomeRanges
            {
                hungerUrgencyBiasRange = new Vector2(0.5f, 2.0f),
                dangerAversionBiasRange = new Vector2(0.5f, 2.0f),
                scanCuriosityBiasRange = new Vector2(0.0f, 1.0f),
                explorationNoiseBiasRange = new Vector2(0.0f, 0.2f),
                reproductionConservatismRange = new Vector2(0.0f, 1.0f),
                growthRateRange = new Vector2(0.5f, 1.5f),
                maturityAgeNormRange = new Vector2(0.3f, 0.7f),
                maxAgeNormRange = new Vector2(0.5f, 1.0f),
                baseEmaxRange = new Vector2(50f, 200f),
                basalMetabolismRange = new Vector2(0.1f, 5f),
                moveEfficiencyRange = new Vector2(0.5f, 2f),
                tempToleranceRange = new Vector2(0.1f, 0.9f),
                antennaGainRange = new Vector2(0.1f, 3f),
                scanCostRange = new Vector2(0.5f, 10f),
                scanRadiusRange = new Vector2(2f, 15f),
                phiAffinityRange = new Vector2(-1f, 1f)
            };
        }
    }

    [Serializable]
    public struct Genome
    {
        [Header("Instinct Biases")]
        public float hungerUrgencyBias;          // scales penalty for low energy
        public float dangerAversionBias;         // scales penalty for integrity loss and hazards
        public float scanCuriosityBias;          // scales intrinsic reward for reducing uncertainty
        public float explorationNoiseBias;       // adds small exploration noise to movement actions
        public float reproductionConservatism;   // raises reproduction threshold and/or increases reproduction cost

        [Header("Life History Traits")]
        public float growthRate;
        public float maturityAgeNorm;            // 0..1 mapped to real maturity age range
        public float maxAgeNorm;                 // 0..1 mapped to real max age range

        [Header("Metabolism")]
        public float baseEmax;
        public float basalMetabolism;
        public float moveEfficiency;

        [Header("Temperature")]
        public float tempTolerance;

        [Header("Sensing")]
        public float antennaGain;
        public float scanCost;
        public float scanRadius;

        [Header("Phi Coupling")]
        public float phiAffinity;

        public static Genome CreateDefault(CreatureDefaults defaults)
        {
            GenomeRanges ranges = defaults.genomeRanges;
            return new Genome
            {
                hungerUrgencyBias = 1.0f,
                dangerAversionBias = 1.0f,
                scanCuriosityBias = 0.5f,
                explorationNoiseBias = 0.05f,
                reproductionConservatism = 0.0f,
                growthRate = 1.0f,
                maturityAgeNorm = 0.5f,
                maxAgeNorm = 0.75f,
                baseEmax = 100f,
                basalMetabolism = 1f,
                moveEfficiency = 1f,
                tempTolerance = 0.5f,
                antennaGain = 1f,
                scanCost = 2f,
                scanRadius = 5f,
                phiAffinity = 0f
            };
        }

        public static Genome Mutate(System.Random rng, Genome parent, GenomeRanges ranges, float mutationRate, float mutationStrength)
        {
            Genome child = parent;

            // Instinct biases
            child.hungerUrgencyBias = MutateValue(rng, child.hungerUrgencyBias, mutationRate, mutationStrength, ranges.hungerUrgencyBiasRange);
            child.dangerAversionBias = MutateValue(rng, child.dangerAversionBias, mutationRate, mutationStrength, ranges.dangerAversionBiasRange);
            child.scanCuriosityBias = MutateValue(rng, child.scanCuriosityBias, mutationRate, mutationStrength, ranges.scanCuriosityBiasRange);
            child.explorationNoiseBias = MutateValue(rng, child.explorationNoiseBias, mutationRate, mutationStrength, ranges.explorationNoiseBiasRange);
            child.reproductionConservatism = MutateValue(rng, child.reproductionConservatism, mutationRate, mutationStrength, ranges.reproductionConservatismRange);

            // Life history
            child.growthRate = MutateValue(rng, child.growthRate, mutationRate, mutationStrength, ranges.growthRateRange);
            child.maturityAgeNorm = MutateValue(rng, child.maturityAgeNorm, mutationRate, mutationStrength, ranges.maturityAgeNormRange);
            child.maxAgeNorm = MutateValue(rng, child.maxAgeNorm, mutationRate, mutationStrength, ranges.maxAgeNormRange);

            // Physiology
            child.baseEmax = MutateValue(rng, child.baseEmax, mutationRate, mutationStrength, ranges.baseEmaxRange);
            child.basalMetabolism = MutateValue(rng, child.basalMetabolism, mutationRate, mutationStrength, ranges.basalMetabolismRange);
            child.moveEfficiency = MutateValue(rng, child.moveEfficiency, mutationRate, mutationStrength, ranges.moveEfficiencyRange);
            child.tempTolerance = MutateValue(rng, child.tempTolerance, mutationRate, mutationStrength, ranges.tempToleranceRange);
            child.antennaGain = MutateValue(rng, child.antennaGain, mutationRate, mutationStrength, ranges.antennaGainRange);
            child.scanCost = MutateValue(rng, child.scanCost, mutationRate, mutationStrength, ranges.scanCostRange);
            child.scanRadius = MutateValue(rng, child.scanRadius, mutationRate, mutationStrength, ranges.scanRadiusRange);
            child.phiAffinity = MutateValue(rng, child.phiAffinity, mutationRate, mutationStrength, ranges.phiAffinityRange);

            return child;
        }

        private static float MutateValue(System.Random rng, float value, float rate, float strength, Vector2 range)
        {
            if (rng.NextDouble() < rate)
            {
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * strength;
                value += noise;
            }
            return Mathf.Clamp(value, range.x, range.y);
        }

        public Genome Clamp(GenomeRanges ranges)
        {
            return new Genome
            {
                hungerUrgencyBias = Mathf.Clamp(hungerUrgencyBias, ranges.hungerUrgencyBiasRange.x, ranges.hungerUrgencyBiasRange.y),
                dangerAversionBias = Mathf.Clamp(dangerAversionBias, ranges.dangerAversionBiasRange.x, ranges.dangerAversionBiasRange.y),
                scanCuriosityBias = Mathf.Clamp(scanCuriosityBias, ranges.scanCuriosityBiasRange.x, ranges.scanCuriosityBiasRange.y),
                explorationNoiseBias = Mathf.Clamp(explorationNoiseBias, ranges.explorationNoiseBiasRange.x, ranges.explorationNoiseBiasRange.y),
                reproductionConservatism = Mathf.Clamp(reproductionConservatism, ranges.reproductionConservatismRange.x, ranges.reproductionConservatismRange.y),
                growthRate = Mathf.Clamp(growthRate, ranges.growthRateRange.x, ranges.growthRateRange.y),
                maturityAgeNorm = Mathf.Clamp(maturityAgeNorm, ranges.maturityAgeNormRange.x, ranges.maturityAgeNormRange.y),
                maxAgeNorm = Mathf.Clamp(maxAgeNorm, ranges.maxAgeNormRange.x, ranges.maxAgeNormRange.y),
                baseEmax = Mathf.Clamp(baseEmax, ranges.baseEmaxRange.x, ranges.baseEmaxRange.y),
                basalMetabolism = Mathf.Clamp(basalMetabolism, ranges.basalMetabolismRange.x, ranges.basalMetabolismRange.y),
                moveEfficiency = Mathf.Clamp(moveEfficiency, ranges.moveEfficiencyRange.x, ranges.moveEfficiencyRange.y),
                tempTolerance = Mathf.Clamp(tempTolerance, ranges.tempToleranceRange.x, ranges.tempToleranceRange.y),
                antennaGain = Mathf.Clamp(antennaGain, ranges.antennaGainRange.x, ranges.antennaGainRange.y),
                scanCost = Mathf.Clamp(scanCost, ranges.scanCostRange.x, ranges.scanCostRange.y),
                scanRadius = Mathf.Clamp(scanRadius, ranges.scanRadiusRange.x, ranges.scanRadiusRange.y),
                phiAffinity = Mathf.Clamp(phiAffinity, ranges.phiAffinityRange.x, ranges.phiAffinityRange.y)
            };
        }

        public int GetHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + hungerUrgencyBias.GetHashCode();
                hash = hash * 31 + dangerAversionBias.GetHashCode();
                hash = hash * 31 + scanCuriosityBias.GetHashCode();
                hash = hash * 31 + explorationNoiseBias.GetHashCode();
                hash = hash * 31 + reproductionConservatism.GetHashCode();
                hash = hash * 31 + growthRate.GetHashCode();
                hash = hash * 31 + maturityAgeNorm.GetHashCode();
                hash = hash * 31 + maxAgeNorm.GetHashCode();
                hash = hash * 31 + baseEmax.GetHashCode();
                hash = hash * 31 + basalMetabolism.GetHashCode();
                hash = hash * 31 + moveEfficiency.GetHashCode();
                hash = hash * 31 + tempTolerance.GetHashCode();
                hash = hash * 31 + antennaGain.GetHashCode();
                hash = hash * 31 + scanCost.GetHashCode();
                hash = hash * 31 + scanRadius.GetHashCode();
                hash = hash * 31 + phiAffinity.GetHashCode();
                return hash;
            }
        }

        public static Genome Lerp(Genome a, Genome b, float t)
        {
            return new Genome
            {
                hungerUrgencyBias = Mathf.Lerp(a.hungerUrgencyBias, b.hungerUrgencyBias, t),
                dangerAversionBias = Mathf.Lerp(a.dangerAversionBias, b.dangerAversionBias, t),
                scanCuriosityBias = Mathf.Lerp(a.scanCuriosityBias, b.scanCuriosityBias, t),
                explorationNoiseBias = Mathf.Lerp(a.explorationNoiseBias, b.explorationNoiseBias, t),
                reproductionConservatism = Mathf.Lerp(a.reproductionConservatism, b.reproductionConservatism, t),
                growthRate = Mathf.Lerp(a.growthRate, b.growthRate, t),
                maturityAgeNorm = Mathf.Lerp(a.maturityAgeNorm, b.maturityAgeNorm, t),
                maxAgeNorm = Mathf.Lerp(a.maxAgeNorm, b.maxAgeNorm, t),
                baseEmax = Mathf.Lerp(a.baseEmax, b.baseEmax, t),
                basalMetabolism = Mathf.Lerp(a.basalMetabolism, b.basalMetabolism, t),
                moveEfficiency = Mathf.Lerp(a.moveEfficiency, b.moveEfficiency, t),
                tempTolerance = Mathf.Lerp(a.tempTolerance, b.tempTolerance, t),
                antennaGain = Mathf.Lerp(a.antennaGain, b.antennaGain, t),
                scanCost = Mathf.Lerp(a.scanCost, b.scanCost, t),
                scanRadius = Mathf.Lerp(a.scanRadius, b.scanRadius, t),
                phiAffinity = Mathf.Lerp(a.phiAffinity, b.phiAffinity, t)
            };
        }
    }
}
