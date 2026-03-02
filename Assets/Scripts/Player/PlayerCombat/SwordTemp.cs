using UnityEngine;

public class SwordTemp : MonoBehaviour
{
    public Transform handPosition;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = handPosition.position;
        this.transform.rotation = handPosition.rotation;
    }
}
