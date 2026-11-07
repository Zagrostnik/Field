
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    [Header("Mouse Sway")]
    [SerializeField] private float mouseSwayAmount = 2f;
    [SerializeField] private float mouseSwaySmoothness = 8f;

    [Header("Movement Sway")]
    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float walkBobAmount = 0.015f;
    [SerializeField] private float runBobAmount = 0.035f;

    [Header("Position")]
    [SerializeField] private float positionSmoothness = 8f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float bobTimer;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        HandleMouseSway();
        HandleMovementSway();
    }

    private void HandleMouseSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Quaternion targetRotation = initialRotation * Quaternion.Euler(
            -mouseY * mouseSwayAmount,
            mouseX * mouseSwayAmount,
            -mouseX * mouseSwayAmount
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * mouseSwaySmoothness
        );
    }

    private void HandleMovementSway()
    {
        if (characterController == null)
            return;

        Vector3 velocity = characterController.velocity;
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

        bool isMoving = characterController.isGrounded && speed > 0.1f;

        float targetY = 0f;
        float targetX = 0f;

        if (isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            float amplitude = isRunning ? runBobAmount : walkBobAmount;

            bobTimer += Time.deltaTime * walkBobSpeed;

            targetY = Mathf.Sin(bobTimer) * amplitude;
            targetX = Mathf.Cos(bobTimer * 0.5f) * amplitude;
        }

        Vector3 targetPosition = initialPosition +
            new Vector3(targetX, targetY, 0f);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * positionSmoothness
        );
    }
}
