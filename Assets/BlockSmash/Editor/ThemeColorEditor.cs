namespace BlockSmash.Editor
{
    using CahtFramework;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ThemeColor))]
    public class ThemeColorEditor : IdentifiedObjectEditor
    {
        private SerializedProperty spritesProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.spritesProperty = this.serializedObject.FindProperty("sprites");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            this.serializedObject.Update();

            if (this.DrawFoldoutTitle("Theme Sprites"))
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(this.spritesProperty, new GUIContent("Sprites List"), true);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}