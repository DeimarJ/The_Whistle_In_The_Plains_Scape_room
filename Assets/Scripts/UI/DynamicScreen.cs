using UnityEngine;

//Abstract parent class for UI Scripts
public abstract class DynamicScreen : MonoBehaviour
{
    // Initite the Screen
    public void Init()
    {
        CustomInit();
    }

    // Activate the screen
    public void Open()
    {
        gameObject.SetActive(true);
        CustomOpen();
    }

    // Deactivate the screen
    public void Close()
    {
        CustomClose();
        gameObject.SetActive(false);
    }

    // Abstract methods for children scripts
    protected abstract void CustomInit();
    protected abstract void CustomOpen();
    protected abstract void CustomClose();
}