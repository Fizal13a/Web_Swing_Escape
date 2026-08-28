using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerStepTracker : MonoBehaviour
{
    #region Fields

    public NetworkPlayer networkPlayer;
    private PlayerController_New playerController;
    Animator animator;

    [SerializeField] private LevelProgressionData levelData;
    [SerializeField] private StepPopupController popupController;

    [SerializeField] private float stepDistance = 1f;

    private Vector3 lastPosition;

    private int currentLevel;
    private int currentSteps;
    private int requiredSteps;

    private int minStepIncrement;
    private int maxStepIncrement;

    public event Action<float, int, int> OnLevelProgressChanged;

    #endregion

    #region Initialize

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController_New>();
        currentLevel = levelData.startingLevel;

        requiredSteps = levelData.startingRequiredSteps;

        minStepIncrement = levelData.startingMinStepIncrement;
        maxStepIncrement = levelData.startingMaxStepIncrement;

        lastPosition = transform.position;

        UpdateUI();
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (!networkPlayer.IsOwner)
            return;

        CheckSteps();
    }

    private void CheckSteps()
    {
        if (Vector3.Distance(
                transform.position,
                lastPosition) < stepDistance)
        {
            return;
        }

        lastPosition = transform.position;

        AddStep();
    }

    #endregion

    #region Step & Level Progression

    private void AddStep()
    {
        int stepGain = Random.Range(
            minStepIncrement,
            maxStepIncrement + 1
        );

        currentSteps += stepGain;

        popupController.ShowStepPopup(
            "+" + stepGain
        );

        CheckLevel();

        UpdateUI();
    }

    private void CheckLevel()
    {
        if (currentSteps < requiredSteps)
            return;

        currentLevel++;

        currentSteps = 0;

        requiredSteps +=
            levelData.requiredStepIncreasePerLevel;

        minStepIncrement +=
            levelData.stepIncrementIncreasePerLevel;

        maxStepIncrement +=
            levelData.stepIncrementIncreasePerLevel + 1;

        playerController.OnLevelUp(levelData.speedIncreasePerLevel);

        popupController.ShowStepPopup(
            "LEVEL UP!"
        );
    }

    public void AddTreadmillSteps(int amount)
    {
        currentSteps += amount;

        popupController.ShowStepPopup(
            "+" + amount
        );

        CheckLevel();
        UpdateUI();
    }

    private void UpdateUI()
    {
        float progress =
            (float)currentSteps / requiredSteps;

        OnLevelProgressChanged?.Invoke(
            progress,
            currentSteps,
            requiredSteps
        );
    }

    #endregion
}