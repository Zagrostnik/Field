using UnityEngine;

namespace FieldMission
{
    public class DroneFactory : MonoBehaviour
    {
        [SerializeField] private FPVDrone dronePrefab;
        public FPVDrone Create(Transform spawnPoint, Transform target)
        {
            if (dronePrefab == null || spawnPoint == null || target == null) return null;
            FPVDrone drone = Instantiate(dronePrefab, spawnPoint.position, spawnPoint.rotation);
            drone.Initialize(target);
            return drone;
        }
    }
}
