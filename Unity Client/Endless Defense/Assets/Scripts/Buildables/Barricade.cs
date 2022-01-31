using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : Buildable
{
    [SerializeField] private Animator animator;

    private string _currentAnimationState;

    public override void Die()
    {
        ChangeAnimationState("barricade_die");
        
        base.Die();
    }

    private void ChangeAnimationState(string newState)
    {
        if (newState == _currentAnimationState)
        {
            return;
        }
        
        animator.Play(newState);
        _currentAnimationState = newState;
    }
}
