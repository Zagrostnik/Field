
using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class ArtilleryDirector : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform player;
        [SerializeField] private float dangerRadiusAroundPlayer = 50f;

        [Header("Timing")]
        [SerializeField] private float firstStrikeMinDelay = 8f;
        [SerializeField] private float firstStrikeMaxDelay = 16f;
        [SerializeField] private float minInterval = 35f;
        [SerializeField] private float maxInterval = 65f;
        [SerializeField] private float launchToWhistleDelay = 1.5f;
        [SerializeField] private float whistleDuration = 4.5f;

        [Header("Damage")]
        [SerializeField] private float blastRadius = 18f;
        [SerializeField] private float lethalRadiusStanding = 8f;
        [SerializeField] private float lethalRadiusProne = 2.2f;
        [SerializeField] private float maxDamage = 100f;
        [SerializeField] private float proneDamageMultiplier = 0.35f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float raycastHeight = 200f;

        [Header("Effects")]
        [SerializeField] private GameObject explosionPrefab;

        [Header("Audio")]
        [SerializeField] private AudioSource artilleryLaunchAudio;
        [SerializeField] private AudioClip artilleryLaunchClip;

        [SerializeField] private AudioSource warningAudio;
        [SerializeField] private AudioClip incomingWhistle;

        [SerializeField] private AudioSource explosionAudio;
        [SerializeField] private AudioClip explosionClip;

        [Header("Player")]
        [SerializeField] private PlayerMovement playerMovement;

        private IEnumerator Start()
        {
            // Ждём начала миссии
            yield return new WaitUntil(() =>
                GameManager.Instance != null &&
                GameManager.Instance.State == MissionState.Playing
            );

            yield return new WaitForSeconds(
                Random.Range(firstStrikeMinDelay, firstStrikeMaxDelay)
            );

            while (GameManager.Instance != null &&
                   GameManager.Instance.State == MissionState.Playing)
            {
                yield return StartCoroutine(ArtilleryStrike());

                yield return new WaitForSeconds(
                    Random.Range(minInterval, maxInterval)
                );
            }
        }

        private IEnumerator ArtilleryStrike()
        {
            if (player == null)
                yield break;

            // Выбираем случайную точку вокруг игрока
            Vector2 offset =
                Random.insideUnitCircle * dangerRadiusAroundPlayer;

            Vector3 targetPosition = player.position +
                new Vector3(offset.x, 0f, offset.y);

            // Определяем точку попадания на земле
            Vector3 rayOrigin =
                targetPosition + Vector3.up * raycastHeight;

            if (!Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                raycastHeight * 2f,
                groundMask,
                QueryTriggerInteraction.Ignore))
            {
                Debug.LogWarning("Артиллерия: не удалось найти поверхность земли.");
                yield break;
            }

            Vector3 impactPosition = hit.point;

            // 1. Звук выстрела артиллерии вдали
            if (artilleryLaunchAudio != null &&
                artilleryLaunchClip != null)
            {
                artilleryLaunchAudio.PlayOneShot(artilleryLaunchClip);
            }

            yield return new WaitForSeconds(launchToWhistleDelay);

            if (!IsMissionPlaying())
                yield break;

            // 2. Свист приближающегося снаряда
            if (warningAudio != null && incomingWhistle != null)
            {
                warningAudio.PlayOneShot(incomingWhistle);
            }

            yield return new WaitForSeconds(whistleDuration);

            if (!IsMissionPlaying())
                yield break;

            // 3. Взрыв
            if (explosionPrefab != null)
            {
                Instantiate(
                    explosionPrefab,
                    impactPosition,
                    Quaternion.identity
                );
            }

            if (explosionAudio != null && explosionClip != null)
            {
                explosionAudio.PlayOneShot(explosionClip);
            }

            // 4. Расчёт расстояния до игрока
            Vector3 playerPosition = player.position;
            playerPosition.y = 0f;

            Vector3 flatImpact = impactPosition;
            flatImpact.y = 0f;

            float distance = Vector3.Distance(
                playerPosition,
                flatImpact
            );

            if (distance > blastRadius)
                yield break;

            // 5. Проверяем, лежит ли игрок
            bool isProne =
                playerMovement != null &&
                playerMovement.IsProne;

            float lethalRadius = isProne
                ? lethalRadiusProne
                : lethalRadiusStanding;

            // 6. Рассчитываем урон
            float damage;

            if (distance <= lethalRadius)
            {
                damage = maxDamage;
            }
            else
            {
                float falloff = 1f - distance / blastRadius;
                damage = maxDamage * falloff;
            }

            // Лежачий игрок получает меньше урона
            if (isProne)
            {
                damage *= proneDamageMultiplier;
            }

            PlayerHealth health =
                player.GetComponentInParent<PlayerHealth>();

            if (health != null && damage > 0f)
            {
                health.TakeDamage(damage);
            }

            Debug.Log(
                $"Артиллерия: расстояние {distance:F1} м, " +
                $"урон {damage:F1}, лежит: {isProne}"
            );
        }

        private bool IsMissionPlaying()
        {
            return GameManager.Instance != null &&
                   GameManager.Instance.State == MissionState.Playing;
        }
    }
}
