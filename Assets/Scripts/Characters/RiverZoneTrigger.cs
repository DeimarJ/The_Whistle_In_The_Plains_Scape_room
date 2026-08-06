using UnityEngine;

public class RiverZoneTrigger : MonoBehaviour
{
    private FirstPersonController controller;

    private void Awake()
    {
        // Busca el controlador en el objeto padre (Player)
        controller = GetComponentInParent<FirstPersonController>();
    }
    private void OnTriggerEnter(Collider other)
    {

        if (controller != null)
        {
            controller.SetSwimming(true);
            Debug.Log(true);
        }  
    }

    private void OnTriggerExit(Collider other)
    {
        if (controller != null)
            controller.SetSwimming(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision"+controller);
        if (controller != null)
        {
            controller.SetSwimming(true);
            Debug.Log(true);
        }
    }
}
