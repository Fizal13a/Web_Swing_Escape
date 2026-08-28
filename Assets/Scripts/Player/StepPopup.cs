using TMPro;
using UnityEngine;

public class StepPopup : MonoBehaviour
{
    #region Fields

    [SerializeField] private TMP_Text text;

    [Header("Juice")]
    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float moveSpeed = 80f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Level Up Style")]
    [SerializeField] private string levelUpMessage = "LEVEL UP!";
    [SerializeField] private Color levelUpColor = Color.yellow;
    [SerializeField] private float levelUpScaleMultiplier = 1.5f;

    private float timer;

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private Color baseTextColor;

    #endregion

    #region Initialize

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        baseScale = rectTransform.localScale;
        targetScale = baseScale;
        baseTextColor = text.color;
    }

    #endregion

    #region Public API

    public void Show(string value)
    {
        text.text = value;

        timer = 0f;

        bool isLevelUp = value == levelUpMessage;

        targetScale = isLevelUp
            ? baseScale * levelUpScaleMultiplier
            : baseScale;

        text.color = isLevelUp
            ? levelUpColor
            : baseTextColor;

        rectTransform.localScale = targetScale * 1.3f;

        gameObject.SetActive(true);
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        timer += Time.deltaTime;

        rectTransform.position +=
            Vector3.up * (moveSpeed * Time.deltaTime);

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        Color color = text.color;
        color.a = 1f - (timer / lifeTime);
        text.color = color;

        if (timer >= lifeTime)
        {
            Hide();
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}