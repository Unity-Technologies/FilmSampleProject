using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor;

namespace MWU.Timeline
{
    public static class TimelineUtils
    {
        /// <summary>
        /// Creation methods
        /// </summary>
        public static TimelineAsset CreateUniqueTimelineAsset(string timelineAssetPath)
        {
            var timeline_asset = ScriptableObject.CreateInstance<TimelineAsset>();
            var uniquePlayableName = AssetDatabase.GenerateUniqueAssetPath(timelineAssetPath);
            AssetDatabase.CreateAsset(timeline_asset, uniquePlayableName);

            return timeline_asset;
        }

        public static GameObject CreatePlayableDirectorObject(string name = null)
        {
            var gameObj = new GameObject(name);
            gameObj.AddComponent<PlayableDirector>();

            return gameObj;
        }
        public static TimelineClip CreateAnimClipInTrack(string trackName, TimelineAsset timeline)
        {
            var track = timeline.CreateTrack<AnimationTrack>(null, trackName);
            var clip = track.CreateDefaultClip();

            return clip;
        }

        public static PlayableDirector SetPlayableAsset(GameObject go, TimelineAsset asset)
        {
            var dir = go.GetComponent<PlayableDirector>();
            dir.playableAsset = asset;

            return dir;
        }


        /// <summary>
        /// For organizing/ manipulating timeline track/ clip
        /// </summary>
        public static void Reorder_B_below_A(List<TrackAsset> tracks, TrackAsset a, TrackAsset b)
        {
            var a_idx = tracks.IndexOf(a);
            var b_idx = tracks.IndexOf(b);

            var head_idx = a_idx + 1;
            TrackAsset tmp = b;

            for (int i = head_idx; i < b_idx; i++)
            {
                var current = tracks[i];
                tracks[i] = tmp;
                tmp = current;
            }

            tracks[b_idx] = tmp;
        }

        public static void AlignClipsToHead(UnityEngine.Object[] objects)
        {
            Undo.RecordObjects(objects, "Align Clips to Head");

            List<double> startTimeList = new List<double>();
            List<TimelineClip> clipList = new List<TimelineClip>();

            foreach (var obj in objects)
            {
                var selectedClip = TimelineUtils.GetClip(obj);
                clipList.Add(selectedClip);
                startTimeList.Add(selectedClip.start);
            }

            double lowest = startTimeList.Min();

            foreach (var clip in clipList)
            {
                clip.start = lowest;
            }
            TimelineUtils.GetTimelineWindow().Repaint();
        }

        public static void AlignClipsToTail(UnityEngine.Object[] objects)
        {
            Undo.RecordObjects(objects, "Align Clips to Tail");

            List<double> startTimeList = new List<double>();
            List<TimelineClip> clipList = new List<TimelineClip>();

            foreach (var obj in objects)
            {
                var selectedClip = TimelineUtils.GetClip(obj);
                clipList.Add(selectedClip);
                startTimeList.Add(selectedClip.end);
            }

            double highest = startTimeList.Max();

            foreach (var clip in clipList)
            {
                clip.start = highest - clip.duration;
            }
            TimelineUtils.GetTimelineWindow().Repaint();
        }

        public static void SnapToPrevious(UnityEngine.Object[] objects)
        {
            Undo.RecordObjects(objects, "Snap To Previous's End");
            List<TimelineClip> selectedClips = new List<TimelineClip>();
            for (int i = 0; i < objects.Length; i++)
            {
                var selectedClip = TimelineUtils.GetClip(objects[i]);
                selectedClips.Add(selectedClip);
            }

            selectedClips = selectedClips.OrderBy(c => c.start).ToList();

            for (int i = 0; i < selectedClips.Count; i++)
            {
                var selectedClip = selectedClips[i];
                var selectedTrack = GetTrackBasedOnClip(selectedClip);
                var clipsInTrack = (Array)selectedTrack.GetClips();
                var index = Array.IndexOf(clipsInTrack, selectedClip);

                // if found
                if (index > -1)
                {
                    // and not the first clip in track
                    if (index != 0)
                    {
                        var prevClip = clipsInTrack.GetValue(index - 1) as TimelineClip;
                        selectedClip.start = prevClip.end;
                    }
                }
                else { UnityEngine.Debug.LogWarning("Clip not found"); }

            }
            TimelineUtils.GetTimelineWindow().Repaint();
        }

        public static void SnapToNext(UnityEngine.Object[] objects)
        {
            Undo.RecordObjects(objects, "Snap To Next's Start");
            List<TimelineClip> selectedClips = new List<TimelineClip>();
            for (int i = 0; i < objects.Length; i++)
            {
                var selectedClip = TimelineUtils.GetClip(objects[i]);
                selectedClips.Add(selectedClip);
            }

            selectedClips = selectedClips.OrderBy(c => c.start).ToList();
            selectedClips.Reverse();

            for (int i = 0; i < selectedClips.Count; i++)
            {
                var selectedClip = selectedClips[i];
                var selectedTrack = GetTrackBasedOnClip(selectedClip);
                var clipsInTrack = (Array)selectedTrack.GetClips();
                var index = Array.IndexOf(clipsInTrack, selectedClip);

                // if found
                if (index > -1)
                {
                    // and not the last clip in track
                    if (index != clipsInTrack.Length - 1)
                    {
                        var nextClip = clipsInTrack.GetValue(index + 1) as TimelineClip;
                        selectedClip.start = nextClip.start - selectedClip.duration;
                    }
                }
                else { UnityEngine.Debug.LogWarning("Clip not found"); }

            }
            TimelineUtils.GetTimelineWindow().Repaint();
        }


