
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

        [Header("Recoil References")]
        [SerializeField] private FirstPersonLook firstPersonLook;
        [SerializeField] private WeaponRecoil weaponRecoil;
        [SerializeField] private AimController aimController;

        [Header("Weapon Settings")]
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 120;
        [SerializeField] private float damage = 35f;
        [SerializeField] private float range = 200f;
        [SerializeField] private float fireRate = 650f;

        [Header("Recoil Settings")]
        [SerializeField] private float cameraRecoil = 1.1f;
        [SerializeField] private float cameraHorizontalRecoil = 0.25f;
        [SerializeField] private float recoilVariation = 0.25f;
        [SerializeField] private float aimingRecoilMultiplier = 0.7f;

        [Header("Reload")]
        [SerializeField] private float lowerDuration = 0.35f;
        [SerializeField] private float reloadPause = 1.2f;
        [SerializeField] private float raiseDuration = 0.4f;
        [SerializeField] private float loweredPosition = -0.45f;

        [Header("Shooting")]
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private MuzzleFlash muzzleFlash;

        private int currentAmmo;
        private float nextShotTime;
        private bool isReloading;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            currentAmmo = magazineSize;

            initialPosition = weaponHolder.localPosition;
            initialRotation = weaponHolder.localRotation;

            GameEvents.RaiseAmmoChanged(currentAmmo, reserveAmmo);
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
            if (muzzleFlash != null)
                muzzleFlash.Play();
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

            ApplyRecoil();

            if (weaponAudio && shotClip)
                weaponAudio.PlayOneShot(shotClip);

            GameEvents.RaiseShotFired();
            GameEvents.RaiseAmmoChanged(currentAmmo, reserveAmmo);
        }


        //private void Shoot()
        //{
        //    if (Time.time < nextShotTime)
        //        return;

        //    if (currentAmmo <= 0)
        //    {
        //        nextShotTime = Time.time + 0.25f;

        //        if (weaponAudio && emptyClip)
        //            weaponAudio.PlayOneShot(emptyClip);

        //        return;
        //    }

        //    nextShotTime = Time.time + 60f / fireRate;
        //    currentAmmo--;

        //    Vector3 origin = playerCamera.transform.position;
        //    Vector3 direction = playerCamera.transform.forward;

        //    Vector3 endPoint = origin + direction * range;

        //    if (Physics.Raycast(
        //        origin,
        //        direction,
        //        out RaycastHit hit,
        //        range,
        //        hitMask))
        //    {
        //        endPoint = hit.point;

        //        Debug.Log(
        //            "Попадание: " + hit.collider.name +
        //            " | Точка: " + hit.point
        //        );

        //        IDamageable target =
        //            hit.collider.GetComponentInParent<IDamageable>();

        //        if (target != null)
        //            target.TakeDamage(damage);
        //    }
        //    else
        //    {
        //        Debug.Log("Выстрел: попаданий нет");
        //    }

        //    Debug.DrawLine(origin, endPoint, Color.red, 2f);


        //    if (muzzleFlash != null)
        //        muzzleFlash.Play();

        //    if (weaponAudio && shotClip)
        //        weaponAudio.PlayOneShot(shotClip);

        //    GameEvents.RaiseShotFired();
        //    GameEvents.RaiseAmmoChanged(currentAmmo, reserveAmmo);
        //}



        private void ApplyRecoil()
        {
            bool aiming = aimController != null &&
                          aimController.IsAiming;

            float multiplier = aiming
                ? aimingRecoilMultiplier
                : 1f;

            float horizontal = Random.Range(
                -cameraHorizontalRecoil,
                cameraHorizontalRecoil
            );

            float variation = Random.Range(
                1f - recoilVariation,
                1f + recoilVariation
            );

            if (firstPersonLook != null)
            {
                firstPersonLook.AddRecoil(
                    cameraRecoil * multiplier * variation,
                    horizontal * multiplier
                );
            }

            if (weaponRecoil != null)
            {
                weaponRecoil.AddRecoil(
                    multiplier * variation,
                    horizontal
                );
            }
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
                initialPosition +
                Vector3.down * Mathf.Abs(loweredPosition);

            yield return MoveWeapon(
                initialPosition,
                loweredPositionVector,
                initialRotation,
                Quaternion.Euler(15f, 0f, 0f) * initialRotation,
                lowerDuration
            );

            yield return new WaitForSeconds(reloadPause);

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
