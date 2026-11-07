using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FieldMission
{
    public class MissionUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private Slider progressSlider;

        private void OnEnable()
        {
            GameEvents.AmmoChanged += UpdateAmmo;
            GameManager.StateChanged += UpdateState;
            GameManager.MissionProgressChanged += UpdateProgress;
        }

        private void OnDisable()
        {
            GameEvents.AmmoChanged -= UpdateAmmo;
            GameManager.StateChanged -= UpdateState;
            GameManager.MissionProgressChanged -= UpdateProgress;
        }

        private void UpdateAmmo(int current, int reserve)
        {
            if (ammoText != null) ammoText.text = $"{current:00} / {reserve:000}";
        }

        private void UpdateState(MissionState state)
        {
            if (deathPanel != null) deathPanel.SetActive(state == MissionState.PlayerDead);
            if (completePanel != null) completePanel.SetActive(state == MissionState.Completed);
        }

        private void UpdateProgress(float value)
        {
            if (progressSlider != null) progressSlider.value = value;
        }

        public void Restart() => GameManager.Instance?.RestartMission();
        public void BackToMenu() => GameManager.Instance?.ReturnToMenu();
    }
}
