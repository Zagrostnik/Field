
using FieldMission;
using UnityEngine;

public class AimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Aim Settings")]
    [SerializeField] private float aimSpeed = 10f;
    [SerializeField] private float normalFOV = 75f;
    [SerializeField] private float aimFOV = 55f;

    private Vector3 normalPosition;
    private Quaternion normalRotation;

    private bool isAiming;

    public bool IsAiming => isAiming;

    private void Start()
    {
        normalPosition = weaponHolder.localPosition;
        normalRotation = weaponHolder.localRotation;

        playerCamera.fieldOfView = normalFOV;
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != MissionState.Playing)
            return;

        isAiming = Input.GetMouseButton(1);

        Vector3 targetPosition = normalPosition;
        Quaternion targetRotation = normalRotation;
        float targetFOV = normalFOV;

        if (isAiming && aimPoint != null)
        {
            Vector3 localAimPosition =
                weaponHolder.InverseTransformPoint(aimPoint.position);

            targetPosition = normalPosition - localAimPosition;
            targetFOV = aimFOV;
        }

        weaponHolder.localPosition = Vector3.Lerp(
            weaponHolder.localPosition,
            targetPosition,
            Time.deltaTime * aimSpeed
        );

        weaponHolder.localRotation = Quaternion.Slerp(
            weaponHolder.localRotation,
            targetRotation,
            Time.deltaTime * aimSpeed
        );

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * aimSpeed
        );
    }
}
