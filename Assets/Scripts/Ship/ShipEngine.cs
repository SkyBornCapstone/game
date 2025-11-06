using UnityEngine;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipEngine : MonoBehaviour
    {
        [Header("Engine Settings")]
        public float maxThrust = 10000f;           // N
        [Range(0f, 1f)] public float throttle = 0f;
        public Vector3 localThrustDirection = Vector3.up;
        public float throttleResponse = 2f;

        private float currentThrottle;
        public string engineID;

        void Update()
        {
            // Smooth throttle change for realism
            currentThrottle = Mathf.MoveTowards(currentThrottle, throttle, throttleResponse * Time.deltaTime);
        }

        public Vector3 GetWorldThrustForce()
        {
            return localThrustDirection.normalized * (maxThrust * currentThrottle);
        }

        public Vector3 GetRelativePosition(Transform shipRoot)
        {
            return transform.position - shipRoot.position;
        }
    }
}
