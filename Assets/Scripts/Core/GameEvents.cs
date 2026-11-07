using System;
using UnityEngine;

namespace FieldMission
{
    // Central event channel: UI/audio systems subscribe without hard references to gameplay systems.
    public static class GameEvents
    {
        public static event Action<int, int> AmmoChanged;
        public static event Action ShotFired;
        public static event Action ReloadStarted;
        public static event Action<float> PlayerDamaged;
        public static event Action ThreatSpawned;
        public static event Action ThreatDestroyed;

        public static void RaiseAmmoChanged(int current, int reserve) => AmmoChanged?.Invoke(current, reserve);
        public static void RaiseShotFired() => ShotFired?.Invoke();
        public static void RaiseReloadStarted() => ReloadStarted?.Invoke();
        public static void RaisePlayerDamaged(float amount) => PlayerDamaged?.Invoke(amount);
        public static void RaiseThreatSpawned() => ThreatSpawned?.Invoke();
        public static void RaiseThreatDestroyed() => ThreatDestroyed?.Invoke();
    }
}
