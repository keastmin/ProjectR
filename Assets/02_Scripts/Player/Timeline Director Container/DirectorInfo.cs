using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public struct DirectorInfo
{
    public DirectorID ID;
    public PlayableDirector Director;

    [Tooltip("이 Timeline의 파티클을 히트스탑에 포함할지 선택합니다.")]
    public HitStopParticleMode ParticleMode;
}
