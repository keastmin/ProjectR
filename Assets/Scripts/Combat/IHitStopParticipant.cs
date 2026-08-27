public interface IHitStopParticipant
{
    bool IsHitStopped { get; }

    void BeginHitStop();
    void EndHitStop();
}
