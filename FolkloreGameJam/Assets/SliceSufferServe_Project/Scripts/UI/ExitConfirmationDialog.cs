using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExitConfirmationDialog : MonoBehaviour
{
    private const string DialogResourcePath = "ExitConfirmationDialog";

    private static ExitConfirmationDialog currentDialog;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button cancelButton;

    private Action onConfirm;

    public static void Show(Action confirmAction)
    {
        if (currentDialog != null)
        {
            return;
        }

        ExitConfirmationDialog prefab = Resources.Load<ExitConfirmationDialog>(DialogResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"Could not find exit confirmation prefab at Resources/{DialogResourcePath}.prefab.");
            return;
        }

        EnsureEventSystem();

        currentDialog = Instantiate(prefab);
        currentDialog.name = DialogResourcePath;
        currentDialog.onConfirm = confirmAction;
        DontDestroyOnLoad(currentDialog.gameObject);
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(Confirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Cancel);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Confirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(Cancel);
        }

        if (currentDialog == this)
        {
            currentDialog = null;
        }
    }

    private void Confirm()
    {
        Action confirmAction = onConfirm;
        Close();
        confirmAction?.Invoke();
    }

    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
