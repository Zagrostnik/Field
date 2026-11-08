
using UnityEngine;

namespace FieldMission
{
    public class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float maxLookAngle = 85f;

        [Header("Camera Recoil")]
        [SerializeField] private float recoilRecovery = 5f;
        [SerializeField] private float recoilSnappiness = 12f;

        private float pitch;

        private Vector2 recoil;
        private Vector2 recoilTarget;
        private Vector2 recoilVelocity;

        public void AddRecoil(float vertical, float horizontal)
        {
            recoilTarget.x += vertical;
            recoilTarget.y += horizontal;

            recoilTarget.x = Mathf.Clamp(recoilTarget.x, 0f, 15f);
            recoilTarget.y = Mathf.Clamp(recoilTarget.y, -5f, 5f);
        }

        private void Update()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.State != MissionState.Playing)
                return;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            pitch = Mathf.Clamp(
                pitch - mouseY,
                -maxLookAngle,
                maxLookAngle
            );

            transform.Rotate(Vector3.up * mouseX);
        }

        private void LateUpdate()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.State != MissionState.Playing)
                return;

            recoilTarget = Vector2.Lerp(
                recoilTarget,
                Vector2.zero,
                Time.deltaTime * recoilRecovery
            );

            recoil = Vector2.SmoothDamp(
                recoil,
                recoilTarget,
                ref recoilVelocity,
                1f / recoilSnappiness
            );

            cameraPivot.localRotation = Quaternion.Euler(
                pitch - recoil.x,
                recoil.y,
                0f
            );
        }
    }
}
