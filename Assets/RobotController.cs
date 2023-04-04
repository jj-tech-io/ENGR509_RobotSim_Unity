using UnityEngine;

public class RobotController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float turnSpeed = 50.0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // Move the robot forward and backward
        rb.velocity = transform.forward * moveInput * moveSpeed;

        // Rotate the robot around the Y-axis
        rb.angularVelocity = new Vector3(0, turnInput * turnSpeed * Mathf.Deg2Rad, 0);
    }
}
