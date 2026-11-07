using UnityEngine;

namespace FieldMission
{
    public class MissionProgress : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform goalPoint;

        private void Update()
        {
            if (player == null || startPoint == null || goalPoint == null || GameManager.Instance == null) return;
            float total = Vector3.Distance(startPoint.position, goalPoint.position);
            float remaining = Vector3.Distance(player.position, goalPoint.position);
            GameManager.Instance.ReportProgress(total <= 0.01f ? 1f : 1f - remaining / total);
        }
    }
}
