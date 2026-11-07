using System.Collections;
using UnityEngine;

namespace FieldMission
{
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform weaponVisual;
        [SerializeField] private Animator weaponAnimator;
        [SerializeField] private AudioSource weaponAudio;
        [SerializeField] private AudioClip shotClip, reloadClip, emptyClip;
        [SerializeField] private ParticleSystem muzzleFlash;
        [Header("Weapon")]
        [SerializeField] private int magazineSize = 30;
        [SerializeField] private int reserveAmmo = 120;
        [SerializeField] private float damage = 35f;
        [SerializeField] private float range = 180f;
        [SerializeField] private float roundsPerMinute = 650f;
        [SerializeField] private float reloadDuration = 2.3f;
        [SerializeField] private LayerMask hitMask = ~0;
        [Header("Recoil")]
        [SerializeField] private float recoilKick = 2.2f;
        [SerializeField] private float recoilReturn = 8f;
        [SerializeField] private float aimSensitivityMultiplier = 0.65f;

        private int ammo;
        private float nextShotTime;
        private bool reloading;
        private Vector3 initialWeaponPosition;
        private Quaternion initialWeaponRotation;
        private float recoil;
        private IWeaponFireStrategy fireStrategy;

        public int Ammo => ammo;
        public int ReserveAmmo => reserveAmmo;

        private void Awake()
        {
            ammo = magazineSize;
            fireStrategy = new HitscanFireStrategy();
            if (weaponVisual != null) { initialWeaponPosition = weaponVisual.localPosition; initialWeaponRotation = weaponVisual.localRotation; }
            PublishAmmo();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != MissionState.Playing || reloading) return;

            if (Input.GetKeyDown(KeyCode.R)) { StartCoroutine(Reload()); return; }
            if (Input.GetMouseButton(0)) TryFire();
            if (Input.GetMouseButtonDown(1) && weaponAnimator != null) weaponAnimator.SetBool("Aiming", true);
            if (Input.GetMouseButtonUp(1) && weaponAnimator != null) weaponAnimator.SetBool("Aiming", false);

            recoil = Mathf.Lerp(recoil, 0f, Time.deltaTime * recoilReturn);
            if (weaponVisual != null)
            {
                weaponVisual.localPosition = Vector3.Lerp(weaponVisual.localPosition, initialWeaponPosition + Vector3.back * recoil * 0.012f, Time.deltaTime * 12f);
                weaponVisual.localRotation = Quaternion.Slerp(weaponVisual.localRotation, initialWeaponRotation * Quaternion.Euler(-recoil * 1.7f, 0, 0), Time.deltaTime * 12f);
            }
        }

        private void TryFire()
        {
            if (Time.time < nextShotTime) return;
            if (ammo <= 0)
            {
                nextShotTime = Time.time + 0.25f;
                if (weaponAudio != null && emptyClip != null) weaponAudio.PlayOneShot(emptyClip);
                return;
            }

            nextShotTime = Time.time + 60f / roundsPerMinute;
            ammo--;
            fireStrategy.Fire(playerCamera.transform, damage, range, hitMask);
            recoil = Mathf.Min(recoil + recoilKick, 7f);
            if (muzzleFlash != null) muzzleFlash.Play();
            if (weaponAudio != null && shotClip != null) weaponAudio.PlayOneShot(shotClip);
            if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");
            GameEvents.RaiseShotFired();
            PublishAmmo();
        }

        private IEnumerator Reload()
        {
            if (ammo == magazineSize || reserveAmmo <= 0) yield break;
            reloading = true;
            GameEvents.RaiseReloadStarted();
            if (weaponAnimator != null) weaponAnimator.SetTrigger("Reload");
            if (weaponAudio != null && reloadClip != null) weaponAudio.PlayOneShot(reloadClip);
            yield return new WaitForSeconds(reloadDuration);
            int needed = magazineSize - ammo;
            int loaded = Mathf.Min(needed, reserveAmmo);
            ammo += loaded;
            reserveAmmo -= loaded;
            reloading = false;
            PublishAmmo();
        }

        private void PublishAmmo() => GameEvents.RaiseAmmoChanged(ammo, reserveAmmo);
    }
}
