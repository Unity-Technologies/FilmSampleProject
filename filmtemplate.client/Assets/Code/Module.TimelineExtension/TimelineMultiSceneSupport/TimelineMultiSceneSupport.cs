using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using MWU.Attributes;

namespace MWU.Timeline
{

    [System.Serializable]
    public struct DynamicBinding
    {
        public TrackAsset track;
        public string id;
        [System.NonSerialized]
        public System.Type type;
    }

    [RequireComponent(typeof(PlayableDirector))]
    [ExecuteInEditMode]
    public class TimelineMultiSceneSupport : MonoBehaviour
    {
        [ShowAsReadOnly]
        public List<DynamicBinding> bindings = new List<DynamicBinding>();
        private long lastUpdate = -1;

        void OnEnable()
        {
            UpdateBindingList();
            UpdateBindings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            RemoveDynamicBindings();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnValidate()
        {
            UpdateBindingList();
            UpdateBindings();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateBindings();
        }

        void RemoveDynamicBindings()
        {
            var playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector == null || playableDirector.playableAsset == null)
                return;

            foreach (var output in playableDirector.playableAsset.outputs)
            {
                var owner = playableDirector.GetGenericBinding(output.sourceObject) as GameObject;
                var comp = playableDirector.GetGenericBinding(output.sourceObject) as Component;
                if (comp != null)
                    owner = comp.gameObject;

                if (owner != null && owner.scene != playableDirector.gameObject.scene)
                {
                    playableDirector.SetGenericBinding(output.sourceObject, null);
                }
            }
        }

        void UpdateBindingList()
        {
            var playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector == null)
                return;

            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            if (timelineAsset == null)
                return;

            var outputs = timelineAsset.outputs;
            foreach (var output in outputs)
            {
                if (output.outputTargetType == null || !typeof(UnityEngine.Object).IsAssignableFrom(output.outputTargetType) || output.sourceObject == null)
                    continue;

                TrackAsset asset = output.sourceObject as TrackAsset;
                if (asset == null)
                    continue;

                var guid = string.Empty;
                var owner = playableDirector.GetGenericBinding(asset) as GameObject;
                var comp = playableDirector.GetGenericBinding(asset) as Component;
                if (comp != null)
                    owner = comp.gameObject;

                // search for guid component
                if (owner != null && gameObject.scene != owner.scene)
                {
                    var compID = owner.GetComponent<TimelineId>();
                    if (compID == null)
                    {
                        compID = owner.AddComponent<TimelineId>();
#if UNITY_EDITOR
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(owner.scene);
#endif
                    }
                    guid = compID.Id;
                }

                int index = bindings.FindIndex(x => x.track == asset);
                if (index != -1)
                {
                    var s = bindings[index];
                    s.type = output.outputTargetType;
                    if (!string.IsNullOrEmpty(guid))
                        s.id = guid;
                    bindings[index] = s;
                }
                else
                {
                    bindings.Add(new DynamicBinding() { track = asset, id = guid, type = output.outputTargetType });
                }
            }

            bindings.RemoveAll(t => t.track == null);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            UpdateBindingList();
#endif

            if (TimelineId.LastUpdate != lastUpdate)
            {
                UpdateBindings();
                lastUpdate = TimelineId.LastUpdate;
            }
        }

        void UpdateBindings()
        {
            var playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector == null)
                return;

            bool any = false;
            foreach (var entry in bindings)
            {
                if (entry.track == null || string.IsNullOrEmpty(entry.id))
                    continue;

                if (entry.type == null)
                    continue;

                bool isComponent = typeof(Component).IsAssignableFrom(entry.type);
                bool isGameObject = typeof(GameObject).IsAssignableFrom(entry.type);
                if (!isComponent && !isGameObject)
                    continue;

                GameObject actor = null;
                if (!TimelineId.IdMap.TryGetValue(entry.id, out actor))
                    continue;

                UnityEngine.Object binding = actor;
                if (isComponent)
                {
                    binding = actor.GetComponent(entry.type);
                }

                var oldBinding = playableDirector.GetGenericBinding(entry.track);
                if (oldBinding != binding)
                {
                    playableDirector.SetGenericBinding(entry.track, binding);
                    any = true;
                }
            }

            // update the bindings if changed
            if (any && playableDirector.playableGraph.IsValid())
                playableDirector.RebuildGraph();
        }
    }
}