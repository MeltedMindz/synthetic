using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;

namespace SyntheticLife.Phi.Core
{
    public class PopulationManager : MonoBehaviour
    {
        [Header("Population Settings")]
        public int maxCreatures = 20;
        public GameObject creaturePrefab;

        private List<CreatureAgent> allCreatures = new List<CreatureAgent>();
        private Dictionary<CreatureAgent, float> creatureSpawnTimes = new Dictionary<CreatureAgent, float>();

        private void Start()
        {
            if (creaturePrefab == null)
            {
                Debug.LogError("PopulationManager: No creature prefab assigned!");
            }
        }

        public void RegisterCreature(CreatureAgent creature)
        {
            if (!allCreatures.Contains(creature))
            {
                allCreatures.Add(creature);
                creatureSpawnTimes[creature] = Time.time;
            }
        }

        public void UnregisterCreature(CreatureAgent creature)
        {
            allCreatures.Remove(creature);
            creatureSpawnTimes.Remove(creature);
        }

        public bool CanSpawn()
        {
            // Remove null references
            allCreatures.RemoveAll(c => c == null);
            
            return allCreatures.Count < maxCreatures;
        }

        public void EnforcePopulationLimit()
        {
            allCreatures.RemoveAll(c => c == null);
            creatureSpawnTimes = creatureSpawnTimes
                .Where(kvp => kvp.Key != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            while (allCreatures.Count > maxCreatures)
            {
                // Find oldest creature or lowest integrity
                CreatureAgent toRemove = null;
                float oldestTime = float.MaxValue;
                float lowestIntegrity = float.MaxValue;

                foreach (var creature in allCreatures)
                {
                    if (creature == null) continue;

                    float integrity = creature.GetIntegrity();
                    float spawnTime = creatureSpawnTimes.ContainsKey(creature) ? creatureSpawnTimes[creature] : Time.time;

                    // Prioritize removing lowest integrity, then oldest
                    if (integrity < lowestIntegrity || 
                        (Mathf.Approximately(integrity, lowestIntegrity) && spawnTime < oldestTime))
                    {
                        lowestIntegrity = integrity;
                        oldestTime = spawnTime;
                        toRemove = creature;
                    }
                }

                if (toRemove != null)
                {
                    // End episode for this agent
                    toRemove.EndEpisode();
                    Destroy(toRemove.gameObject);
                }
                else
                {
                    break;
                }
            }
        }

        public int GetPopulationCount()
        {
            allCreatures.RemoveAll(c => c == null);
            return allCreatures.Count;
        }

        private void Update()
        {
            EnforcePopulationLimit();
        }
    }
}

