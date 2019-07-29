using UnityEngine;
using UnityEditor;
using MWU.Attributes;

namespace MWU.Attributes
{

    [CustomPropertyDrawer(typeof(ShowAsReadOnlyAttribute))]
    public class ShowAsReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
                EditorGUI.PropertyField(position, property, label, true);
        }
    }
}