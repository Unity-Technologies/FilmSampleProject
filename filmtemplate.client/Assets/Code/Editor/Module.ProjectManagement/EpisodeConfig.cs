using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

namespace MWU.Shared
{

    [CreateAssetMenu(fileName = "EpisodeConfig", menuName = "Film Sample/Episode Config", order = 2)]
    public class EpisodeConfig : ScriptableObject
    {
		[Header("Main Scenes")]
		[SerializeField] Object[] mainScenes;

		[Header("Set Scenes")]
		[SerializeField] Object[] setScenes;

		public void LoadAllScenes()
		{
			if (mainScenes.Length == 0)
			{
				Debug.LogError("No Main scenes have been specified. Loading failed.");
				return;
			}

			EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(mainScenes[0]), OpenSceneMode.Single);

			for(int i = 1; i < mainScenes.Length; i++)
				EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(mainScenes[i]), OpenSceneMode.Additive);

			LoadSetScenes(false);
		}

		public void LoadSetScenes(bool unloadOtherScenes)
		{
			if (setScenes.Length == 0)
			{
				if (unloadOtherScenes)
					Debug.LogError("No Set scene have been specified. Loading failed.");

				return;
			}

			OpenSceneMode mode = OpenSceneMode.Additive;

			if (unloadOtherScenes)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				mode = OpenSceneMode.Single;
			}

			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(setScenes[0]), mode);

			for (int i = 1; i < setScenes.Length; i++)
				EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(setScenes[i]), OpenSceneMode.Additive);

			SetLightingSceneAsActive();
		}

		void SetLightingSceneAsActive()
		{
			for (int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				Scene scene = EditorSceneManager.GetSceneAt(i);
				if (Regex.IsMatch(scene.name, "lighting", RegexOptions.IgnoreCase))
				{
					EditorSceneManager.SetActiveScene(scene);
					return;
				}
			}
		}
	}
}