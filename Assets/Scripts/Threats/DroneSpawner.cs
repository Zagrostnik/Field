
using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class DroneSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DroneFactory factory;
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Spawn Settings")]
        [SerializeField] private float firstAttackDelay = 18f;
        [SerializeField] private float minInterval = 25f;
        [SerializeField] private float maxInterval = 45f;

        private IEnumerator Start()
        {
            // Ждём начала миссии
            yield return new WaitUntil(() =>
                GameManager.Instance != null &&
                GameManager.Instance.State == MissionState.Playing
            );

            // Первая атака
            yield return new WaitForSeconds(firstAttackDelay);

            while (GameManager.Instance != null &&
                   GameManager.Instance.State == MissionState.Playing)
            {
                SpawnDrone();

                float interval = Random.Range(
                    minInterval,
                    Mathf.Max(minInterval, maxInterval)
                );

                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnDrone()
        {
            if (factory == null ||
                player == null ||
                spawnPoints == null ||
                spawnPoints.Length == 0)
                return;

            Transform point = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            if (point != null)
                factory.Create(point, player);
        }
    }
}
