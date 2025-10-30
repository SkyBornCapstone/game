using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _cam;

    private void Awake()
    {
        _cam = Camera.main?.transform;
    }

    private void LateUpdate()
    {
        if (!_cam)
        {
            _cam = Camera.main?.transform;
            if (!_cam) return;
        }
        
        var dir = _cam.position - transform.position;
        dir.x = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
