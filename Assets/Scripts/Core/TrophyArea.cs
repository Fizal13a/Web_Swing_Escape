using System;
using TMPro;
using UnityEngine;

public class TrophyArea : MonoBehaviour
{
   public int trophyCount;
   public Transform spawnPoint;
   
   public Canvas canvas;
   public TextMeshProUGUI  trophyCountText;
   
   private void OnEnable()
   {
      BillboardManager.Instance?.Register(transform);
   }

   private void OnDisable()
   {
      BillboardManager.Instance?.Unregister(canvas.transform);
   }
   private void Start()
   {
      trophyCountText.text = $"+{trophyCount.ToString()}";
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Player")
      {
         PlayerController_New playerController = other.GetComponent<PlayerController_New>();
         if (playerController != null)
         {
            playerController.TeleportTo(spawnPoint.position);
            playerController.SetTrophyCount(trophyCount);
         }
      }
   }
}
