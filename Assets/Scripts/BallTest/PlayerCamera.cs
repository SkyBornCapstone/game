using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance;
    [SerializeField] private CinemachineCamera _cinemachineCamera;

    public void Awake()
    {
        Instance = this;
    }
    
    public void SetTarget(Transform target)
    {
        _cinemachineCamera.Target.TrackingTarget = target;
    }
}
