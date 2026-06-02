using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class DynamicButton : MonoBehaviour
{
    [SerializeField] private Button button;

    [Header("Audio")]
    [SerializeField] private UIClipType clickSound = UIClipType.ButtonClick1;

    private void Reset()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    /// <summary>
    /// Sets button callback
    /// </summary>
    public void SetOnClick(Action callback)
    {
        if (button == null)
        {
            Debug.LogError("No button reference in Dynamic Button");
            return;
        }

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUI(clickSound);

            callback?.Invoke();
        });
    }
}