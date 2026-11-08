using UnityEngine;
using System.Collections;

namespace FieldMission
{
    public class MuzzleFlash : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light muzzleLight;
        [SerializeField] private ParticleSystem muzzleParticles;

        [Header("Settings")]
        [SerializeField] private float flashDuration = 0.05f;

        private Coroutine flashCoroutine;

        private void Awake()
        {
            if (muzzleLight != null)
                muzzleLight.enabled = false;

            if (muzzleParticles != null)
                muzzleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            if (muzzleParticles != null)
                muzzleParticles.Play();

            if (muzzleLight != null)
            {
                muzzleLight.enabled = true;
                flashCoroutine = StartCoroutine(DisableLight());
            }
        }

        private IEnumerator DisableLight()
        {
            yield return new WaitForSeconds(flashDuration);

            if (muzzleLight != null)
                muzzleLight.enabled = false;

            flashCoroutine = null;
        }
    }
}