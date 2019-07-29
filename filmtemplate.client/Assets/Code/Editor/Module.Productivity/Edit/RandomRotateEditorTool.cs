
using UnityEditor;
using UnityEngine;

namespace MWU.Shared.Utilities
{
    public static class RandomRotateEditorTool
    {
        [MenuItem("Tools/Edit/Random Rotate Y %#_y")]
        public static void RandomRotateSelectionAroundY()
        {
            var selectedObjects = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(selectedObjects, "Random Rotate Y");

            foreach (GameObject selectedObject in selectedObjects) {
                float randomAngle = Random.Range(0f, 360f);
                selectedObject.transform.Rotate(Vector3.up, randomAngle, Space.Self);
            }
        }
    }
}
