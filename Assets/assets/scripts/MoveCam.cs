using UnityEngine;

public class MoveCam : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Transform playerCam;

    // Update is called once per frame
    void Update()
    {
        transform.position = playerCam.position;
        
    }
}
