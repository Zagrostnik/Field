using UnityEngine;

namespace FieldMission
{
    public class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float maxLookAngle = 85f;
        private float pitch;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != MissionState.Playing) return;
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch - mouseY, -maxLookAngle, maxLookAngle);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }
}
