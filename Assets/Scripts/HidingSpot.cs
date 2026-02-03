using UnityEngine;
using TMPro;

public class HidingSpot : MonoBehaviour
{
    public GameObject blackoutPanel;      // UI blackout panel to show when hiding
    public TextMeshProUGUI hidingText;    // "You are hiding" text UI

    private bool playerInRange = false;
    private GameObject player;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!PlayerHiding.isHiding)
                EnterHiding();
            else
                ExitHiding();
        }
    }

    private void EnterHiding()
    {
        PlayerHiding.isHiding = true;

        if (player != null)
            player.SetActive(false);     // Disable player game object

        blackoutPanel.SetActive(true);    // Show blackout
        hidingText.gameObject.SetActive(true);  // Show hiding text
    }

    private void ExitHiding()
    {
        PlayerHiding.isHiding = false;

        if (player != null)
            player.SetActive(true);      // Enable player game object

        blackoutPanel.SetActive(false);   // Hide blackout
        hidingText.gameObject.SetActive(false); // Hide hiding text
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }
}