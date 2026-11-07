using UnityEngine;

namespace FieldMission
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float sprintSpeed = 5.2f;
        [SerializeField] private float crouchSpeed = 1.7f;
        [SerializeField] private float proneSpeed = 0.8f;
        [SerializeField] private float gravity = -22f;
        [Header("Posture")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.15f;
        [SerializeField] private float proneHeight = 0.55f;
        [SerializeField] private float postureLerp = 8f;
        [Header("Audio")]
        [SerializeField] private AudioSource footsteps;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float stepInterval = 0.48f;

        private CharacterController controller;
        private Vector3 verticalVelocity;
        private float stepTimer;
        private float targetHeight;
        private float cameraStandY;
        private float cameraTargetY;
        private bool crouching, prone;

        public bool IsProne => prone;
        public bool IsCrouching => crouching;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            targetHeight = standingHeight;
            cameraStandY = cameraPivot.localPosition.y;
            cameraTargetY = cameraStandY;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != MissionState.Playing) return;

            if (Input.GetKeyDown(KeyCode.C)) { crouching = !crouching; if (crouching) prone = false; }
            if (Input.GetKeyDown(KeyCode.LeftControl)) { prone = !prone; if (prone) crouching = false; }

            targetHeight = prone ? proneHeight : crouching ? crouchHeight : standingHeight;
            cameraTargetY = cameraStandY - (standingHeight - targetHeight) * 0.82f;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * postureLerp);
            controller.center = new Vector3(0, controller.height * 0.5f, 0);
            Vector3 camPos = cameraPivot.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, cameraTargetY, Time.deltaTime * postureLerp);
            cameraPivot.localPosition = camPos;

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1f);
            bool sprinting = Input.GetKey(KeyCode.LeftShift) && !crouching && !prone && z > 0.1f;
            float speed = prone ? proneSpeed : crouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
            Vector3 move = transform.TransformDirection(input) * speed;

            if (controller.isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;
            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move((move + verticalVelocity) * Time.deltaTime);

            bool moving = input.sqrMagnitude > 0.08f && controller.isGrounded;
            if (moving)
            {
                stepTimer += Time.deltaTime * (sprinting ? 1.35f : 1f) * (prone ? 0.65f : 1f);
                if (stepTimer >= stepInterval) { PlayFootstep(); stepTimer = 0f; }
            }
            else stepTimer = 0f;
        }

        private void PlayFootstep()
        {
            if (footsteps == null || footstepClips == null || footstepClips.Length == 0) return;
            footsteps.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
        }
    }
}
