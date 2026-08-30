using System;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public int CurrentAnimationState { get; private set; }
    public float CurrentAnimationSpeed { get; private set; }

    public const int Idle = 0;
    public const int Run = 1;
    public const int Air = 2;
    public const int Swing = 3;
    
    public event Action<int, float> AnimationChanged;


    // =========================================================
    // LOCAL ANIMATION
    // =========================================================

    public void SetRun(bool value)
    {
        animator.SetBool("Run", value);
        UpdateState();
    }

    public void SetAir(bool value)
    {
        animator.SetBool("Air", value);
        UpdateState();
    }

    public void SetSwing(bool value)
    {
        animator.SetBool("Swing", value);
        UpdateState();
    }

    public void SetRunSpeed(float value)
    {
        animator.SetFloat("RunSpeed", value);
        CurrentAnimationSpeed = value;
    }


    private void UpdateState()
    {
        int newState;

        if (animator.GetBool("Swing"))
        {
            newState = Swing;
        }
        else if (animator.GetBool("Air"))
        {
            newState = Air;
        }
        else if (animator.GetBool("Run"))
        {
            newState = Run;
        }
        else
        {
            newState = Idle;
        }

        float newSpeed = animator.GetFloat("RunSpeed");

        bool stateChanged = newState != CurrentAnimationState;
        bool speedChanged = Mathf.Abs(newSpeed - CurrentAnimationSpeed) > 0.05f;

        CurrentAnimationState = newState;
        CurrentAnimationSpeed = newSpeed;

        if (CurrentAnimationSpeed < 1)
            CurrentAnimationSpeed = 1f;
        
        if (stateChanged || speedChanged)
        {
            AnimationChanged?.Invoke(
                CurrentAnimationState,
                CurrentAnimationSpeed
            );
        }
    }


    // =========================================================
    // NETWORK ANIMATION
    // =========================================================

    public void SetAnimationSpeed(float value)
    {
        animator.SetFloat("RunSpeed", value);
        CurrentAnimationSpeed = value;
    }


    public void SetNetworkAnimationState(int state)
    {
        CurrentAnimationState = state;

        switch (state)
        {
            case Idle:

                animator.SetBool("Run", false);
                animator.SetBool("Air", false);
                animator.SetBool("Swing", false);

                break;


            case Run:

                animator.SetBool("Run", true);
                animator.SetBool("Air", false);
                animator.SetBool("Swing", false);

                break;


            case Air:

                animator.SetBool("Run", false);
                animator.SetBool("Air", true);
                animator.SetBool("Swing", false);

                break;


            case Swing:

                animator.SetBool("Run", false);
                animator.SetBool("Air", true);
                animator.SetBool("Swing", true);

                break;
        }
    }
}