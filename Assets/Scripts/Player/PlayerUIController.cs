using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerUIController : MonoBehaviour
{
    public NetworkPlayer networkPlayer;
    public PlayerController_New playerController;
    
    [SerializeField] private PlayerStepTracker stepTracker;

    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text stepText;
    
    [SerializeField] private TMP_Text trophyCountText;
    
    [Header("Speed")]
    [SerializeField] private TMP_Text maxSpeedText;
    [SerializeField] private TMP_InputField currentSpeedText;
    
    [Header("Swing")]
    [SerializeField] private TMP_Text swingText;


    private void OnEnable()
    {
        stepTracker.OnLevelProgressChanged += UpdateLevelUI;
    }


    private void OnDisable()
    {
        stepTracker.OnLevelProgressChanged -= UpdateLevelUI;
    }

    private void Awake()
    {
        currentSpeedText.contentType = TMP_InputField.ContentType.IntegerNumber;
        currentSpeedText.onValueChanged.AddListener(OnSpeedInputChanged);
    }


    private void Start()
    {
        if (!networkPlayer.IsOwner)
        {
            gameObject.SetActive(false);
        }
        
        levelBar.fillAmount = 0f;
        stepText.text = "0 / 0";
        maxSpeedText.text = "MaxSpeed - " + playerController.GetMaxSpeed().ToString();
        currentSpeedText.text = "MaxSpeed - " + playerController.GetMaxSpeed().ToString();
        
        OnTrophyCountChanged(0);
    }


    private void UpdateLevelUI(
        float progress,
        int current,
        int max)
    {
        levelBar.fillAmount = progress;

        stepText.text =
            $"{current} / {max}";
    }

    public void ToggleSwingText(bool toggle)
    {
        swingText.gameObject.SetActive(toggle);
    }

    public void OnSwing(int swingCount, int maxSwingCount)
    {
        swingText.text = $"Swing {swingCount} / {maxSwingCount}";
    }

    public void OnMaxSpeedChanged(int max)
    {
        maxSpeedText.text = max.ToString();
    }

    public void OnTrophyCountChanged(int count)
    {
        trophyCountText.text = count.ToString();
    }

    private void OnSpeedInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (!int.TryParse(value, out int speed))
            return;

        speed = Mathf.Clamp(speed, 0, (int)playerController.GetMaxSpeed());

        string clampedValue = speed.ToString();

        if (currentSpeedText.text != clampedValue)
        {
            currentSpeedText.SetTextWithoutNotify(clampedValue);
        }

        playerController.SetCurrentSpeed(speed);
    }
}