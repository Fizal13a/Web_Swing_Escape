using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerStepTracker : MonoBehaviour
{
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


    private void Awake()
    {
        currentLevel = levelData.startingLevel;

        requiredSteps = levelData.startingRequiredSteps;

        minStepIncrement = levelData.startingMinStepIncrement;
        maxStepIncrement = levelData.startingMaxStepIncrement;

        lastPosition = transform.position;


        UpdateUI();
    }


    private void Update()
    {
        CheckSteps();
    }


    private void CheckSteps()
    {
        if(Vector3.Distance(transform.position, lastPosition) < stepDistance)
            return;


        lastPosition = transform.position;

        AddStep();
    }


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
        if(currentSteps < requiredSteps)
            return;


        currentLevel++;

        currentSteps = 0;


        requiredSteps +=
            levelData.requiredStepIncreasePerLevel;


        minStepIncrement +=
            levelData.stepIncrementIncreasePerLevel;


        maxStepIncrement +=
            levelData.stepIncrementIncreasePerLevel + 1;


        popupController.ShowStepPopup(
            "LEVEL UP!"
        );
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
}