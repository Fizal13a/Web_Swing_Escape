using UnityEngine;

public class StepPopupController : MonoBehaviour
{
    [SerializeField] private StepPopupPool pool;
    [SerializeField] private Camera playerCamera;

    [Header("Popup Position")]
    [SerializeField] private float minHorizontalOffset = 80f;
    [SerializeField] private float maxHorizontalOffset = 180f;
    [SerializeField] private float minVerticalOffset = 80f;
    [SerializeField] private float maxVerticalOffset = 180f;

    public void SetCamera(Camera cam)
    {
        playerCamera = cam;
    }


    public void ShowStepPopup(string value)
    {
        if(playerCamera == null) return;
        
        StepPopup popup = pool.Get();


        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(transform.position);


        float xOffset = Random.Range(
            minHorizontalOffset,
            maxHorizontalOffset
        );

        float yOffset = Random.Range(
            minVerticalOffset,
            maxVerticalOffset
        );


        // Left or right only
        xOffset *= Random.value > 0.5f ? 1 : -1;


        Vector3 popupPosition =
            screenPosition +
            new Vector3(
                xOffset,
                yOffset,
                0f
            );


        popup.transform.position = popupPosition;


        popup.Show(value);
    }
}