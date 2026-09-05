using UnityEngine.Timeline;

[TrackColor(0.95f, 0.35f, 0.2f)]
[TrackBindingType(typeof(PlayerSkillAttackReceiver))]
public sealed class PlayerSkillHitTrack : MarkerTrack
{
}
