using System;
using UnityEngine;

namespace SyntheticLife.Phi.Core
{
    [Serializable]
    public struct Genome
    {
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

        public static Genome Mutate(Genome parent, float mutationRate, float mutationStrength)
        {
            Genome child = parent;
            
            child.baseEmax = MutateValue(child.baseEmax, mutationRate, mutationStrength, 50f, 200f);
            child.basalMetabolism = MutateValue(child.basalMetabolism, mutationRate, mutationStrength, 0.1f, 5f);
            child.moveEfficiency = MutateValue(child.moveEfficiency, mutationRate, mutationStrength, 0.5f, 2f);
            child.tempTolerance = MutateValue(child.tempTolerance, mutationRate, mutationStrength, 0.1f, 0.9f);
            child.antennaGain = MutateValue(child.antennaGain, mutationRate, mutationStrength, 0.1f, 3f);
            child.scanCost = MutateValue(child.scanCost, mutationRate, mutationStrength, 0.5f, 10f);
            child.scanRadius = MutateValue(child.scanRadius, mutationRate, mutationStrength, 2f, 15f);
            child.phiAffinity = MutateValue(child.phiAffinity, mutationRate, mutationStrength, -1f, 1f);

            return child;
        }

        private static float MutateValue(float value, float rate, float strength, float min, float max)
        {
            if (UnityEngine.Random.value < rate)
            {
                float noise = UnityEngine.Random.Range(-strength, strength);
                value += noise;
                value = Mathf.Clamp(value, min, max);
            }
            return value;
        }

        public static Genome Lerp(Genome a, Genome b, float t)
        {
            return new Genome
            {
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

