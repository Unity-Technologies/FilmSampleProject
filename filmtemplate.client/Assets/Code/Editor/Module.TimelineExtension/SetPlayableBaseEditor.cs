using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MWU.Timeline
{

    public abstract class SetPlayableBaseEditor : EditorWindow
    {
        public static PlayableDirector[] m_directorList;
        public static PlayableDirector m_director = null;
        public static List<TrackAsset> m_animTracks;
        public static TrackAsset m_animTrack = null;

        public static string[] m_list;
        public static string[] m_tracks;
        public static int m_retIdx = 0;
        public static int m_trackIdx = 0;
        public static bool b_custom = false;
        public static string m_customPlayableName = "";
        public static int m_playableNameIdx = 0;
        public enum m_playableName { Custom, Scene, Object };

    }
}