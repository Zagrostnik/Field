using UnityEngine;
using UnityEngine.SceneManagement;

namespace FieldMission
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private string missionSceneName = "Mission";
        public void Play() => SceneManager.LoadScene(missionSceneName);
        public void Quit() => Application.Quit();
    }
}
