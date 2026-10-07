using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private GameObject victory;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            victory.SetActive(true);
        }
    }
}
