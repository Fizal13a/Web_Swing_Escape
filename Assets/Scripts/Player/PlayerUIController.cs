using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerUIController : MonoBehaviour
{
    #region Fields

    public NetworkPlayer networkPlayer;
    public PlayerController_New playerController;

    [SerializeField] private PlayerStepTracker stepTracker;

    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text stepText;

    [SerializeField] private TMP_Text trophyCountText;

    [Header("Speed")]
    [SerializeField] private TMP_Text maxSpeedText;
    [SerializeField] private TMP_InputField currentSpeedText;

    [Header("Swing")]
    [SerializeField] private TMP_Text swingText;
    
    [Header("Rebirth")]
    [SerializeField] private TMP_Text rebirthText;
    [SerializeField] private TMP_Text rebirthLevelText;
    [SerializeField] private TMP_Text currentSpeedMultiplierText;
    [SerializeField] private TMP_Text nextSpeedMultiplierText;
    [SerializeField] private Image rebirthLevelBar;

    #endregion

    #region Initialize

    private void Awake()
    {
        currentSpeedText.contentType = TMP_InputField.ContentType.IntegerNumber;
        currentSpeedText.onValueChanged.AddListener(OnSpeedInputChanged);
    }

    private void OnEnable()
    {
        stepTracker.OnLevelProgressChanged += UpdateLevelUI;
    }

    private void OnDisable()
    {
        stepTracker.OnLevelProgressChanged -= UpdateLevelUI;
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
        currentSpeedText.text = playerController.GetMaxSpeed().ToString();

        OnTrophyCountChanged(0);
    }

    #endregion

    #region UI Updates

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
        currentSpeedText.text = max.ToString();
        maxSpeedText.text = max.ToString();
    }

    public void OnTrophyCountChanged(int count)
    {
        trophyCountText.text = count.ToString();
    }

    public void UpdateSpeedMultiplier(float speedMultiplier)
    {
        currentSpeedMultiplierText.text =  $"{speedMultiplier}";
        nextSpeedMultiplierText.text = $"{speedMultiplier + 0.5f}x";
    }

    public void OnLevelUp(int level, float rebirthProgress, int rebirthLevel)
    {
        levelText.text = $"Level : {level.ToString()}";
        
        rebirthLevelBar.fillAmount = rebirthProgress;
        rebirthLevelText.text = $"{level.ToString()} / {rebirthLevel.ToString()}";
    }

    public void OnRebirth(int level, int rebirthLevel)
    {
        levelText.text = $"Level : {level.ToString()}";
        
        rebirthLevelBar.fillAmount = 0;
        rebirthLevelText.text = $"{level.ToString()} / {rebirthLevel.ToString()}";
        
        playerController.OnRebirth();
    }

    public void OnRebirthIncrement(int rebirthCount)
    {
        rebirthText.text = rebirthCount.ToString();
    }

    #endregion

    #region Speed Input

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

    #endregion
}