using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class DroneSpawner : MonoBehaviour
    {
        [SerializeField] private DroneFactory factory;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float firstAttackDelay = 18f;
        [SerializeField] private float minInterval = 25f;
        [SerializeField] private float maxInterval = 45f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(firstAttackDelay);
            while (GameManager.Instance != null && GameManager.Instance.State == MissionState.Playing)
            {
                if (factory != null && player != null && spawnPoints != null && spawnPoints.Length > 0)
                {
                    Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    factory.Create(point, player);
                }
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            }
        }
    }
}
