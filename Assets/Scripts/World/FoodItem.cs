using UnityEngine;

namespace SyntheticLife.Phi.World
{
    public class FoodItem : MonoBehaviour
    {
        [Header("Food Properties")]
        public float nutrientValue = 10f;
        public float phiValue = 0f;

        [Header("Visual")]
        public Renderer foodRenderer;
        public Color baseColor = Color.green;

        private void Start()
        {
            if (foodRenderer == null)
            {
                foodRenderer = GetComponent<Renderer>();
            }
            
            UpdateVisual();
        }

        public void Initialize(float nutrients, float phi)
        {
            nutrientValue = nutrients;
            phiValue = phi;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (foodRenderer != null)
            {
                // Color based on nutrient value
                float intensity = Mathf.Clamp01(nutrientValue / 20f);
                foodRenderer.material.color = baseColor * intensity;
            }

            // Scale based on nutrient value
            float scale = 0.5f + (nutrientValue / 20f) * 0.5f;
            transform.localScale = Vector3.one * scale;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Food consumption is handled by CreatureAgent collision detection
        }
    }
}

