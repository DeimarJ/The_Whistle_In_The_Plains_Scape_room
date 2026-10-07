using UnityEngine;

public class LastPhase : MonoBehaviour
{
    [SerializeField] private float penaltyPercent = 0.9f; // 90% de reducción

    private PickupObject pickup;

    [SerializeField] private GameObject victoryTrigger;

    private void Awake()
    {
        pickup = GetComponent<PickupObject>();
    }

    private void OnEnable()
    {
        pickup.OnPickedUp += ApplyPenalty;
    }

    private void OnDisable()
    {
        pickup.OnPickedUp -= ApplyPenalty;
    }

    private void ApplyPenalty()
    {
        victoryTrigger = GameObject.FindWithTag("Victory");
        victoryTrigger.GetComponent<Collider>().enabled = true;

        ConsumableLight[] lights = FindObjectsByType<ConsumableLight>(FindObjectsSortMode.None);

        foreach (ConsumableLight light in lights)
        {
            light.ReduceTime(penaltyPercent);
        }
    }


}
