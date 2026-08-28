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
    }

    private void UpdateState()
    {
        if (animator.GetBool("Swing"))
        {
            CurrentAnimationState = Swing;
        }
        else if (animator.GetBool("Air"))
        {
            CurrentAnimationState = Air;
        }
        else if (animator.GetBool("Run"))
        {
            CurrentAnimationState = Run;
        }
        else
        {
            CurrentAnimationState = Idle;
        }
        
        CurrentAnimationSpeed =  animator.GetFloat("RunSpeed");
    }

    public void SetAnimationSpeed(float value)
    {
        animator.SetFloat("RunSpeed", value);
    }
    
    public void SetNetworkAnimationState(int state)
    {
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