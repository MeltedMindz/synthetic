using UnityEngine;

namespace SyntheticLife.Phi.Config
{
    [CreateAssetMenu(fileName = "PhiFieldConfig", menuName = "SyntheticLife/Config/PhiFieldConfig")]
    public class PhiFieldConfig : ScriptableObject
    {
        [Header("Noise Parameters")]
        public float scale = 0.1f;
        public float speedX = 0.01f;
        public float speedY = 0.01f;
        public float amplitude = 1f;
        public int seed = 42;

        [Header("Visualization")]
        public bool showVisualization = false;
        public Material visualizationMaterial;
        public int resolution = 256;

        [Header("Back Action")]
        public bool enableBackAction = false;
        public float backActionRadius = 5f;
        public float backActionDuration = 2f;
        public float backActionDamping = 0.5f;
    }
}

