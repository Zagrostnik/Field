
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CameraBobbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private CharacterController characterController;

    [Header("Walking")]
    [SerializeField] private float walkFrequency = 8f;
    [SerializeField] private float walkAmplitude = 0.035f;

    [Header("Running")]
    [SerializeField] private float runFrequency = 12f;
    [SerializeField] private float runAmplitude = 0.065f;

    [Header("Movement")]
    [SerializeField] private float speedForFullBob = 5.5f;
    [SerializeField] private float smoothness = 10f;

    private Vector3 initialPosition;
    private float bobTimer;

    private void Start()
    {
        if (cameraPivot == null)
            cameraPivot = transform;

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        initialPosition = cameraPivot.localPosition;
    }

    private void Update()
    {
        float horizontalSpeed = new Vector3(
            characterController.velocity.x,
            0f,
            characterController.velocity.z
        ).magnitude;

        bool isMoving = characterController.isGrounded && horizontalSpeed > 0.1f;

        float targetX = 0f;
        float targetY = 0f;

        if (isMoving)
        {
            float movement = Mathf.Clamp01(horizontalSpeed / speedForFullBob);

            bool isRunning = horizontalSpeed > speedForFullBob * 0.65f;

            float frequency = isRunning ? runFrequency : walkFrequency;
            float amplitude = isRunning ? runAmplitude : walkAmplitude;

            bobTimer += Time.deltaTime * frequency;

            targetY = Mathf.Sin(bobTimer) * amplitude * movement;
            targetX = Mathf.Cos(bobTimer * 0.5f) * amplitude * 0.5f * movement;
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * 3f);
        }

        Vector3 targetPosition = initialPosition + new Vector3(targetX, targetY, 0f);

        cameraPivot.localPosition = Vector3.Lerp(
            cameraPivot.localPosition,
            targetPosition,
            Time.deltaTime * smoothness
        );
    }
}
