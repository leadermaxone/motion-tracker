using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomButtonBehaviour : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent<bool> SetStateCallback;

    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => { OnClick?.Invoke(); });
    }

    public void SetUIState(bool status)
    {
        if (SetStateCallback != null)
            SetStateCallback.Invoke(status);
    }

}
