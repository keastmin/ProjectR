using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Stored inside the Timeline: no separate SignalAsset or UnityEvent per hit.
public sealed class PlayerSkillHitMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField, Range(0, PlayerSkillAttackReceiver.DamageFieldCount)]
    [Tooltip("0은 시간순 자동 배정(Hit1~12, 이후 Hit12). 1~12를 지정하면 해당 필드를 사용합니다.")]
    private int _damageFieldNumber;

    public PropertyName id => new PropertyName(nameof(PlayerSkillHitMarker));
    public NotificationFlags flags => NotificationFlags.TriggerOnce;

    public int DamageFieldNumber
    {
        get
        {
            if (_damageFieldNumber > 0)
                return Mathf.Clamp(_damageFieldNumber, 1, PlayerSkillAttackReceiver.DamageFieldCount);

            int number = 1;
            bool passedSelf = false;
            if (parent != null)
            {
                // For equal times, preserve the track's marker order.
                foreach (IMarker marker in parent.GetMarkers())
                {
                    if (ReferenceEquals(marker, this))
                        passedSelf = true;
                    else if (marker is PlayerSkillHitMarker &&
                        (marker.time < time || (marker.time == time && !passedSelf)))
                        number++;
                }
            }
            return Mathf.Min(number, PlayerSkillAttackReceiver.DamageFieldCount);
        }
    }
}
