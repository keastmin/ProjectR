using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisallowMultipleComponent, RequireComponent(typeof(PlayableDirector))]
public sealed class PlayerSkillAttackReceiver : MonoBehaviour, INotificationReceiver
{
    public const int DamageFieldCount = 12;

    [SerializeField] private PlayerAttackInstanceContainer _attackContainer;
    [SerializeField] private Transform _hitboxRoot;
    [SerializeField] private AttackDamageField[] _damageFields = new AttackDamageField[DamageFieldCount];
    private PlayableDirector _director;

    public AttackDamageField GetDamageField(int number) =>
        _damageFields != null && number >= 1 && number <= _damageFields.Length ? _damageFields[number - 1] : null;

    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        if (_attackContainer == null)
            _attackContainer = GetComponentInParent<PlayerAttackInstanceContainer>();
        foreach (AttackDamageField field in _damageFields)
            field?.DisablePhysicalCollision();
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // Editor preview/scrubbing and callbacks left over after an interrupted
        // skill must never deal damage. Timeline handles once-per-playback delivery.
        if (!Application.isPlaying || !isActiveAndEnabled ||
            notification is not PlayerSkillHitMarker marker ||
            _director == null || _director.state != PlayState.Playing ||
            marker.parent is not PlayerSkillHitTrack ||
            marker.parent.timelineAsset != _director.playableAsset ||
            !origin.IsValid() || !ReferenceEquals(origin.GetGraph().GetResolver(), _director))
            return;

        AttackDamageField field = GetDamageField(marker.DamageFieldNumber);
        if (_attackContainer == null || field == null || field.Hitbox == null)
        {
            Debug.LogWarning($"Skill Hit{marker.DamageFieldNumber}: Attack Container / Damage Field / Hitbox 연결을 확인하세요.", this);
            return;
        }

        // Timeline moves the hitboxes in this frame; queries need their current
        // world poses even when Physics.autoSyncTransforms is disabled.
        Physics.SyncTransforms();
        // Each marker is a separate hit. Multiple colliders on one enemy are
        // deduplicated within this hit, while later Hit12 markers can hit it again.
        _attackContainer.GiveDamageFieldNoHashing(field);
    }
}
