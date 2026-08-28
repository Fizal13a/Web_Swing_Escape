using UnityEngine;

public class TreadMill : MonoBehaviour
{
    [Header("Step Reward")]
    [SerializeField] private int minStepIncrement = 2;
    [SerializeField] private int maxStepIncrement = 5;
    [SerializeField] private float stepInterval = 1f;

    private float _timer;
    private PlayerStepTracker _playerStepTracker;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(
                out PlayerController_New networkPlayer))
        {
            return;
        }

        if (!networkPlayer.networkPlayer.IsOwner)
            return;
        
        if (!other.TryGetComponent(
                out PlayerStepTracker stepTracker))
        {
            return;
        }

        _playerStepTracker = stepTracker;
        _timer = stepInterval;

        networkPlayer.SetTreadmillState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(
                out PlayerController_New networkPlayer))
        {
            return;
        }

        if (!networkPlayer.networkPlayer.IsOwner)
            return;
        
        if (!other.TryGetComponent(
                out PlayerStepTracker stepTracker))
        {
            return;
        }

        if (_playerStepTracker != stepTracker)
            return;

        networkPlayer.SetTreadmillState(false);

        _playerStepTracker = null;
        _timer = 0f;
    }

    private void Update()
    {
        if (_playerStepTracker == null)
            return;

        _timer -= Time.deltaTime;

        if (_timer > 0f)
            return;

        _timer += stepInterval;

        int stepIncrement = Random.Range(
            minStepIncrement,
            maxStepIncrement + 1
        );

        _playerStepTracker.AddTreadmillSteps(
            stepIncrement
        );
    }
}