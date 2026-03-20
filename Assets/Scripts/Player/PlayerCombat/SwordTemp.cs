using UnityEngine;

public class SwordTemp : MonoBehaviour
{
    public Transform handPosition;
    public float xRot = 0;
    public float yRot = 0;
    public float zRot = 0;
    
    void Update()
    {
        this.transform.position = handPosition.position;
        this.transform.rotation = handPosition.rotation;
        this.transform.Rotate(xRot, yRot, zRot);
    }
}
