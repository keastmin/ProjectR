using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

public static class PlayerOpacityVerification
{
    // A one-shot request also allows verification after a script refresh without
    // opening or saving the user's scene. No scene objects survive the check.
    private const string RequestPath = "Temp/PlayerOpacityVerification.request";

    [InitializeOnLoadMethod]
    private static void RunRequestedVerification()
    {
        if (!File.Exists(RequestPath))
            return;
        EditorApplication.delayCall += () =>
        {
            File.Delete(RequestPath);
            Validate();
        };
    }

    [MenuItem("Tools/Combat/Validate Player Opacity Timeline")]
    public static void Validate()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        try
        {
            var source = AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/04_Timelines/Player/SkillTimeline.playable");
            var sourceTrack = source.GetOutputTracks().OfType<PlayerOpacityTrack>().Single();
            var sourceClip = sourceTrack.GetClips().Single();
            var track = timeline.CreateTrack<PlayerOpacityTrack>();
            var trackProperties = new SerializedObject(track);
            trackProperties.FindProperty("_fadeMaterial").objectReferenceValue =
                new SerializedObject(sourceTrack).FindProperty("_fadeMaterial").objectReferenceValue;
            trackProperties.ApplyModifiedPropertiesWithoutUndo();
            Check(trackProperties.FindProperty("_fadeMaterial").objectReferenceValue != null, "Build includes transparent material");
            var clip = track.CreateClip<PlayerOpacityClip>();
            clip.start = sourceClip.start;
            clip.duration = sourceClip.duration;
            clip.easeInDuration = sourceClip.easeInDuration;
            clip.easeOutDuration = sourceClip.easeOutDuration;
            clip.mixInCurve = sourceClip.mixInCurve;
            clip.mixOutCurve = sourceClip.mixOutCurve;
            ((PlayerOpacityClip)clip.asset).Opacity = ((PlayerOpacityClip)sourceClip.asset).Opacity;
            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = 2;

            var root = new GameObject("Opacity Verification");
            SceneManager.MoveGameObjectToScene(root, scene);
            var animator = root.AddComponent<Animator>();
            var director = root.AddComponent<PlayableDirector>();
            director.playOnAwake = false;
            director.timeUpdateMode = DirectorUpdateMode.Manual;
            director.playableAsset = timeline;
            director.SetGenericBinding(track, animator);
            Material bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Download Assets/Scythe Animation Pack/Materials/Materials/M_9CGMan.mat");
            Material weaponMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Download Assets/Scythe Animation Pack/Materials/Materials/M_9CGAsset.mat");
            var body = root.AddComponent<SkinnedMeshRenderer>();
            body.sharedMaterials = new[] { bodyMaterial, weaponMaterial };
            var weaponObject = new GameObject("Weapon");
            weaponObject.transform.SetParent(root.transform);
            var weapon = weaponObject.AddComponent<MeshRenderer>();
            weapon.sharedMaterial = weaponMaterial;
            weapon.enabled = false;

            double fadeOutMiddle = clip.start + clip.easeInDuration / 2;
            double hiddenTime = clip.start + clip.easeInDuration + 0.01;
            double fadeInMiddle = clip.end - clip.easeOutDuration / 2;
            Evaluate(director, clip.start - 0.01);
            Check(body.sharedMaterial == bodyMaterial, "Before clip uses original opaque material");
            Evaluate(director, fadeOutMiddle);
            CheckOpacity(body, 0.5f);
            CheckOpacity(weapon, 0.5f);
            Check(body.sharedMaterial.renderQueue == 3000 && body.sharedMaterial.GetFloat("_ZWrite") == 0,
                "Opaque source becomes transparent without writing depth");
            Check(!body.sharedMaterial.GetShaderPassEnabled("ShadowCaster"), "Hidden model leaves no opaque shadow");
            Evaluate(director, hiddenTime);
            CheckOpacity(body, 0f);
            Check(root.activeSelf && animator.enabled && body.enabled && !weapon.enabled,
                "Fade preserves activation, animation and disabled renderers");
            Near(bodyMaterial.GetColor("_BaseColor").a, 1f, "Shared body material remains unchanged");
            Near(weaponMaterial.GetColor("_BaseColor").a, 1f, "Shared weapon material remains unchanged");
            Evaluate(director, fadeInMiddle);
            CheckOpacity(body, 0.5f);
            Evaluate(director, clip.end + 0.01);
            Check(body.sharedMaterial == bodyMaterial && weapon.sharedMaterial == weaponMaterial,
                "After clip restores exact original materials");
            Evaluate(director, hiddenTime);
            Evaluate(director, clip.start - 0.01);
            Check(body.sharedMaterial == bodyMaterial, "Backward scrubbing restores opacity");

            director.Play();
            Evaluate(director, hiddenTime);
            director.Pause();
            CheckOpacity(body, 0f);
            director.Stop();
            Check(body.sharedMaterial == bodyMaterial, "Stopping a paused skill restores materials");
            director.Play();
            Evaluate(director, hiddenTime);
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
            director.playableGraph.Evaluate(0.1f);
            CheckOpacity(body, 0f);
            director.Stop();
            Check(body.sharedMaterial == bodyMaterial, "Repeated playback and hit-stop restore correctly");

            // Edits are evaluated by Timeline instead of hardcoded skill-state timers.
            clip.start = 1;
            clip.duration = 0.5;
            clip.easeInDuration = 0.2;
            clip.easeOutDuration = 0.2;
            ((PlayerOpacityClip)clip.asset).Opacity = 0.2f;
            director.RebuildGraph();
            director.Play();
            Evaluate(director, 1.1);
            CheckOpacity(body, 0.6f);
            Evaluate(director, 1.25);
            CheckOpacity(body, 0.2f);
            root.SetActive(false);
            Check(body.sharedMaterial == bodyMaterial, "Disabling the owner restores materials");
            root.SetActive(true);
            Evaluate(director, 1.25);
            director.Pause();
            root.SetActive(false);
            Check(body.sharedMaterial == bodyMaterial, "Disabling a paused owner restores materials");
            Debug.Log("PlayerOpacityVerification: PASS (asset references, body/weapon, fade curves, scrubbing, pause/stop, replay, hit-stop, clip edits, owner disable).");
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
            foreach (TrackAsset track in timeline.GetOutputTracks().ToArray())
            {
                foreach (TimelineClip clip in track.GetClips())
                    Object.DestroyImmediate(clip.asset);
                Object.DestroyImmediate(track);
            }
            Object.DestroyImmediate(timeline);
        }
    }

    private static void Evaluate(PlayableDirector director, double time)
    {
        director.time = time;
        director.Evaluate();
    }

    private static void CheckOpacity(Renderer renderer, float expected)
    {
        foreach (Material material in renderer.sharedMaterials)
            Near(material.GetColor("_BaseColor").a, expected, renderer.name + " opacity");
    }

    private static void Near(float actual, float expected, string message) =>
        Check(Mathf.Abs(actual - expected) < 0.002f, $"{message}: expected {expected}, got {actual}");

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException("PlayerOpacityVerification: " + message);
    }
}
