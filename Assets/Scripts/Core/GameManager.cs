using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FieldMission
{
    public enum MissionState { Menu, Playing, PlayerDead, Completed }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static event Action<MissionState> StateChanged;
        public static event Action<float> MissionProgressChanged;

        [SerializeField] private MissionState initialState = MissionState.Menu;
        [SerializeField] private Transform player;
        [SerializeField] private Transform missionGoal;
        private MissionState state;

        public MissionState State => state;
        public Transform Player => player;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            state = initialState;
        }

        private void Start()
        {
            if (state == MissionState.Playing) BeginMission();
            else SetCursor(true);
        }

        public void SetPlayer(Transform value) => player = value;
        public void SetGoal(Transform value) => missionGoal = value;

        public void BeginMission()
        {
            Time.timeScale = 1f;
            SetState(MissionState.Playing);
            SetCursor(false);
        }

        public void PlayerDied()
        {
            if (state != MissionState.Playing) return;
            SetState(MissionState.PlayerDead);
            SetCursor(true);
        }

        public void CompleteMission()
        {
            if (state != MissionState.Playing) return;
            SetState(MissionState.Completed);
            SetCursor(true);
        }

        public void ReportProgress(float normalized)
        {
            MissionProgressChanged?.Invoke(Mathf.Clamp01(normalized));
        }

        public void RestartMission()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu(string sceneName = "MainMenu")
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        private void SetState(MissionState value)
        {
            state = value;
            StateChanged?.Invoke(state);
        }

        private static void SetCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
