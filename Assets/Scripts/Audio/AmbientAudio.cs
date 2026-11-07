using UnityEngine;

namespace FieldMission
{
    public class AmbientAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource wind;
        [SerializeField] private AudioSource distantGunfire;
        [SerializeField] private float minGunfireDelay = 7f;
        [SerializeField] private float maxGunfireDelay = 18f;

        private void Start()
        {
            if (wind != null && !wind.isPlaying) wind.Play();
            if (distantGunfire != null) StartCoroutine(GunfireLoop());
        }

        private System.Collections.IEnumerator GunfireLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minGunfireDelay, maxGunfireDelay));
                if (GameManager.Instance != null && GameManager.Instance.State == MissionState.Playing)
                    distantGunfire.PlayOneShot(distantGunfire.clip);
            }
        }
    }
}
