using UnityEngine; 
using UnityEditor;

namespace MWU.Shared.Utilities
{

    public class AutoSnap : EditorWindow
    {
        private Vector3 prevPosition;
        private Vector3 prevScale;
        private Vector3 prevRotation;

        // These need to be static because the auto snap window is
        // recreated when opened from the menu
        private static bool doSnap = false;
        private static bool doScaleSnap = false;
        private static bool doRotateSnap = false;
        private static float snapValueX = 1;
        private static float snapValueY = 1;
        private static float snapValueZ = 1;
        private static float snapRotateValue = 1;

        // We need to remember the window doing the snap updates
        // so it will do it when the window is closed
        private static AutoSnap updateWindow = null;

        private const string doSnapKey = "AutoSnap_doSnapKey";
        private const string doScaleSnapKey = "AutoSmap_doScaleSnapKey";
        private const string doRotateSnapKey = "AutoSnap_doRotateSnapKey";
        private const string snapValueXKey = "AutoSnap_snapValueXKey";
        private const string snapValueYKey = "AutoSnap_snapValueYKey";
        private const string snapValueZKey = "AutoSnap_snapValueZKey";
        private const string snapRotateValueKey = "AutoSnap_snapRotateValueKey";

        [MenuItem("Tools/Edit/Auto Snap %_l")]

        static void Init()
        {
            // Debug.Log("AutoSnap: Init");

            AutoSnap window = (AutoSnap)EditorWindow.GetWindow(typeof(AutoSnap));
            window.maxSize = new Vector2(200, 125);
            window.Show();
        }

        public void OnGUI()
        {
            // Debug.Log("AutoSnap: OnGUI");

            doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
            doScaleSnap = EditorGUILayout.Toggle("Auto Snap Scale", doScaleSnap);
            doRotateSnap = EditorGUILayout.Toggle("Auto Snap Rotation", doRotateSnap);

            snapValueX = EditorGUILayout.FloatField("Snap X Value", snapValueX);
            snapValueY = EditorGUILayout.FloatField("Snap Y Value", snapValueY);
            snapValueZ = EditorGUILayout.FloatField("Snap Z Value", snapValueZ);

            snapRotateValue = EditorGUILayout.FloatField("Rotation Snap Value", snapRotateValue);
        }

        public void Update()
        {
            if (!EditorApplication.isPlaying && Selection.transforms.Length > 0)
            {
                // Debug.Log("AutoSnap: Update");

                if (doSnap
                    && Selection.transforms[0].position != prevPosition)
                {
                    Snap();
                    prevPosition = Selection.transforms[0].position;
                }

                if (doScaleSnap
                    && Selection.transforms[0].localScale != prevScale)
                {
                    ScaleSnap();
                    prevScale = Selection.transforms[0].localScale;
                }

                if (doRotateSnap
                    && Selection.transforms[0].eulerAngles != prevRotation)
                {
                    RotateSnap();
                    prevRotation = Selection.transforms[0].eulerAngles;
                }
            }
        }

        private void Snap()
        {
            // Debug.Log("AutoSnap: Snap");

            foreach (var transform in Selection.transforms)
            {
                var t = transform.transform.position;
                t.x = RoundX(t.x);
                t.y = RoundY(t.y);
                t.z = RoundZ(t.z);
                transform.transform.position = t;
            }
        }

        private void ScaleSnap()
        {
            // Debug.Log("AutoSnap: ScaleSnap");

            foreach (var transform in Selection.transforms)
            {
                var s = transform.transform.localScale;
                s.x = ScaleRoundX(s.x);
                s.y = ScaleRoundY(s.y);
                s.z = ScaleRoundZ(s.z);
                transform.transform.localScale = s;
            }
        }

        private void RotateSnap()
        {
            // Debug.Log("AutoSnap: RotateSnap");

            foreach (var transform in Selection.transforms)
            {
                var r = transform.transform.eulerAngles;
                r.x = RotateRound(r.x);
                r.y = RotateRound(r.y);
                r.z = RotateRound(r.z);
                transform.transform.eulerAngles = r;
            }
        }

        private float RoundX(float input)
        {
            return snapValueX * Mathf.Round((input / snapValueX));
        }

        private float RoundY(float input)
        {
            return snapValueY * Mathf.Round((input / snapValueY));
        }

        private float RoundZ(float input)
        {
            return snapValueZ * Mathf.Round((input / snapValueZ));
        }

        private float ScaleRoundX(float input)
        {
            return snapValueX * Mathf.Round((input / snapValueX));
        }

        private float ScaleRoundY(float input)
        {
            return snapValueY * Mathf.Round((input / snapValueY));
        }

        private float ScaleRoundZ(float input)
        {
            return snapValueZ * Mathf.Round((input / snapValueZ));
        }

        private float RotateRound(float input)
        {
            return snapRotateValue * Mathf.Round((input / snapRotateValue));
        }

        public void OnEnable()
        {
            // Debug.Log("AutoSnap: OnEnable");

            if (EditorPrefs.HasKey(doSnapKey))
            {
                doSnap = EditorPrefs.GetBool(doSnapKey);
            }

            if (EditorPrefs.HasKey(doScaleSnapKey))
            {
                doScaleSnap = EditorPrefs.GetBool(doScaleSnapKey);
            }

            if (EditorPrefs.HasKey(doRotateSnapKey))
            {
                doRotateSnap = EditorPrefs.GetBool(doRotateSnapKey);
            }

            if (EditorPrefs.HasKey(snapValueXKey))
            {
                snapValueX = EditorPrefs.GetFloat(snapValueXKey);
            }

            if (EditorPrefs.HasKey(snapValueYKey))
            {
                snapValueY = EditorPrefs.GetFloat(snapValueYKey);
            }

            if (EditorPrefs.HasKey(snapValueZKey))
            {
                snapValueZ = EditorPrefs.GetFloat(snapValueZKey);
            }

            if (EditorPrefs.HasKey(snapRotateValueKey))
            {
                snapRotateValue = EditorPrefs.GetFloat(snapRotateValueKey);
            }

            // Here, if a previous auto snap window was doing updates,
            // remove it
            if (updateWindow != null)
            {
                EditorApplication.update -= updateWindow.Update;
            }

            // Now make this window handle the updates
            EditorApplication.update += Update;
        }

        public void OnDisable()
        {
            Debug.Log("AutoSnap: OnDisable");

            EditorPrefs.SetBool(doSnapKey, doSnap);
            EditorPrefs.SetBool(doScaleSnapKey, doScaleSnap);
            EditorPrefs.SetBool(doRotateSnapKey, doRotateSnap);
            EditorPrefs.SetFloat(snapValueXKey, snapValueX);
            EditorPrefs.SetFloat(snapValueYKey, snapValueY);
            EditorPrefs.SetFloat(snapValueZKey, snapValueZ);
            EditorPrefs.SetFloat(snapRotateValueKey, snapRotateValue);

            // Remember we're doing the updates
            updateWindow = this;

            // Normally you'd remove the window, however we don't
            // as we want the snapping to continue when this window is closed
            //EditorApplication.update -= Update;
        }
    }
}