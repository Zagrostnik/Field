using UnityEngine;

namespace FieldMission
{
    public interface IWeaponFireStrategy
    {
        void Fire(Transform origin, float damage, float range, LayerMask hitMask);
    }
}
