using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelProgressionData",
    menuName = "Game/Level Progression"
)]
public class LevelProgressionData : ScriptableObject
{
    [Header("Starting")]
    public int startingLevel = 1;
    public int startingRequiredSteps = 600;


    [Header("Step Increment")]
    public int startingMinStepIncrement = 1;
    public int startingMaxStepIncrement = 1;


    [Header("Level Scaling")]
    public int requiredStepIncreasePerLevel = 200;

    public int stepIncrementIncreasePerLevel = 1;
}