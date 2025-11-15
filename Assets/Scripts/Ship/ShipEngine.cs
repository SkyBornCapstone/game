using UnityEngine;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipEngine : MonoBehaviour
    {
        [Header("Engine Settings")]
        public float maxThrust = 10000f;
        //  Positive values request increasing engine throttle
        //  Negative values request decreasing engine throttle
        //  Zero means 'hold' (do not change current throttle)
        [Range(-1f, 1f)] public float throttle = 0f;
        public Vector3 localThrustDirection = Vector3.up;
        public float throttleResponse = 100f;
        public float currentThrust;
        public string engineID;

        void FixedUpdate()
        {
            if (!Mathf.Approximately(throttle, 0f))
            {
                currentThrust += throttle * throttleResponse * Time.fixedDeltaTime;
                currentThrust = Mathf.Clamp(currentThrust, 0f, maxThrust);
            }
        }

        public Vector3 GetWorldThrustForce()
        {
            return localThrustDirection.normalized * currentThrust;
        }

        public Vector3 GetRelativePosition(Transform shipRoot)
        {
            return transform.position - shipRoot.position;
        }
    }
}
