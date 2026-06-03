using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DynamicScreen : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private GameObject defaultFocusObject;

    public void Init()
    {
        CustomInit();
    }

    public void Open()
    {
        gameObject.SetActive(true);

        UIFocusManager.Instance?.RegisterScreen(this);

        CustomOpen();
    }

    public void Close()
    {
        CustomClose();

        UIFocusManager.Instance?.UnregisterScreen(this);

        gameObject.SetActive(false);
    }

    public virtual void FocusDefaultObject()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (defaultFocusObject == null)
            return;

        Canvas.ForceUpdateCanvases();

        EventSystem.current.SetSelectedGameObject(defaultFocusObject);
    }
    public virtual void OnCancel()
    {
        Close();
    }
    protected abstract void CustomInit();
    protected abstract void CustomOpen();
    protected abstract void CustomClose();
}