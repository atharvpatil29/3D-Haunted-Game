using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
{
    public GameObject gate;
    public TextMeshProUGUI messageText;
    public float displayTime = 3f;
    private bool isGateOpen = false;

    void Start()
    {
        if (gate != null)
            gate.SetActive(true);
        if (messageText != null)
            messageText.text = "";
    }

    public void OpenGate()
    {
        if (!isGateOpen)
        {
            isGateOpen = true;
            gate.SetActive(false);
            ShowMessage("Gate Opened");
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            Invoke("ClearMessage", displayTime);
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}