        public static void ControlTrackSetGameObject(PlayableDirector director, ControlPlayableAsset clip, GameObject obj)
        {
            director.playableGraph.GetResolver().SetReferenceValue(clip.sourceGameObject.exposedName, obj);
        }

        /// <summary>
        /// All manipulation through reflection
        /// Highly prompt to changes, use at risk
        /// </summary>
        public static void SetClipExtrapolationMode(TimelineClip clip, string propertyName, TimelineClip.ClipExtrapolation mode)
        {
            var pro = clip.GetType().GetProperty(propertyName);

            if (pro == null)
            {


                UnityEngine.Debug.LogWarning("Error in getting property: " + clip + "." + propertyName);
                return;
            }
            pro.SetValue(clip, Convert.ChangeType(mode, pro.PropertyType), null);
        }

        public static List<TrackAsset> GetTimelineTracks(TimelineAsset timeline)
        {
            // locate track list in timeline
            var propTracks = timeline.GetType().GetProperty("tracks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return propTracks.GetValue(timeline, null) as List<TrackAsset>;
        }

        public static TrackAsset GetTrack(System.Object obj)
        {
#if UNITY_2017_2_OR_NEWER
            var clip = GetClip(obj);
            return clip.parentTrack;
#else
        var trackProp = obj.GetType().GetProperty("track", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return trackProp.GetValue(obj, null) as TrackAsset;
#endif
        }

        public static TrackAsset GetTrackBasedOnClip(TimelineClip clip)
        {
            var trackProp = clip.GetType().GetProperty("parentTrack", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return trackProp.GetValue(clip, null) as TrackAsset;
        }
        public static TimelineClip GetClip(System.Object obj)
        {
            var clipProp = obj.GetType().GetProperty("clip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return clipProp.GetValue(obj, null) as TimelineClip;
        }

        public static TimelineAsset GetCurrentActiveTimeline()
        {
            var timeline_win = TimelineUtils.GetTimelineWindow();
            var asset = timeline_win.GetType().GetProperty("timeline");
            var timeline = asset.GetValue(timeline_win, null) as TimelineAsset;

            return timeline;
        }

        public static void SetTimeline(TimelineAsset timeline, PlayableDirector director)
        {
            var window = TimelineUtils.GetTimelineWindow();
#if UNITY_2018_2_OR_NEWER

            var method = window.GetType().GetMethod("SetCurrentTimeline", new[] { typeof(PlayableDirector), typeof(TimelineClip) });
            var retVal = method.Invoke(window, new object[] { director, null });
#else
        var method = window.GetType().GetMethod("SetCurrentTimeline", new [] { typeof(TimelineAsset), typeof(PlayableDirector) });
        method.Invoke(window, new object[]{ timeline, director});
#endif
            Verify(retVal);
        }



        public static PlayableDirector GetDirectorFromTimeline(TimelineAsset timeline)
        {
            var directors = Resources.FindObjectsOfTypeAll<PlayableDirector>();
            var director = Array.Find(directors, d => d.playableAsset == timeline);

            return director;
        }

        public static void SetPlayheadByFrame(PlayableDirector director, float fps, double gotoFrame)
        {
            var setTimeMethod = director.GetType().GetMethod("set_time");
            var retVal = setTimeMethod.Invoke(director, new object[] { gotoFrame / fps });

            Verify(retVal);

        }

        public static void SetPlayheadBySeconds(PlayableDirector director, double gotoTime)
        {
            var setTimeMethod = director.GetType().GetMethod("set_time");
            var retVal = setTimeMethod.Invoke(director, new object[] { gotoTime });

            Verify(retVal);
        }

        /// <summary>
        /// Misc - mainly Timeline Editor Window related
        /// </summary>
        public static EditorWindow GetTimelineWindow()
        {
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
#else
            EditorApplication.ExecuteMenuItem("Window/Timeline Editor");
#endif
            var timelineWindowType = Assembly.Load("UnityEditor.Timeline").GetType("UnityEditor.Timeline.TimelineWindow");

            // Assuming there always only one timeline window
            var timeline_window = Resources.FindObjectsOfTypeAll(timelineWindowType)[0] as EditorWindow;
            return timeline_window;
        }

        public static void ToggleLockWindow()
        {
            var window = TimelineUtils.GetTimelineWindow();
            PropertyInfo propertyInfo = window.GetType().GetProperty("locked");
            bool value = (bool)propertyInfo.GetValue(window, null);
            propertyInfo.SetValue(window, !value, null);
            window.Repaint();
        }

        public static void Verify(System.Object obj)
        {
            if (obj != null)
            {
                StackTrace stackTrace = new StackTrace();
                UnityEngine.Debug.Log("Error in TimelineUtils: " + stackTrace.GetFrame(1).GetMethod().Name);
            }
        }
    }

}