using UnityEngine;

namespace FieldMission
{
    public class FPVDrone : ThreatBase
    {
        [SerializeField] private float flySpeed = 12f;
        [SerializeField] private float turnSpeed = 4f;
        [SerializeField] private float detonationDistance = 2.2f;
        [SerializeField] private float impactDamage = 100f;
        [SerializeField] private AudioSource droneAudio;
        [SerializeField] private GameObject explosionPrefab;
        private Transform target;

        public void Initialize(Transform player) { target = player; }

        private void Update()
        {
            if (!activeThreat || target == null || GameManager.Instance == null || GameManager.Instance.State != MissionState.Playing) return;
            Vector3 direction = (target.position + Vector3.up * 0.8f - transform.position).normalized;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            transform.position += transform.forward * flySpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, target.position + Vector3.up * 0.8f) <= detonationDistance)
            {
                PlayerHealth health = target.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(impactDamage);
                if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                DestroyThreat();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (droneAudio != null && !droneAudio.isPlaying) droneAudio.Play();
        }
    }
}
