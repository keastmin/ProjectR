using UnityEngine;

public class EnemyAnimationEventSender : MonoBehaviour
{
    [SerializeField] private EnemyAnimationEvent _animEvent;

    public void FrontHitEnd()
    {
        _animEvent.FrontHitEnd();
    }

    public void BackHitEnd()
    {
        _animEvent.BackHitEnd();
    }
}
