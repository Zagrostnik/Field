using UnityEngine;

namespace FieldMission
{
    public class HitscanFireStrategy : IWeaponFireStrategy
    {
        public void Fire(Transform origin, float damage, float range, LayerMask hitMask)
        {
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, range, hitMask))
            {
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null) target.TakeDamage(damage);
            }
        }
    }
}
