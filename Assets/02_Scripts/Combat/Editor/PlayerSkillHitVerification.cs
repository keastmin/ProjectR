using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

public static class PlayerSkillHitVerification
{
    private const string Request = "Temp/PlayerSkillHitVerification.request";
    private const string Report = "Temp/PlayerSkillHitVerification.result";
    private const string Pending = "ProjectR.SkillHitVerification.Pending";
    private const string StartedPlay = "ProjectR.SkillHitVerification.StartedPlay";
    private static GameObject _root;
    private static TimelineAsset _timeline;
    private static PlayerSkillAttackReceiver _receiver;
    private static PlayerSkillHitTestActor _attacker, _victim;
    private static PlayableDirector _director;
    private static double _deadline;
    private static int _phase;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        if (SessionState.GetBool(Pending, false))
            EditorApplication.update += Tick;
        else if (File.Exists(Request))
            EditorApplication.delayCall += () =>
            {
                File.Delete(Request);
                Run();
            };
    }

    [MenuItem("Tools/Combat/Validate Skill Hit Timeline")]
    public static void Run()
    {
        try
        {
            ValidateAssets();
            SessionState.SetBool(Pending, true);
            SessionState.SetBool(StartedPlay, !EditorApplication.isPlaying);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (!EditorApplication.isPlaying)
                EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void ValidateAssets()
    {
        var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/04_Timelines/Player/SkillTimeline.playable");
        var track = timeline.GetOutputTracks().OfType<PlayerSkillHitTrack>().Single();
        var markers = track.GetMarkers().OfType<PlayerSkillHitMarker>().OrderBy(m => m.time).ToArray();
        Check(markers.Length == 16, "All 16 placeholders were migrated");
        for (int i = 0; i < markers.Length; i++)
        {
            Check(markers[i].DamageFieldNumber == Math.Min(i + 1, 12), "Time-ordered Hit1..12 mapping");
            Check(Math.Abs(markers[i].time * 60 - (43 + i)) < 0.001, "Authored marker times preserved");
            Check(markers[i].flags == NotificationFlags.TriggerOnce, "Once per playback; no preview or retroactive attacks");
        }

        var preview = EditorSceneManager.OpenPreviewScene("Assets/01_Scenes/DemoScene 1.unity");
        try
        {
            var receiver = preview.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerSkillAttackReceiver>(true)).Single();
            var director = receiver.GetComponent<PlayableDirector>();
            Check(director.GetGenericBinding(track) == receiver, "Scene hit track is bound to its attack receiver");
            Check(new SerializedObject(receiver).FindProperty("_attackContainer").objectReferenceValue != null,
                "Scene receiver uses the player's existing damage pipeline");
            for (int i = 1; i <= 12; i++)
            {
                var field = receiver.GetDamageField(i);
                Check(field != null && field.Hitbox != null && field.Hitbox.name == $"Hit{i}", "Scene collider mapping");
                Check(!field.Hitbox.enabled && field.Damage > 0 && field.HitStopFrame > 0 && field.StaggerLevel == StaggerLevel.Level2,
                    "Hit fields have query-only colliders and real attack settings");
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }

        var scratch = ScriptableObject.CreateInstance<TimelineAsset>();
        try
        {
            var orderTrack = scratch.CreateTrack<PlayerSkillHitTrack>();
            var later = orderTrack.CreateMarker<PlayerSkillHitMarker>(1);
            var earlier = orderTrack.CreateMarker<PlayerSkillHitMarker>(0.5);
            Check(earlier.DamageFieldNumber == 1 && later.DamageFieldNumber == 2, "Order uses time, not creation order");
            later.time = 0.25;
            Check(later.DamageFieldNumber == 1 && earlier.DamageFieldNumber == 2, "Moving a marker recalculates automatic mapping");
            var so = new SerializedObject(later);
            so.FindProperty("_damageFieldNumber").intValue = 7;
            so.ApplyModifiedPropertiesWithoutUndo();
            Check(later.DamageFieldNumber == 7, "Manual field selection overrides order");
        }
        finally { DestroyTimeline(scratch); }
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Time.frameCount < 3)
            return;
        try
        {
            if (_root == null)
            {
                BuildTestAttack();
                _deadline = EditorApplication.timeSinceStartup + 15;
                _phase = 0;
                _director.Play();
                return;
            }
            Check(EditorApplication.timeSinceStartup < _deadline, "Runtime notifications complete before timeout");
            if (_phase == 0)
            {
                if (_victim.Hits.Count < 16 || _attacker.IsStopped || _victim.IsStopped)
                    return;
                Check(_victim.Hits.Count == 16, "Every emitter deals exactly one hit despite multiple victim colliders");
                for (int i = 0; i < 16; i++)
                    Check(_victim.Hits[i].DamageAmount == Math.Min(i + 1, 12), "Native notification uses the correct field");
                Check(Mathf.Abs(_victim.Health - 874) < 0.001f, "Actual damage totals 126 including five Hit12 attacks");
                Check(_victim.Hits.All(h => h.StaggerLevel == StaggerLevel.Level2), "Attack level reaches damage receiver");
                Check(_attacker.StopCount > 0 && _victim.StopCount > 0, "Landed hits stop and release attacker and victim");
                int hits = _victim.Hits.Count;
                _director.Pause();
                _director.time = 0.01;
                _director.Evaluate();
                _director.time = 0.4;
                _director.Evaluate();
                Check(_victim.Hits.Count == hits, "Scrubbing does not attack");
                _director.Stop();
                ConfigureFields(AttackHitStopMode.VictimsOnly);
                _attacker.StopCount = _victim.StopCount = 0;
                _phase = 1;
                _director.time = 0;
                _director.Play();
            }
            else if (_phase == 1)
            {
                if (_victim.Hits.Count < 32 || _victim.IsStopped)
                    return;
                Check(_victim.Hits.Count == 32, "Replaying creates a fresh set of hits");
                Check(_attacker.StopCount == 0 && _victim.StopCount > 0, "Victims Only preserves attacker motion");
                _director.Stop();
                _victim.RejectDamage = true;
                _victim.StopCount = 0;
                ConfigureFields(AttackHitStopMode.AttackerAndVictims);
                _phase = 2;
                _director.time = 0;
                _director.Play();
            }
            else if (_director.time >= 0.45)
            {
                Check(_victim.Hits.Count == 32 && _victim.StopCount == 0 && _attacker.StopCount == 0,
                    "Rejected damage causes neither damage nor hit-stop");
                Finish(null);
            }
        }
        catch (Exception exception) { Finish(exception); }
    }

    private static void BuildTestAttack()
    {
        _root = new GameObject("Skill Hit Verification");
        _root.SetActive(false);
        _root.transform.position = new Vector3(10000, 10000, 10000);
        _attacker = _root.AddComponent<PlayerSkillHitTestActor>();
        var container = _root.AddComponent<PlayerAttackInstanceContainer>();
        _director = _root.AddComponent<PlayableDirector>();
        _director.playOnAwake = false;
        _attacker.Director = _director;
        _receiver = _root.AddComponent<PlayerSkillAttackReceiver>();
        var settings = new SerializedObject(_receiver);
        settings.FindProperty("_attackContainer").objectReferenceValue = container;
        var fields = settings.FindProperty("_damageFields");
        fields.arraySize = 12;
        for (int i = 0; i < 12; i++)
        {
            var hit = new GameObject($"Hit{i + 1}");
            hit.transform.SetParent(_root.transform, false);
            var box = hit.AddComponent<BoxCollider>();
            box.size = Vector3.one * 4;
            box.enabled = false;
            var field = fields.GetArrayElementAtIndex(i);
            field.FindPropertyRelative("_hitbox").objectReferenceValue = box;
            field.FindPropertyRelative("_damage").floatValue = i + 1;
            field.FindPropertyRelative("_hitStopFrame").intValue = 2;
            field.FindPropertyRelative("_staggerLevel").intValue = 2;
            field.FindPropertyRelative("_targetLayers").intValue = 1 << 6;
            field.FindPropertyRelative("_triggerInteraction").intValue = (int)QueryTriggerInteraction.Collide;
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
        var victim = new GameObject("Victim");
        victim.transform.SetParent(_root.transform, false);
        victim.layer = 6;
        victim.AddComponent<BoxCollider>().isTrigger = true;
        victim.AddComponent<SphereCollider>().isTrigger = true;
        _victim = victim.AddComponent<PlayerSkillHitTestActor>();
        _timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        _timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
        _timeline.fixedDuration = 1;
        var track = _timeline.CreateTrack<PlayerSkillHitTrack>();
        for (int i = 0; i < 16; i++)
            track.CreateMarker<PlayerSkillHitMarker>(0.1 + i * 0.02);
        _director.playableAsset = _timeline;
        _director.SetGenericBinding(track, _receiver);
        _root.SetActive(true);
        Physics.SyncTransforms();
    }

    private static void ConfigureFields(AttackHitStopMode mode)
    {
        var settings = new SerializedObject(_receiver);
        var fields = settings.FindProperty("_damageFields");
        for (int i = 0; i < fields.arraySize; i++)
            fields.GetArrayElementAtIndex(i).FindPropertyRelative("_hitStopMode").intValue = (int)mode;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Finish(Exception exception)
    {
        EditorApplication.update -= Tick;
        SessionState.SetBool(Pending, false);
        if (_director != null) _director.Stop();
        if (_root != null) Object.DestroyImmediate(_root);
        if (_timeline != null) DestroyTimeline(_timeline);
        string result = exception == null ? "PlayerSkillHitVerification: PASS (16 marker mapping, scene bindings, actual notifications/damage, repeated Hit12, collider deduplication, attack level, both hit-stop modes, release, replay, scrub, rejected damage)." : exception.ToString();
        File.WriteAllText(Report, result);
        if (exception == null) Debug.Log(result);
        else Debug.LogException(exception);
        if (SessionState.GetBool(StartedPlay, false))
            EditorApplication.ExitPlaymode();
        SessionState.SetBool(StartedPlay, false);
    }

    private static void DestroyTimeline(TimelineAsset timeline)
    {
        foreach (var track in timeline.GetOutputTracks().ToArray())
        {
            foreach (var marker in track.GetMarkers().ToArray())
                Object.DestroyImmediate((Object)marker);
            Object.DestroyImmediate(track);
        }
        Object.DestroyImmediate(timeline);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("PlayerSkillHitVerification: " + message);
    }
}
