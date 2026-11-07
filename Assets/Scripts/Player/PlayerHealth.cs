using UnityEngine;

namespace FieldMission
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        private float health;
        public float Health => health;

        private void Awake() { health = maxHealth; }

        public void TakeDamage(float amount)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != MissionState.Playing) return;
            health -= amount;
            GameEvents.RaisePlayerDamaged(amount);
            if (health <= 0f) GameManager.Instance.PlayerDied();
        }
    }
}
