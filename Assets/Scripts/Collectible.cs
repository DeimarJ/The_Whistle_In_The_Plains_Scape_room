using UnityEngine;

public class Collectible : MonoBehaviour,IInteractuable
{
    [SerializeField] private GameObject outlineObject;
    public string InteractionText => "Presiona [E] para recoger";
    public void Highlight(bool state)
    {
        
        if (outlineObject == null)
            return;
        outlineObject.SetActive(state);

    }

    public void Interact()
    {
        Debug.Log("Coleccionado");
    }
}
