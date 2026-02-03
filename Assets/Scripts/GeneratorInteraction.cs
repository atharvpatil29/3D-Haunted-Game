using UnityEngine;
using TMPro;

public class GeneratorInteraction : MonoBehaviour
{
    [Header("Repair Settings")]
    public float repairTime = 20f;
    private float repairProgress = 0f;
    private bool isFixed = false;

    [Header("Visual Feedback")]
    public Renderer generatorRenderer;
    public Color fixedColor = Color.green;

    [Header("UI")]
    public TextMeshProUGUI repairTimerText;

    private bool isPlayerNearby = false;
    private bool isRepairing = false;
    private float timeSinceStoppedRepairing = 0f;

    void Start()
    {
        if (repairTimerText == null)
        {
            Debug.LogError("RepairTimerText is not assigned on " + gameObject.name);
        }
    }

    void Update()
    {
        if (isFixed) return;

        if (isPlayerNearby)
        {
            if (Input.GetKey(KeyCode.E))
            {
                StartRepair();
            }
            else
            {
                ShowPrompt(); // Player near but not pressing E
                StopRepair();
            }
        }
        else
        {
            StopRepair();
        }

        HandleProgressDecay();
    }

    private void StartRepair()
    {
        if (!isRepairing)
        {
            isRepairing = true;
            timeSinceStoppedRepairing = 0f;
        }

        repairProgress += Time.deltaTime;

        if (repairTimerText != null)
        {
            repairTimerText.text = $"Repairing: {Mathf.Ceil(repairTime - repairProgress)}s";
        }

        if (repairProgress >= repairTime)
        {
            CompleteRepair();
        }
    }

    private void ShowPrompt()
    {
        if (repairTimerText != null)
        {
            repairTimerText.text = "Hold E to Repair";
        }
    }

    private void StopRepair()
    {
        if (isRepairing)
        {
            isRepairing = false;
            timeSinceStoppedRepairing = 0f;
        }
    }

    private void HandleProgressDecay()
    {
        if (!isRepairing && repairProgress > 0f)
        {
            timeSinceStoppedRepairing += Time.deltaTime;

            if (timeSinceStoppedRepairing >= 5f)
            {
                repairProgress = Mathf.Max(0f, repairProgress - Time.deltaTime * 2f);
            }
        }
    }

    private void CompleteRepair()
    {
        isFixed = true;

        if (generatorRenderer != null)
            generatorRenderer.material.color = fixedColor;

        if (repairTimerText != null)
            repairTimerText.text = "Generator Fixed";

        GameManager.Instance.OnGeneratorFixed();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFixed)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            isRepairing = false;
            repairTimerText.text = ".";
        }
    }
}