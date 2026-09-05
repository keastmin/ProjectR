# 완벽 회피 슬로우모션

씬의 **Player → PlayerCore → Slow Motion Settings**를 펼쳐 조정합니다.
별도 매니저 배치나 ScriptableObject 연결은 필요하지 않습니다.

| 설정 | 의미 |
| --- | --- |
| Slow Scale | 전투 재생 배율. 1은 정상, 작은 값일수록 느림. 애니메이션 종료 이벤트가 실행되도록 최솟값은 0.01 |
| Start Delay | 완벽 회피 성공 후 슬로우와 반격 입력 구간을 열기 전까지의 실제 시간. 무적은 이 지연과 무관하게 즉시 시작 |
| Max Duration | 슬로우와 반격 입력 구간의 최대 유지 시간, 실제 초. 회피 애니메이션 이벤트가 먼저 오면 즉시 종료 |
| Fade In Duration | 슬로우 진입 시간, 실제 초. 0은 즉시 적용 |
| Fade In Curve | 진입 보간 곡선. 가로축은 진행도, 세로축은 목표 배율로의 보간 정도 |
| Fade Out Duration | 자연 종료 후 복귀 시간, 실제 초. 0은 즉시 적용 |
| Fade Out Curve | 복귀 보간 곡선 |

기본값은 Slow Scale 0.2, Start Delay 0.07초, Max Duration 1.65초, Fade In 0.08초, Fade Out 0.15초이며 로직 내부의 고정 상수가 아닙니다.
현재 `Player` 프리팡 튜닝값은 0.08 / 0.07초 / 1.65초 / 0.06초 / 0.07초입니다.
실행 중에도 다음 완벽 회피부터 새 설정을 사용합니다. Unity Play Mode에서 변경한 값은 종료하면 되돌아가므로 영구 설정은 Edit Mode에서 저장합니다.

## 동작

- 회피 성공 시 대상 적을 저장하고 무적을 즉시 적용합니다. Start Delay 동안은 정상 속도로 회피 동작을 보여주며 회피 반격 입력은 아직 열리지 않습니다.
- Start Delay가 끝나면 플레이어, 활성 적, 등록된 전투 VFX에 슬로우를 적용하고 반격 입력 구간을 엽니다. 지연 중 누른 공격이 일반 공격으로 전환되지는 않습니다.
- 회피 클립의 `OnPerfectDodgeEndInvoke`나 Max Duration 중 먼저 도달한 쪽이 반격 구간을 닫고 FadeOut을 시작합니다. Max Duration은 실제 시간이므로 슬로우 강도를 더 높여도 유지 시간이 무한정 늘지 않습니다.
- 반격 구간에서 좌클릭하면 FadeIn/FadeOut을 기다리지 않고 자신의 슬로우 요청을 제거한 뒤 `DodgeAttackStartState`에 진입합니다.
- 완벽 회피 성공부터 `OnPerfectDodgeEndInvoke` 또는 Max Duration까지 무적이며, 반격하면 DodgeAttack의 Start/Loop/End 전체로 무적이 이어집니다. 일반 회피, 자연 종료 후 FadeOut만 남은 구간, 반격을 벗어난 상태는 무적이 아닙니다.
- 무적 중에는 `TryTakeDamage`가 false를 반환하므로 마지막 피해 기록, 피격 이벤트/상태 전환, 해당 타격의 히트스탑이 발생하지 않습니다. 다른 적의 공격에도 동일하게 적용됩니다.
- 대상이 비활성화되거나 사망했다면 슬로우를 정리하고 기존 일반 공격 분기로 넘어갑니다.
- 피격, 회피 도중 상태 이탈, 플레이어 비활성화 시 요청을 정리합니다. 자연 종료 후 다른 상태로 넘어갈 때에는 FadeOut이 계속 진행됩니다.
- 히트스탑 중에는 속도 0이 우선합니다. 히트스탑 해제 시 현재 슬로우 배율로 복귀합니다. 다른 소유자의 슬로우나 전역 일시정지를 제거하지 않습니다.
- `Time.timeScale`과 `Time.fixedDeltaTime`은 변경하지 않습니다. UI, 입력, 카메라 조작은 전투 배율을 사용하지 않습니다.

