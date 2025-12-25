using UnityEngine;

namespace SyntheticLife.Phi.World
{
    public class TemperatureZone : MonoBehaviour
    {
        [Header("Temperature Settings")]
        [Range(0f, 1f)]
        public float temperatureValue = 0.5f;
        public float transitionRadius = 5f;

        private Collider zoneCollider;

        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null)
            {
                // Add a box collider if none exists
                zoneCollider = gameObject.AddComponent<BoxCollider>();
                zoneCollider.isTrigger = true;
            }

            gameObject.tag = "TemperatureZone";
        }

        public float GetTemperatureAt(Vector3 position)
        {
            if (zoneCollider == null) return 0.5f;

            // Check if point is inside zone
            Vector3 closestPoint = zoneCollider.ClosestPoint(position);
            float distance = Vector3.Distance(position, closestPoint);

            if (distance < 0.1f)
            {
                // Inside zone
                return temperatureValue;
            }
            else if (distance < transitionRadius)
            {
                // In transition zone - lerp between ambient and zone temp
                float t = 1f - (distance / transitionRadius);
                return Mathf.Lerp(0.5f, temperatureValue, t);
            }

            // Outside zone - return neutral
            return 0.5f;
        }

        private void OnDrawGizmos()
        {
            if (zoneCollider != null)
            {
                Gizmos.color = temperatureValue > 0.5f ? Color.red : Color.blue;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
                
                if (zoneCollider is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (zoneCollider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }
    }
}

