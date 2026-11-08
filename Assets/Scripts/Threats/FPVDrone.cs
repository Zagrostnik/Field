
using UnityEngine;

namespace FieldMission
{
    public class FPVDrone : ThreatBase
    {
        [Header("Flight")]
        [SerializeField] private float flySpeed = 12f;
        [SerializeField] private float turnSpeed = 5f;
        [SerializeField] private float detonationDistance = 2.2f;
        [SerializeField] private float targetHeight = 0.8f;

        [Header("Damage")]
        [SerializeField] private float impactDamage = 100f;

        [Header("Audio")]
        [SerializeField] private AudioSource droneAudio;

        [Header("Explosion")]
        [SerializeField] private GameObject explosionPrefab;

        private Transform target;
        private bool exploded;

        public void Initialize(Transform player)
        {
            target = player;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            exploded = false;

            if (droneAudio != null && !droneAudio.isPlaying)
                droneAudio.Play();
        }

        private void Update()
        {
            if (!activeThreat || target == null)
                return;

            if (GameManager.Instance == null ||
                GameManager.Instance.State != MissionState.Playing)
                return;

            Vector3 targetPosition = target.position +
                                     Vector3.up * targetHeight;

            Vector3 direction = targetPosition - transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction.normalized);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
            }

            transform.position += transform.forward *
                                  flySpeed * Time.deltaTime;

            float distance = Vector3.Distance(
                transform.position,
                targetPosition
            );

            if (distance <= detonationDistance)
                Detonate();
        }

        private void Detonate()
        {
            if (!activeThreat || exploded)
                return;

            exploded = true;

            PlayerHealth health =
                target.GetComponentInParent<PlayerHealth>();

            if (health != null)
                health.TakeDamage(impactDamage);

            DestroyThreat();
        }

        protected override void DestroyThreat()
        {
            if (!activeThreat)
                return;

            if (!exploded)
            {
                exploded = true;
                CreateExplosion();
            }
            else
            {
                CreateExplosion();
            }

            base.DestroyThreat();
        }

        private void CreateExplosion()
        {
            if (explosionPrefab != null)
            {
                Instantiate(
                    explosionPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }
        }
    }
}
