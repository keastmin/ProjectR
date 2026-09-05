using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

// Exercises the scene's actual Animator, skill notifications and Rigidbody.
public static class SkillMotionVerification
{
    private const string Request = "Temp/SkillMotionVerification.request";
    private const string Report = "Temp/SkillMotionVerification.result";
    private const string Pending = "ProjectR.SkillMotionVerification";
    private static PlayerCore _player;
    private static PlayerSkillHitTestActor _victim;
    private static PlayableDirector _director;
    private static Vector3 _start;
    private static double _deadline;
    private static int _phase;
    private static readonly int[] Frames = { 0, 2, 12, 30 };
    private static readonly int[] FrameRates = { 30, 60, 120 };
    private static float _baseline;
    private static bool _waiting;
    private static int _oldFps, _oldVsync;
    private static Vector3 _rawMotion, _stoppedMotion;
    private static bool _failed, _oldRunInBackground;
    private static ParticleSystem _particle;
    private static StopProbe _attackerClock, _victimClock;
    private static float _largestDelta;
    private static int _expectedHits;
    private static int StopFrames => Frames[_phase % Frames.Length];

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    [MenuItem("Tools/Combat/Validate Skill Motion With Hit Stop")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Run motion verification from Edit Mode; it restores the scene by exiting Play Mode.");
        File.WriteAllText(Report, "Running actual scene skill motion verification\n");
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (EditorApplication.isCompiling) return;
        if (File.Exists(Request) && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            File.Delete(Request);
            Run();
        }
        if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || Time.frameCount < 5) return;
        try
        {
            if (_player == null)
            {
                _player = Object.FindAnyObjectByType<PlayerCore>();
                if (_player == null) throw new Exception("Open the player demo scene before running this verification.");
                _oldFps = Application.targetFrameRate;
                _oldVsync = QualitySettings.vSyncCount;
                _oldRunInBackground = Application.runInBackground;
                Application.runInBackground = true;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                _player.InputCollector.enabled = false;
                foreach (var enemy in Object.FindObjectsByType<EnemyCore>())
                    enemy.gameObject.SetActive(false);
                _player.transform.position = new Vector3(1000, 1000, 1000);
                _player.GetComponent<Rigidbody>().position = _player.transform.position;
                var go = new GameObject("Skill motion verification victim");
                go.transform.position = _player.transform.position + Vector3.forward * 10;
                go.layer = 6;
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = Vector3.one * 100;
                _victim = go.AddComponent<PlayerSkillHitTestActor>();
                _director = _player.DirectorContainer.Directors[DirectorID.Skill];
                _expectedHits = ((TimelineAsset)_director.playableAsset).GetOutputTracks()
                    .OfType<PlayerSkillHitTrack>().Where(t => !t.mutedInHierarchy)
                    .Sum(t => t.GetMarkers().OfType<PlayerSkillHitMarker>().Count(m => m.time < 2.05));
                if (_expectedHits == 0) throw new Exception("The skill needs active hit markers to exercise hit stop");
                _player.Animator.GetComponent<PlayerAnimatorController>().OnAnimationTick += Sample;
                var effect = new GameObject("Verification VFX", typeof(ParticleSystem));
                effect.transform.SetParent(_player.transform, false);
                _particle = effect.GetComponent<ParticleSystem>();
                CombatVfxTime.RegisterHierarchy(effect);
                _phase = 0;
                _failed = false;
                Begin();
                return;
            }
            if (EditorApplication.timeSinceStartup > _deadline) throw new Exception("Skill motion timed out");
            _largestDelta = Mathf.Max(_largestDelta, Time.unscaledDeltaTime);
            if (!_waiting && _director.time > 2.0 && !_player.IsHitStopped)
            {
                _waiting = true;
                _deadline = EditorApplication.timeSinceStartup + 1;
            }
            if (!_waiting || _director.state == PlayState.Playing) return;
            float distance = Vector3.ProjectOnPlane(_player.GetComponent<Rigidbody>().position - _start, Vector3.up).magnitude;
            File.AppendAllText(Report, $"targetFps={FrameRates[_phase / Frames.Length]} frames={StopFrames} distance={distance:F6} hits={_victim.Hits.Count} raw={_rawMotion.magnitude:F6} stoppedMotion={_stoppedMotion.magnitude:F6}\n");
            if (_phase == 0) _baseline = distance;
            if (distance < 1 || Mathf.Abs(distance - _baseline) > 0.02f || _victim.Hits.Count != _expectedHits || _stoppedMotion.sqrMagnitude > 0.000001f)
            {
                _failed = true;
                File.AppendAllText(Report, $"FAIL: expected baseline distance {_baseline:F6}, {_expectedHits} hits, and no motion while frozen\n");
            }
            ValidateClock(_attackerClock, StopFrames);
            ValidateClock(_victimClock, StopFrames == 0 ? 0 : StopFrames + 1);
            _phase++;
            if (_phase == Frames.Length * FrameRates.Length)
            {
                Finish(null);
                return;
            }
            Begin();
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void Begin()
    {
        _rawMotion = _stoppedMotion = Vector3.zero;
        _largestDelta = 0;
        Application.targetFrameRate = FrameRates[_phase / Frames.Length];
        _player.transform.position = new Vector3(1000, 1000, 1000);
        _player.GetComponent<Rigidbody>().position = _player.transform.position;
        var receiver = _director.GetComponent<PlayerSkillAttackReceiver>();
        var settings = new SerializedObject(receiver);
        var fields = settings.FindProperty("_damageFields");
        for (int i = 0; i < fields.arraySize; i++)
        {
            var field = fields.GetArrayElementAtIndex(i);
            field.FindPropertyRelative("_hitStopFrame").intValue = StopFrames;
            field.FindPropertyRelative("_hitStopMode").intValue = (int)AttackHitStopMode.AttackerAndVictims;
            field.FindPropertyRelative("_targetLayers").intValue = 1 << 6;
            field.FindPropertyRelative("_triggerInteraction").intValue = (int)QueryTriggerInteraction.Collide;
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
        _victim.Hits.Clear();
        _start = _player.GetComponent<Rigidbody>().position;
        _player.AddSkillGauge(100);
        _player.StateMachine.Transition(_player.StateMachine.SkillState);
        Physics.SyncTransforms();
        _deadline = EditorApplication.timeSinceStartup + 45;
        _waiting = false;
        _attackerClock = new StopProbe();
        _victimClock = new StopProbe();
        HitstopCoordinator.Request(_attackerClock, new IHitStopParticipant[] { _victimClock }, StopFrames);
    }

    private static void Sample()
    {
        _rawMotion += _player.Animator.deltaPosition;
        if (_player.IsHitStopped) _stoppedMotion += _player.Animator.deltaPosition;
        if (_player.IsHitStopped && _particle.main.simulationSpeed != 0)
            _failed = true;
    }

    private static void ValidateClock(StopProbe probe, int frames)
    {
        if (frames == 0)
        {
            if (probe.Began) throw new Exception("Zero frames must not freeze a participant");
            return;
        }
        double duration = probe.End - probe.Start;
        double expected = frames / 60d;
        if (!probe.Began || probe.IsHitStopped || duration + 0.000001 < expected || duration > expected + _largestDelta + 0.00001)
            throw new Exception($"Hit stop duration {duration:F6}s does not match {frames}/60s within one render interval");
    }

    private sealed class StopProbe : IHitStopParticipant
    {
        public bool IsHitStopped { get; private set; }
        public bool Began;
        public double Start, End;
        public void BeginHitStop() { IsHitStopped = Began = true; Start = Time.unscaledTimeAsDouble; }
        public void EndHitStop() { IsHitStopped = false; End = Time.unscaledTimeAsDouble; }
    }

    private static void Finish(Exception exception)
    {
        SessionState.SetBool(Pending, false);
        Application.targetFrameRate = _oldFps;
        QualitySettings.vSyncCount = _oldVsync;
        Application.runInBackground = _oldRunInBackground;
        string result = exception == null ? (_failed ? "FAIL\n" : $"PASS: motion, {_expectedHits} authored hits, VFX freeze and 60 Hz durations across 12 cases\n") : exception + "\n";
        File.AppendAllText(Report, result);
        if (exception != null) Debug.LogException(exception);
        else if (_failed) Debug.LogError(result);
        else Debug.Log(result);
        EditorApplication.ExitPlaymode();
        _player = null;
    }
}
