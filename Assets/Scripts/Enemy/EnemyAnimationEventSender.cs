using UnityEngine;

public class EnemyAnimationEventSender : MonoBehaviour
{
    [SerializeField] private EnemyAnimationEvent _animEvent;

    public void OnAnimationEnd(AnimationEvent animationEvent)
    {
        _animEvent.AnimationEndActionInvoke(animationEvent);
    }
}
