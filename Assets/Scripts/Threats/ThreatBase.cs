using UnityEngine;

namespace FieldMission
{
    public abstract class ThreatBase : MonoBehaviour, IDamageable, IThreat
    {
        [SerializeField] protected float maxHealth = 100f;
        protected float health;
        protected bool activeThreat = true;
        public Transform ThreatTransform => transform;
        public bool IsActiveThreat => activeThreat;

        protected virtual void OnEnable()
        {
            health = maxHealth;
            activeThreat = true;
            GameEvents.RaiseThreatSpawned();
        }

        public virtual void TakeDamage(float amount)
        {
            if (!activeThreat) return;
            health -= amount;
            if (health <= 0f) DestroyThreat();
        }

        protected virtual void DestroyThreat()
        {
            if (!activeThreat) return;
            activeThreat = false;
            GameEvents.RaiseThreatDestroyed();
            Destroy(gameObject);
        }
    }
}
