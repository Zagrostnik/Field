using UnityEngine;

namespace FieldMission
{
    public interface IThreat
    {
        Transform ThreatTransform { get; }
        bool IsActiveThreat { get; }
    }
}
