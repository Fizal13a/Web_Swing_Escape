using TMPro;
using UnityEngine;

public class StepPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Juice")]
    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float moveSpeed = 80f;
    [SerializeField] private float scaleSpeed = 12f;

    private float timer;

    private RectTransform rectTransform;
    private Vector3 targetScale;
    private Color textColor;


    private void Awake()
    {
        rectTransform = transform as RectTransform;

        targetScale = rectTransform.localScale;
        textColor = text.color;
    }


    public void Show(string value)
    {
        text.text = value;

        timer = 0f;

        rectTransform.localScale = targetScale * 1.3f;

        text.color = textColor;

        gameObject.SetActive(true);
    }


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


        if(timer >= lifeTime)
        {
            Hide();
        }
    }


    private void Hide()
    {
        gameObject.SetActive(false);
    }
}