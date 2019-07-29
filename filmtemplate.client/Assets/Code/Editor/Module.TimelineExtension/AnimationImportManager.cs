using System;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

using MWU.Timeline;
using MWU.Utilities;

public class AnimationImportManager : SetPlayableBaseEditor
{
    static GameObject selected;
    static List<AnimationClip> animClips = new List<AnimationClip>();

    [NonSerialized]
    bool m_Initialized;
    [SerializeField]
    TreeViewState m_TreeViewState;
    FlatTreeView m_TreeView;

    bool valid = false;

    public static void ShowWindow()
    {
        var win = EditorWindow.GetWindow(typeof(AnimationImportManager)) as AnimationImportManager;
        win.titleContent = new GUIContent(" Import");            
        win.minSize = new Vector2(500,250);
        win.Show();
    }

    Rect topRect
    {
        get { return new Rect(20, 5, position.width - 40, 20); }
    }

    Rect treeViewRect
    {
        get { return new Rect(20, 130, position.width - 40, position.height - 175); }
    }

    Rect bottomRect
    {
        get { return new Rect(20, position.height - 60, position.width - 40, 60); }
    }

    void InitIfNeeded()
    {
        if (!m_Initialized)
        {
            // Check if it already exists (deserialized from window layout file or scriptable object)
            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }
            m_TreeView = new FlatTreeView(m_TreeViewState);
            m_Initialized = true;
            //m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }
    }

    private void OnEnable()
    {
        selected = Selection.activeGameObject;
        var directorList = Resources.FindObjectsOfTypeAll<PlayableDirector>();
        m_directorList = directorList.Where(x => x.gameObject.activeInHierarchy == true).ToArray();
            
        if (m_directorList.Length == 0)
        {
            Debug.LogWarning("No director component found in scene");            
            return;
        }

        // Prep list to be draw in GUI
        Array.Resize(ref m_list, m_directorList.Length);
        for (int i = 0; i < m_directorList.Length; i++)
        {
            var checkDirector = m_directorList[i];
            if (checkDirector.playableAsset != null)
            {
                m_list[i] = m_directorList[i].playableAsset.name;
            }
        }
        m_director = m_directorList[m_retIdx];

        UpdateTrackList();

        valid = true;
    }


    void UpdateTrackList()
    {
        var currentTimeline = m_director.playableAsset as TimelineAsset;
        var tracks = currentTimeline.GetRootTracks().ToList();
        m_animTracks = tracks.Where(y => y.GetType() == typeof(AnimationTrack)).ToList();
        List<string> initTrackList =  new List<string>{ "* Create in New Track" };
        var trackNameList = m_animTracks.Select(z => z.name).ToList();
        initTrackList.AddRange (trackNameList);
        m_tracks = initTrackList.ToArray();

    }

    private void OnGUI()
    {
        if (!valid) { return; }
        InitIfNeeded();

        m_retIdx = EditorGUILayout.Popup("Playable Director", m_retIdx, m_list);
        EditorGUI.BeginChangeCheck();
        m_trackIdx = EditorGUILayout.Popup("Tracks", m_trackIdx, m_tracks);
        if (EditorGUI.EndChangeCheck() && m_trackIdx != 0)
        {
            Selection.activeObject = m_animTracks[m_trackIdx - 1];
        }
        selected = (GameObject)EditorGUILayout.ObjectField("Rig Object", selected, typeof(GameObject), true);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Selected Anim Clip(s)",GUILayout.Height(30)))
        {
            foreach (var obj in Selection.objects)
            {
                var clip = obj as AnimationClip;
                if (clip != null)
                {
                    animClips.Add(clip);
                }
            }
            animClips = animClips.OrderBy(n => n.name).ToList();
            m_TreeView.SetList(animClips);
            animClips.Clear();

        }
        if (GUILayout.Button("Clear List", GUILayout.Height(30)))
        {
            ClearList();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.Label("Loaded animation clip:");

        m_TreeView.OnGUI(treeViewRect);

        GUILayout.BeginArea(bottomRect);
       
        if (GUILayout.Button("IMPORT CLIP", GUILayout.Height(35)))
        {
            if (selected != null && m_directorList[m_retIdx] != null )
            {
                ConstructTimeline();
                EnableAlwaysAnimate(selected);
                ClearList();
            }
            else
            {
                Debug.LogWarning("Missing director or scene object or animation clip");
            }
        }
        GUILayout.EndArea();

    }
   
    void ClearList()
    {
        m_TreeView.m_itemList.Clear();
        m_TreeView.Reload();
    }


    void EnableAlwaysAnimate(GameObject obj)
    {
        var animator = obj.GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    void ConstructTimeline()
    {
        m_director = m_directorList[m_retIdx];

        Undo.RecordObject(m_director.playableAsset, "Import Animation Clip");

        var timeline_asset = m_director.playableAsset as TimelineAsset;

        double marker = 0.0;
        AnimationTrack animTrack = null;

        if (m_trackIdx == 0 && animTrack == null)
        {
            animTrack = timeline_asset.CreateTrack<AnimationTrack>(null, selected.name);
        }
        else
        {
            animTrack = m_animTracks[m_trackIdx - 1] as AnimationTrack;
        }

        foreach (var i in m_TreeView.m_itemList)
        {
            var animClip = i.animclip;

            var newClip = animTrack.CreateClip(animClip);
            m_director.SetGenericBinding(animTrack, selected);
            newClip.displayName = animClip.name;
            newClip.start = marker;
            marker = newClip.end;

            // set exptrapolation to none
            TimelineUtils.SetClipExtrapolationMode(newClip, "preExtrapolationMode", TimelineClip.ClipExtrapolation.None);
            TimelineUtils.SetClipExtrapolationMode(newClip, "postExtrapolationMode", TimelineClip.ClipExtrapolation.None);

        }


        // update timeline window
        TimelineUtils.ToggleLockWindow();
        TimelineUtils.SetTimeline(timeline_asset, m_director);
        Selection.activeObject = m_director.gameObject;
        TimelineUtils.ToggleLockWindow();

        // persist changes
        EditorSceneManager.SaveScene(selected.scene);
        
    }

}


