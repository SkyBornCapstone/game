using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ThirdPersonCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public enum CameraStyle
    {
        Basic,
        Combat
    }

    public CameraStyle style;

    public Transform CombatLookAt;

    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(style == CameraStyle.Basic)
        {
            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

            }

            else if (style == CameraStyle.Combat)
            {
                Vector3 directionToCombatLookAt = CombatLookAt.position - new Vector3(transform.position.x, CombatLookAt.position.y, transform.position.z);
                orientation.forward = directionToCombatLookAt.normalized;

                playerObj.forward = directionToCombatLookAt.normalized;

            }

        }

        
    }
}
