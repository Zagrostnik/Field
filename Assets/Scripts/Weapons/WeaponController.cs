
using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private AudioSource weaponAudio;
        [SerializeField] private AudioClip shotClip;
        [SerializeField] private AudioClip reloadClip;
        [SerializeField] private AudioClip emptyClip;

        [Header("Weapon Settings")]
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 120;
        [SerializeField] private float damage = 35f;
        [SerializeField] private float range = 200f;
        [SerializeField] private float fireRate = 650f;

        [Header("Recoil")]
        [SerializeField] private float recoilAmount = 2f;
        [SerializeField] private float recoilRecovery = 8f;

        [Header("Reload - Arma Style")]
        [SerializeField] private float lowerDuration = 0.35f;
        [SerializeField] private float reloadPause = 1.2f;
        [SerializeField] private float raiseDuration = 0.4f;
        [SerializeField] private float loweredPosition = -0.45f;

        [Header("Shooting")]
        [SerializeField] private LayerMask hitMask = ~0;

        private int currentAmmo;
        private float nextShotTime;
        private float currentRecoil;
        private bool isReloading;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            currentAmmo = magazineSize;

            initialPosition = weaponHolder.localPosition;
            initialRotation = weaponHolder.localRotation;
        }

        private void Update()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.State != MissionState.Playing)
                return;

            if (Input.GetKeyDown(KeyCode.R))
                StartCoroutine(Reload());

            if (isReloading)
                return;

            if (Input.GetMouseButton(0))
                Shoot();

            currentRecoil = Mathf.Lerp(
                currentRecoil,
                0f,
                Time.deltaTime * recoilRecovery
            );

            weaponHolder.localRotation = Quaternion.Slerp(
                weaponHolder.localRotation,
                initialRotation * Quaternion.Euler(-currentRecoil, 0, 0),
                Time.deltaTime * 10f
            );
        }

        private void Shoot()
        {
            if (Time.time < nextShotTime)
                return;

            if (currentAmmo <= 0)
            {
                nextShotTime = Time.time + 0.25f;

                if (weaponAudio && emptyClip)
                    weaponAudio.PlayOneShot(emptyClip);

                return;
            }

            nextShotTime = Time.time + 60f / fireRate;
            currentAmmo--;

            if (Physics.Raycast(
                playerCamera.transform.position,
                playerCamera.transform.forward,
                out RaycastHit hit,
                range,
                hitMask))
            {
                IDamageable target =
                    hit.collider.GetComponentInParent<IDamageable>();

                if (target != null)
                    target.TakeDamage(damage);
            }

            currentRecoil = Mathf.Min(
                currentRecoil + recoilAmount,
                12f
            );

            if (weaponAudio && shotClip)
                weaponAudio.PlayOneShot(shotClip);

            GameEvents.RaiseShotFired();
            GameEvents.RaiseAmmoChanged(currentAmmo, reserveAmmo);
        }

        private IEnumerator Reload()
        {
            if (isReloading ||
                currentAmmo == magazineSize ||
                reserveAmmo <= 0)
                yield break;

            isReloading = true;

            GameEvents.RaiseReloadStarted();

            if (weaponAudio && reloadClip)
                weaponAudio.PlayOneShot(reloadClip);

            Vector3 loweredPositionVector =
                initialPosition + Vector3.down * Mathf.Abs(loweredPosition);

            // Опускаем автомат
            yield return MoveWeapon(
                initialPosition,
                loweredPositionVector,
                initialRotation,
                Quaternion.Euler(15f, 0f, 0f) * initialRotation,
                lowerDuration
            );

            // Имитация смены магазина
            yield return new WaitForSeconds(reloadPause);

            // Возвращаем автомат
            yield return MoveWeapon(
                loweredPositionVector,
                initialPosition,
                Quaternion.Euler(15f, 0f, 0f) * initialRotation,
                initialRotation,
                raiseDuration
            );

            int needed = magazineSize - currentAmmo;
            int loaded = Mathf.Min(needed, reserveAmmo);

            currentAmmo += loaded;
            reserveAmmo -= loaded;

            GameEvents.RaiseAmmoChanged(currentAmmo, reserveAmmo);

            isReloading = false;
        }

        private IEnumerator MoveWeapon(
            Vector3 fromPosition,
            Vector3 toPosition,
            Quaternion fromRotation,
            Quaternion toRotation,
            float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);

                weaponHolder.localPosition =
                    Vector3.Lerp(fromPosition, toPosition, t);

                weaponHolder.localRotation =
                    Quaternion.Slerp(fromRotation, toRotation, t);

                yield return null;
            }

            weaponHolder.localPosition = toPosition;
            weaponHolder.localRotation = toRotation;
        }
    }
}
