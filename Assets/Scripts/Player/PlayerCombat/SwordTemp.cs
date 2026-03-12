using UnityEngine;

public class SwordTemp : MonoBehaviour
{
    public Transform handPosition;
    
    void Update()
    {
        this.transform.position = handPosition.position;
        this.transform.rotation = handPosition.rotation;
        this.transform.Rotate(0,0,90);
    }
}
