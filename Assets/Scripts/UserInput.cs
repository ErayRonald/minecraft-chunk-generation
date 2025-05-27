using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    private float baseMoveSpeed = 50f; // Increased base movement speed in units per second
    private float moveSpeedMultiplier = 1f; // Movement speed multiplier
    private float pitchSpeed = 150f; // Adjust this value to control the pitch speed
    private float yawSpeed = 150f;
    private float pitchAngle = 0f;
    private float yawAngle = 0f;
    private bool isPitching = false;

    // Update is called once per frame
    void Update()
    {
        // Check if Shift key is held down to increase movement speed
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeedMultiplier = 10f;
        }
        // Check if Ctrl key is held down to decrease movement speed
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            moveSpeedMultiplier = 0.1f;
        }
        else
        {
            moveSpeedMultiplier = 1f;
        }

        // Forward movement when pressing "W"
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * baseMoveSpeed * moveSpeedMultiplier * Time.deltaTime);
        }

        // Backward movement when pressing "S"
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * baseMoveSpeed * moveSpeedMultiplier * Time.deltaTime);
        }

        // Right movement when pressing "D"
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * baseMoveSpeed * moveSpeedMultiplier * Time.deltaTime);
        }

        // Left movement when pressing "A"
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * baseMoveSpeed * moveSpeedMultiplier * Time.deltaTime);
        }

        // Pitch functionality when holding down the right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            isPitching = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isPitching = false;
        }
        if (isPitching)
        {
            // Get mouse movement for pitch
            float mouseY = Input.GetAxis("Mouse Y");
            pitchAngle -= mouseY * pitchSpeed * Time.deltaTime;
            pitchAngle = Mathf.Clamp(pitchAngle, -90f, 90f);

            // Get mouse movement for yaw
            float mouseX = Input.GetAxis("Mouse X");
            yawAngle += mouseX * yawSpeed * Time.deltaTime;
            yawAngle = Mathf.Clamp(yawAngle, -180f, 180f);

            // Apply rotation to the camera
            transform.eulerAngles = new Vector3(pitchAngle, yawAngle, 0f);
        }
    }
}
