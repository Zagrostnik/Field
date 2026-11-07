using UnityEngine;

namespace FieldMission
{
    [RequireComponent(typeof(Collider))]
    public class MissionGoal : MonoBehaviour
    {
        private void Reset() { GetComponent<Collider>().isTrigger = true; }
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerMovement>() != null && GameManager.Instance != null)
                GameManager.Instance.CompleteMission();
        }
    }
}
