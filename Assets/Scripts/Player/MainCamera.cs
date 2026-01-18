using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class MainCamera : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;

        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
        }

        public void SetTarget(Transform target)
        {
            cinemachineCamera.Follow = target;
        }
    }
}