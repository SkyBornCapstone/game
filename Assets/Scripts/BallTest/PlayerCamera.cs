using Unity.Cinemachine;
using UnityEngine;

namespace BallTest
{
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera Instance;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        public void Awake()
        {
            Instance = this;
        }

        public void SetTarget(Transform target)
        {
            cinemachineCamera.Target.TrackingTarget = target;
        }
    }
}