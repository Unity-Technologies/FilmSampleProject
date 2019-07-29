
using UnityEditor;
using UnityEngine;

namespace MWU.Shared.Utilities
{
    public static class DistributeEvenly
    {
        [MenuItem("Tools/Edit/Distribute Evenly %#_r")]
        public static void DistributeObjectsEvenly()
        {
            var selectedObjects = Selection.gameObjects;
            var sourcePosition = selectedObjects[0];
            Undo.RegisterCompleteObjectUndo(selectedObjects, "Distribute Evenly");

            float offset = 0f;
            foreach (GameObject selectedObject in selectedObjects)
            {
                selectedObject.transform.position = new Vector3(sourcePosition.transform.position.x, sourcePosition.transform.position.y, sourcePosition.transform.position.z + offset);
                offset += 1f;
            }
        }
    }
}
