using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MWU.Timeline;

public class TimelineToolsMenu
{
    [MenuItem("Tools/Timeline/Align 'selected Clips' to Head", false, 2)]
    static void AlignToHead()
    {
        TimelineUtils.AlignClipsToHead(Selection.objects);
    }

    [MenuItem("Tools/Timeline/Align 'selected Clips' to Tail", false, 2)]
    static void AlignToTail()
    {
        TimelineUtils.AlignClipsToTail(Selection.objects);
    }

    [MenuItem("Tools/Timeline/Snap to Previous Clip", false, 2)]
    static void SnapToPrevious()
    {
        TimelineUtils.SnapToPrevious(Selection.objects);
    }

    [MenuItem("Tools/Timeline/Snap to Next Clip", false, 2)]
    static void SnapToNext()
    {
        TimelineUtils.SnapToNext(Selection.objects);
    }

    [MenuItem("Tools/Timeline/Replace Animation Clip")]
    static void ReplaceAnim()
    {
        ReplaceAnimClipEditor.ShowWindow();
    }

    [MenuItem("Tools/Timeline/Animation Import Manager")]
    static void ImportManager()
    {
        AnimationImportManager.ShowWindow();
    }

    [MenuItem("Tools/Timeline/Toggle Lock Timeline Window %T", false, 2)]
    static void ToggleLockTimelineWin()
    {
        TimelineUtils.ToggleLockWindow();
    }


}
