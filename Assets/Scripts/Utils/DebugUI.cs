using UnityEngine;
using UnityEngine.UI;
using SyntheticLife.Phi.Core;

namespace SyntheticLife.Phi.Utils
{
    public class DebugUI : MonoBehaviour
    {
        [Header("UI References")]
        public Text statusText;
        public CreatureAgent trackedCreature;

        private void Update()
        {
            if (statusText != null && trackedCreature != null)
            {
                string info = $"Energy: {trackedCreature.energy:F1}\n";
                info += $"Temperature: {trackedCreature.temperature:F2}\n";
                info += $"Integrity: {trackedCreature.integrity:F2}\n";
                info += $"Age: {trackedCreature.growthSystem?.age:F1}s\n";
                info += $"Stage: {trackedCreature.growthSystem?.stage}\n";
                info += $"Size: {trackedCreature.growthSystem?.size:F2}\n";
                info += $"Phi Sense: {trackedCreature.transform.position}\n";
                
                if (trackedCreature.populationManager != null)
                {
                    info += $"Population: {trackedCreature.populationManager.GetPopulationCount()}\n";
                }

                statusText.text = info;
            }
        }
    }
}

