using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private PlayerStepTracker stepTracker;

    [SerializeField] private Image levelBar;
    [SerializeField] private TMP_Text stepText;


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
        levelBar.fillAmount = 0f;
        stepText.text = "0 / 0";
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
}