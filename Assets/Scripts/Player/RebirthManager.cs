using UnityEngine;

public class RebirthManager : MonoBehaviour
{
   [SerializeField] private PlayerUIController playerUIController;
   [SerializeField] private PlayerStepTracker _playerStepTracker;
   [SerializeField] private PlayerController_New playerController;
   
   [Header("Rebirth")]
   private int rebirthCount = 0;

   [SerializeField] private int rebirthLevel = 5;
   [SerializeField] private int rebirthLevelIncrement = 3;
   
   private int currentLevel = 0;
   
   private bool canrebirth = false;
   
   public void OnLevelUp(int level)
   {
      currentLevel = level;
      
      float progress = Mathf.Clamp01((float)level / rebirthLevel);
      playerUIController.OnLevelUp(level, progress, rebirthLevel);
      
      if(canrebirth) return;
      
      if (level >= rebirthLevel)
      {
         canrebirth = true;
         rebirthLevel += rebirthLevelIncrement;
      }
   }

   public void OnRebirth()
   {
      if (canrebirth)
      {
         rebirthCount++;
         playerController.SetRebirthLevel(rebirthCount);
         playerUIController.OnRebirthIncrement(rebirthCount);
         _playerStepTracker.OnRebirth();
         currentLevel = 1;
         _playerStepTracker.OnRebirthLevel();
         playerUIController.OnRebirth(currentLevel, rebirthLevel);
         canrebirth = false;
      }
   }
   
}
