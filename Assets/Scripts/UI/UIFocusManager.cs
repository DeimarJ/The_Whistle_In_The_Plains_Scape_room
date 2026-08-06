using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIFocusManager : MonoBehaviour
{
    public static UIFocusManager Instance { get; private set; }

    private readonly Stack<DynamicScreen> screenStack = new();
    public DynamicScreen CurrentScreen =>
    screenStack.Count > 0
        ? screenStack.Peek()
        : null;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (screenStack.Count == 0)
            return;

        if (EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        FocusTopScreen();
    }

    public void RegisterScreen(DynamicScreen screen)
    {
        if (screen == null)
            return;

        screenStack.Push(screen);

        FocusTopScreen();
    }

    public void UnregisterScreen(DynamicScreen screen)
    {
        if (screen == null)
            return;

        if (screenStack.Count > 0 && screenStack.Peek() == screen)
        {
            screenStack.Pop();
        }
        else
        {
            Stack<DynamicScreen> temp = new();

            while (screenStack.Count > 0)
            {
                DynamicScreen current = screenStack.Pop();

                if (current == screen)
                    break;

                temp.Push(current);
            }

            while (temp.Count > 0)
                screenStack.Push(temp.Pop());
        }

        FocusTopScreen();
    }

    public void FocusTopScreen()
    {
        if (EventSystem.current == null)
            return;

        if (screenStack.Count == 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        screenStack.Peek().FocusDefaultObject();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            FocusTopScreen();
    }
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            FocusTopScreen();
    }
    public void CancelTopScreen()
    {
        if (screenStack.Count == 0)
            return;

        screenStack.Peek().OnCancel();
    }
}