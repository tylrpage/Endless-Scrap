using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private string _currentState;
    private DirectionType _previousDirectionType;
    
    public enum AnimationType
    {
        idle, hit1, hit2, run, walk
    }

    public enum DirectionType
    {
        down, up, side
    }

    public void SetAnimation(AnimationType animationType, Vector2 direction)
    {
        // Flip sprite if we are moving to the right
        spriteRenderer.flipX = direction.x > 0;
        
        DirectionType directionType = DirectionType.down;
        if (Mathf.Abs(direction.x) == 0 && Mathf.Abs(direction.y) == 0)
        {
            directionType = _previousDirectionType;
            animationType = AnimationType.idle;
        }
        else if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            directionType = DirectionType.side;
            _previousDirectionType = directionType;
        }
        else
        {
            directionType = direction.y <= 0 ? DirectionType.down : DirectionType.up;
            _previousDirectionType = directionType;
        }
        
        string animationName = GetAnimationName(animationType, directionType);
        ChangeAnimationState(animationName);
    }

    private string GetAnimationName(AnimationType animationType, DirectionType directionType)
    {
        return $"{animationType}_{directionType}";
    }

    private void ChangeAnimationState(string newState)
    {
        if (newState == _currentState)
        {
            return;
        }
        
        animator.Play(newState);
        _currentState = newState;
    }
}