반격은 저장된 적 방향으로 회전하고 Start → Loop → End → Idle로 진행합니다. 기존 `MotionWarpToEnemyHurtbox`의 이동 보정은 이번 시간 제어 작업의 범위에 포함하지 않았으며, 현재 이동은 기존 루트 모션을 따릅니다.

## VFX와 화면 효과 연결

현재 플레이어/적 하위 VFX, 발 부스터 생성 경로, `CombatEffectPool`, 플레이어 Timeline의 Control Track 대상은 자동으로 등록합니다. 새 전투 VFX 생성 경로를 추가하면 인스턴스에 `CombatVfxTime.RegisterHierarchy(instance)`를 호출하거나 ParticleSystem/VisualEffect/Tiny.Trail이 있는 오브젝트에 `CombatVfxTime`을 추가합니다. Canvas 하위는 자동 등록에서 제외합니다.

Timeline이 ParticleSystem을 직접 시뮬레이션하면 파티클 배율을 중복 적용하지 않습니다. `HitStopParticleMode.ContinueDuringHitStop`인 독립 파티클도 완벽 회피 슬로우의 영향은 받습니다. 풀의 재생 수명도 전투 시간으로 계산합니다.

`PlayerCore`의 Inspector 이벤트 `On Perfect Dodge Started`, `On Perfect Dodge Ended`, `On Dodge Attack Started`에 UI/화면 연출을 연결할 수 있습니다. C# 이벤트는 시작/반격 시 원인 적도 전달합니다. 종료 이벤트는 반격 가능 구간이 닫히는 순간이며 FadeOut 완료 이벤트는 아닙니다.

새로 추가하는 코드 기반 전투 회전/타이머는 `CombatTimeController.DeltaTime`을 사용합니다. **Animator.deltaPosition으로 얻은 루트 모션에는 배율을 다시 곱하지 않습니다.** 중력/물리 시뮬레이션 전체나 셰이더 `_Time`은 자동 감속되지 않습니다. 시간으로 움직이는 커스텀 셰이더에는 별도 전투 시간 전달이 필요합니다.

## 검증

- Unity 메뉴 `Tools → Combat → Validate Slow Motion Curves`: 설정값, 진입 중 해제, 중복 해제, 0초 전환 검사.
- 별도 Unity 배치 실행에서 `-executeMethod PerfectDodgeVerification.RunBatch` (`-quit` 제외): 프로토타입 씬의 Play Mode에서 입력, 상태 전환, 이벤트 연결, 히트스탑, Timeline, 파티클 재사용, 대상 소멸, 요청 소유권을 검증하고 종료합니다. 배치 검증은 실행 중 씬을 저장하지 않습니다.

2026-09-01, Unity 6000.5.8f1 배치 실행에서 컴파일 및 44개 assertion을 통과했습니다. 로그: `Temp/PerfectDodgeVerification.log`. 화면 렌더링을 생략한 검증이므로 곡선의 체감과 셰이더 표현은 Game 뷰에서 확인해야 합니다.

이후 추가한 무적 처리는 런타임/Editor 어셈블리 빌드에서 오류 0개를 확인했습니다. 일반 회피의 피해 허용, 완벽 회피 및 반격 3단계의 피해 거부, 전환 순간의 무적 유지, 무적 종료 후 피해 허용, 거부된 타격의 히트스탑 제외 검사를 배치 검증에 추가했습니다. 이 추가 검사는 현재 열려 있는 Unity 작업을 방해하지 않도록 Play Mode에서 재실행하지 않았습니다. 위 44개 통과 기록은 무적 확장 이전 실행 결과입니다.

가져온 외부 에셋 `Assets/Download Assets/Hovl Studio/HSFiles/Models/CylindricalCone2.fbx.meta`에는 기존 YAML escape 오류가 있습니다. 이 파일은 이번 작업에서 수정하지 않았으며, 위 검증은 해당 오류와 별개로 통과했습니다.
