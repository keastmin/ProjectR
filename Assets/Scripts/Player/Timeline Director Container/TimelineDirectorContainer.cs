using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineDirectorContainer : MonoBehaviour
{
    [SerializeField] private DirectorInfo[] _directorInfos;

    public readonly Dictionary<DirectorID, PlayableDirector> Directors = new();

    public void InitTimelineDirectorContainer()
    {
        foreach (var info in _directorInfos)
        {
            Directors.Add(info.ID, info.Director);
        }
    }
}