using UnityEngine;

namespace SyntheticLife.Phi.World
{
    public class ShelterZone : MonoBehaviour
    {
        [Header("Shelter Settings")]
        public float metabolismReduction = 0.5f;
        public float healingMultiplier = 2f;

        private Collider shelterCollider;

        private void Start()
        {
            shelterCollider = GetComponent<Collider>();
            if (shelterCollider == null)
            {
                shelterCollider = gameObject.AddComponent<BoxCollider>();
                shelterCollider.isTrigger = true;
            }

            gameObject.tag = "Shelter";
        }

        public bool IsInside(Vector3 position)
        {
            if (shelterCollider == null) return false;
            Vector3 closestPoint = shelterCollider.ClosestPoint(position);
            return Vector3.Distance(position, closestPoint) < 0.1f;
        }

        public float GetMetabolismMultiplier()
        {
            return metabolismReduction;
        }

        public float GetHealingMultiplier()
        {
            return healingMultiplier;
        }

        private void OnDrawGizmos()
        {
            if (shelterCollider != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                
                if (shelterCollider is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (shelterCollider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }
    }
}

