using UnityEngine;
using UnityEditor;

namespace MWU.Shared
{
	[CustomEditor(typeof(EpisodeConfig))]
	public class EpisodeConfigEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EpisodeConfig config = (EpisodeConfig)target;

			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (GUILayout.Button("Load Full Workflow", GUILayout.MinHeight(100), GUILayout.Height(50)))
				config.LoadAllScenes();
			
			EditorGUILayout.Space();

			if (GUILayout.Button("Load Set Only", GUILayout.MinHeight(100), GUILayout.Height(50)))
				config.LoadSetScenes(true);
		}
	}
}