using UnityEngine;

namespace SyntheticLife.Phi.World
{
    public class HazardZone : MonoBehaviour
    {
        [Header("Hazard Settings")]
        public float damagePerSecond = 0.1f;
        public float knockbackForce = 5f;

        private Collider hazardCollider;

        private void Start()
        {
            hazardCollider = GetComponent<Collider>();
            if (hazardCollider == null)
            {
                hazardCollider = gameObject.AddComponent<BoxCollider>();
                hazardCollider.isTrigger = true;
            }

            gameObject.tag = "Hazard";
        }

        public float GetDamageRate()
        {
            return damagePerSecond;
        }

        private void OnTriggerStay(Collider other)
        {
            // Damage is applied by CreatureAgent that checks for hazard zones
        }

        private void OnDrawGizmos()
        {
            if (hazardCollider != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                
                if (hazardCollider is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (hazardCollider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }
    }
}

