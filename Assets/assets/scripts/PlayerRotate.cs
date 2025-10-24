using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float sensX = 100f;
    public Transform orientation;

    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;

        // Only rotate horizontally
        yRotation += mouseX;

        // Apply rotation to the player (yaw only)
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

        // Keep orientation synced (if needed)
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
