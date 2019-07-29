using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace MWU.Editor.Utilities
{

    [InitializeOnLoad]
    public class ReadMe : EditorWindow
    {
		public static readonly string kLoadLayout = "MWU.FilmSample.ReadMe.LoadLayout";
		public static readonly string kShowOnStart = "MWU.FilmSample.ReadMe.ShowOnStart";
        public static readonly int kShowOnStartCookie = 1;

        public static readonly string kShownThisSession = "MWU.FilmSample.ShownThisSession";
		public static readonly string kDefaultSceneConfig = "Assets/Scenes/FilmSampleV1/FilmSample - 001 Scene Loader - Click Me.asset";

		static string _salt;

        static string salt
        {
            get
            {
                if (string.IsNullOrEmpty(_salt))
                    _salt = Application.dataPath.GetHashCode().ToString("MWU");
                return _salt;
            }
        }

        static ReadMe()
        {
            EditorApplication.delayCall += () =>
            {
                CheckPrefsAndShow();
                EnsureLinearProject();
            };
        }

        static string ParseMarkdown(string path)
        {
            // strict markdown subset:
            var h1 = new Regex(@"^\=+\s*$");
            var h2 = new Regex(@"^\-+\s*$");
            var hr = new Regex(@"^\*+\s*$");
            var li = new Regex(@"^\-\s+(.*)$");

            var lines = new Queue<string>(File.ReadAllLines(path));
            var builder = new StringBuilder();

            bool list = true;
            Action endList = () =>
            {
                if (list)
                    builder.Append('\n');
                list = false;
            };

            while (lines.Count > 0)
            {
                var line = lines.Dequeue();
                Match match;

                if (string.IsNullOrEmpty(line))
                {
                    endList();
                    continue;
                }
                else if (hr.IsMatch(line))
                {
                    endList();
                    builder.Append('\n');
                    for (int i = 0; i < 20; ++i)
                        builder.Append("\u2e3b");
                    builder.Append("\n\n");
                    continue;
                }
                else if ((match = li.Match(line)).Success)
                {
                    builder.AppendFormat(" \u2022 {0}\n", match.Groups[1].Captures[0].Value);
                    list = true;
                    continue;
                }
                else if (lines.Count > 0)
                    if (h1.IsMatch(lines.Peek()))
                    {
                        lines.Dequeue();
                        endList();
                        builder.AppendFormat("\n<size=24><b>{0}</b></size>\n\n", line);
                        continue;
                    }
                    else if (h2.IsMatch(lines.Peek()))
                    {
                        lines.Dequeue();
                        endList();
                        builder.AppendFormat("\n<size=18><b>{0}</b></size>\n\n", line);
                        continue;
                    }

                builder.AppendFormat("{0}\n\n", line);
            }

            return builder.ToString();
        }

        static void CheckPrefsAndShow()
        {
            int cookie = EditorPrefs.GetInt(kShowOnStart + salt, defaultValue: 0);
            if (cookie < kShowOnStartCookie && !SessionState.GetBool(kShownThisSession, defaultValue: false))
            {
                Show();
            }
        }

        [MenuItem("Tools/About Film Sample Project", false, -100)]
        public static new void Show()
        {
            ((ReadMe)ScriptableObject.CreateInstance(typeof(ReadMe))).ShowUtility();
            SessionState.SetBool(kShownThisSession, true);
        }

        [PreferenceItem("Read Me")]
        static void OnPrefsGUI()
        {
            int cookie = EditorPrefs.GetInt(kShowOnStart + salt, defaultValue: 0);
            bool showOnStart = cookie < kShowOnStartCookie;

            if (EditorGUILayout.Toggle("Show On Start", showOnStart) != showOnStart)
                EditorPrefs.SetInt(kShowOnStart + salt, showOnStart ? kShowOnStartCookie : 0);
        }

        GUIStyle _style;
        Vector2 _scroll;
        string _text;

        protected void OnEnable()
        {
            titleContent = new GUIContent("About");
            minSize = new Vector2(640, 320);
            maxSize = new Vector2(1280, 960);

            try
            {
                _text = ParseMarkdown("Assets/ReadMe.md");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _text = e.Message;
            }
        }

        protected void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(EditorStyles.textArea);
                _style.richText = true;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.TextArea(_text, _style, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            int cookie = EditorPrefs.GetInt(kShowOnStart + salt, defaultValue: 0);
            bool showOnStart = cookie < kShowOnStartCookie;

			GUILayout.BeginHorizontal();

			if (EditorGUILayout.ToggleLeft("Show On Start", showOnStart) != showOnStart)
                EditorPrefs.SetInt(kShowOnStart + salt, showOnStart ? kShowOnStartCookie : 0);

			bool loadEditorLayout = EditorPrefs.GetBool(kLoadLayout + salt, defaultValue: true);
			loadEditorLayout = EditorGUILayout.ToggleLeft("Load Film Template Editor Layout", loadEditorLayout);
			EditorPrefs.SetBool(kLoadLayout + salt, loadEditorLayout);

			GUILayout.EndHorizontal();

			if (GUILayout.Button("Load Film Template Master Scene", GUILayout.MinHeight(100), GUILayout.Height(50)))
            {
				MWU.Shared.EpisodeConfig config = AssetDatabase.LoadAssetAtPath<MWU.Shared.EpisodeConfig>(kDefaultSceneConfig);

				if (config == null)
				{
					Debug.LogError("Error: could not locate the file \"" + kDefaultSceneConfig + "\". File has been moved or renamed.");
					return;
				}

				config.LoadAllScenes();

				if(loadEditorLayout)
					MWU.Layout.LayoutLoader.LoadFilmLayout();
				else
					Close();
            }
        }

        static void EnsureLinearProject()
        {
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                Debug.Log("Forcing project to Linear colorspace.");

                PlayerSettings.colorSpace = ColorSpace.Linear;
                AssetDatabase.SaveAssets();
            }
        }

    }
}