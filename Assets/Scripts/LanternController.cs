using UnityEngine;

public class LanternController : MonoBehaviour
{
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject lanternVisual;

    public bool IsOn { get; private set; }

    public void SetLanternState(bool state)
    {
        IsOn = state;

        if (lanternLight != null)
        {
            lanternLight.enabled = state;
        }

        if (lanternVisual != null)
        {
            lanternVisual.SetActive(state);
        }
    }
}