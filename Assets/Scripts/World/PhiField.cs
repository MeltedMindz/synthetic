using UnityEngine;
using System.Collections.Generic;
using SyntheticLife.Phi.Config;

namespace SyntheticLife.Phi.World
{
    public class PhiField : MonoBehaviour
    {
        [SerializeField] private PhiFieldConfig config;

        private float time = 0f;
        private Dictionary<Vector2, BackActionPatch> activeBackActions = new Dictionary<Vector2, BackActionPatch>();

        private struct BackActionPatch
        {
            public Vector2 center;
            public float startTime;
            public float duration;
            public float damping;
        }

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("PhiField: No PhiFieldConfig assigned!");
                return;
            }

            Random.InitState(config.seed);
        }

        private void Update()
        {
            time += Time.deltaTime;

            // Clean up expired back actions
            var expired = new List<Vector2>();
            foreach (var kvp in activeBackActions)
            {
                if (time - kvp.Value.startTime > kvp.Value.duration)
                {
                    expired.Add(kvp.Key);
                }
            }
            foreach (var key in expired)
            {
                activeBackActions.Remove(key);
            }
        }

        public float SamplePhi(Vector3 worldPos)
        {
            if (config == null) return 0f;

            float x = worldPos.x * config.scale + time * config.speedX;
            float y = worldPos.z * config.scale + time * config.speedY;

            float basePhi = Mathf.PerlinNoise(x, y);
            // Remap from [0,1] to [-1,1]
            float phi = (basePhi - 0.5f) * 2f * config.amplitude;

            // Apply back action if enabled and active patches exist
            if (config.enableBackAction && activeBackActions.Count > 0)
            {
                Vector2 pos2D = new Vector2(worldPos.x, worldPos.z);
                foreach (var patch in activeBackActions.Values)
                {
                    float dist = Vector2.Distance(pos2D, patch.center);
                    if (dist < config.backActionRadius)
                    {
                        float t = (time - patch.startTime) / patch.duration;
                        if (t < 1f)
                        {
                            float falloff = 1f - (dist / config.backActionRadius);
                            float dampingFactor = Mathf.Lerp(patch.damping, 1f, t) * falloff;
                            phi *= dampingFactor;
                        }
                    }
                }
            }

            return phi;
        }

        public float SamplePhiNoisy(Vector3 worldPos, float noiseLevel)
        {
            float phi = SamplePhi(worldPos);
            float noise = Random.Range(-noiseLevel, noiseLevel);
            return phi + noise;
        }

        public Vector2 SamplePhiGradient(Vector3 worldPos, float epsilon = 0.5f)
        {
            float phiXp = SamplePhi(worldPos + new Vector3(epsilon, 0, 0));
            float phiXn = SamplePhi(worldPos - new Vector3(epsilon, 0, 0));
            float phiZp = SamplePhi(worldPos + new Vector3(0, 0, epsilon));
            float phiZn = SamplePhi(worldPos - new Vector3(0, 0, epsilon));

            float gradX = (phiXp - phiXn) / (2f * epsilon);
            float gradZ = (phiZp - phiZn) / (2f * epsilon);

            return new Vector2(gradX, gradZ);
        }

        public void TriggerBackAction(Vector3 position)
        {
            if (!config.enableBackAction) return;

            Vector2 pos2D = new Vector2(position.x, position.z);
            activeBackActions[pos2D] = new BackActionPatch
            {
                center = pos2D,
                startTime = time,
                duration = config.backActionDuration,
                damping = config.backActionDamping
            };
        }

        public PhiFieldConfig GetConfig()
        {
            return config;
        }

        private void OnDrawGizmos()
        {
            if (config != null && config.showVisualization)
            {
                // Draw sample grid
                int gridSize = 20;
                float cellSize = 5f;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        Vector3 pos = new Vector3((x - gridSize/2) * cellSize, 0, (z - gridSize/2) * cellSize);
                        float phi = SamplePhi(pos);
                        
                        // Map phi [-1,1] to color [blue, red]
                        Color color = Color.Lerp(Color.blue, Color.red, (phi + 1f) / 2f);
                        Gizmos.color = color;
                        Gizmos.DrawCube(pos, Vector3.one * 2f);
                    }
                }
            }
        }
    }
}

