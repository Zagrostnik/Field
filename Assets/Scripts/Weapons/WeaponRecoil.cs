
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Position")]
    [SerializeField] private float kickBack = 0.045f;
    [SerializeField] private float verticalKick = 0.018f;

    [Header("Recoil Rotation")]
    [SerializeField] private float pitchKick = 2.2f;
    [SerializeField] private float yawKick = 0.45f;
    [SerializeField] private float rollKick = 0.3f;

    [Header("Recovery")]
    [SerializeField] private float positionRecovery = 7f;
    [SerializeField] private float rotationRecovery = 6f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 currentPositionKick;
    private Vector3 targetPositionKick;
    private Vector3 positionVelocity;

    private Vector3 currentRotationKick;
    private Vector3 targetRotationKick;
    private Vector3 rotationVelocity;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public void AddRecoil(float strength, float horizontalDirection)
    {
        targetPositionKick += new Vector3(
            horizontalDirection * kickBack * 0.25f,
            verticalKick * strength,
            -kickBack * strength
        );

        targetRotationKick += new Vector3(
            -pitchKick * strength,
            horizontalDirection * yawKick * strength,
            -horizontalDirection * rollKick * strength
        );

        targetPositionKick.z = Mathf.Max(targetPositionKick.z, -0.16f);
        targetRotationKick.x = Mathf.Clamp(targetRotationKick.x, -12f, 0f);
        targetRotationKick.y = Mathf.Clamp(targetRotationKick.y, -4f, 4f);
        targetRotationKick.z = Mathf.Clamp(targetRotationKick.z, -3f, 3f);
    }

    private void LateUpdate()
    {
        targetPositionKick = Vector3.Lerp(
            targetPositionKick,
            Vector3.zero,
            Time.deltaTime * positionRecovery
        );

        targetRotationKick = Vector3.Lerp(
            targetRotationKick,
            Vector3.zero,
            Time.deltaTime * rotationRecovery
        );

        currentPositionKick = Vector3.SmoothDamp(
            currentPositionKick,
            targetPositionKick,
            ref positionVelocity,
            0.06f
        );

        currentRotationKick = Vector3.SmoothDamp(
            currentRotationKick,
            targetRotationKick,
            ref rotationVelocity,
            0.07f
        );

        transform.localPosition = initialPosition + currentPositionKick;
        transform.localRotation = initialRotation *
            Quaternion.Euler(currentRotationKick);
    }
}
