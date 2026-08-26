using System;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    public event Action OnFrontHitEnd;
    public event Action OnBackHitEnd;

    public void FrontHitEnd()
    {
        OnFrontHitEnd?.Invoke();
    }

    public void BackHitEnd()
    {
        OnBackHitEnd?.Invoke();
    }
}
