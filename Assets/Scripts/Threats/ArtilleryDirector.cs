using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class ArtilleryDirector : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform player;
        [SerializeField] private float dangerRadiusAroundPlayer = 50f;
        [SerializeField] private float lethalRadiusStanding = 8f;
        [SerializeField] private float lethalRadiusProne = 2.2f;
        [SerializeField] private float minInterval = 35f;
        [SerializeField] private float maxInterval = 65f;
        [SerializeField] private float whistleDuration = 4.5f;
        [Header("Effects")]
        [SerializeField] private AudioSource warningAudio;
        [SerializeField] private AudioClip incomingWhistle;
        [SerializeField] private AudioSource explosionAudio;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private PlayerMovement playerMovement;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(Random.Range(8f, 16f));
            while (GameManager.Instance != null && GameManager.Instance.State == MissionState.Playing)
            {
                yield return StartCoroutine(ArtilleryStrike());
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            }
        }

        private IEnumerator ArtilleryStrike()
        {
            if (player == null) yield break;
            Vector2 offset = Random.insideUnitCircle * dangerRadiusAroundPlayer;
            Vector3 impact = player.position + new Vector3(offset.x, 0f, offset.y);
            if (warningAudio != null && incomingWhistle != null) warningAudio.PlayOneShot(incomingWhistle);
            yield return new WaitForSeconds(whistleDuration);

            if (explosionPrefab != null) Instantiate(explosionPrefab, impact, Quaternion.identity);
            if (explosionAudio != null && explosionClip != null) explosionAudio.PlayOneShot(explosionClip);

            float distance = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(impact.x, 0, impact.z));
            bool isProne = playerMovement != null && playerMovement.IsProne;
            float lethalRadius = isProne ? lethalRadiusProne : lethalRadiusStanding;
            if (distance <= lethalRadius)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(100f);
            }
        }
    }
}
