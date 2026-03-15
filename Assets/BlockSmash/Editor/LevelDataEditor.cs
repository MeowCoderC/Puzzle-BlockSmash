namespace BlockSmash.Editor
{
    using CahtFramework;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;

    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : IdentifiedObjectEditor
    {
        private SerializedProperty predefinedWavesProperty;
        private SerializedProperty availableShapesProperty;
        private SerializedProperty generatorModuleProperty;

        private ThemeColor cachedTheme;
        private int selectedColorIndex = -1; 

        protected override void OnEnable()
        {
            base.OnEnable();
            this.predefinedWavesProperty = this.serializedObject.FindProperty("predefinedWaves");
            this.availableShapesProperty = this.serializedObject.FindProperty("availableShapes");
            this.generatorModuleProperty = this.serializedObject.FindProperty("generatorModule");

            string[] guids = AssetDatabase.FindAssets("t:ThemeColor");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                this.cachedTheme = AssetDatabase.LoadAssetAtPath<ThemeColor>(path);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.serializedObject.Update();
            var levelData = this.target as LevelData;
            levelData.ValidateData();

            if (this.DrawFoldoutTitle("Level Designer"))
            {
                this.DrawColorPalette();
                this.DrawGridPainter(levelData);
            }

            if (this.DrawFoldoutTitle("Random & Waves"))
            {
                EditorGUILayout.PropertyField(this.availableShapesProperty, true);
                if (GUILayout.Button("Load All Shapes")) this.LoadAllShapes();
                EditorGUILayout.PropertyField(this.generatorModuleProperty);
                EditorGUILayout.PropertyField(this.predefinedWavesProperty, true);
                this.DrawWaveVisualizer(levelData);
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorPalette()
        {
            EditorGUILayout.LabelField("Select Color to Paint", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = this.selectedColorIndex == -1 ? Color.yellow : Color.green;
            if (GUILayout.Button("Empty Cell", GUILayout.Width(80), GUILayout.Height(30))) this.selectedColorIndex = -1;

            GUI.backgroundColor = this.selectedColorIndex == -2 ? Color.yellow : Color.gray;
            if (GUILayout.Button("Hole", GUILayout.Width(60), GUILayout.Height(30))) this.selectedColorIndex = -2;

            GUI.backgroundColor = oldBg;
            EditorGUILayout.EndHorizontal();

            if (this.cachedTheme != null)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < this.cachedTheme.Sprites.Count; i++)
                {
                    var sprite = this.cachedTheme.Sprites[i];
                    var rect = GUILayoutUtility.GetRect(35, 35);
                    if (this.selectedColorIndex == i) Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.yellow);
                    if (GUI.Button(rect, sprite.texture)) this.selectedColorIndex = i;
                    if ((i + 1) % 8 == 0) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawGridPainter(LevelData levelData)
        {
            int newSize = EditorGUILayout.IntSlider("Grid Size", levelData.GridSize, 4, 12);
            if (newSize != levelData.GridSize) { Undo.RecordObject(levelData, "Resize"); levelData.ResizeGrid(newSize); }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill Playable")) levelData.FillAll(true, this.selectedColorIndex < 0 ? -1 : this.selectedColorIndex);
            if (GUILayout.Button("Clear All Holes")) levelData.FillAll(false, -1);
            EditorGUILayout.EndHorizontal();

            float btnSize = 35f;
            for (int y = 0; y < levelData.GridSize; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < levelData.GridSize; x++)
                {
                    bool isPlayable = levelData.GetCell(x, y);
                    int colorIdx = levelData.GetBlockColor(x, y);
                    
                    if (!isPlayable)
                    {
                        GUI.backgroundColor = Color.black;
                        if (GUILayout.Button("", GUILayout.Width(btnSize), GUILayout.Height(btnSize))) this.PaintCell(levelData, x, y);
                    }
                    else if (colorIdx >= 0 && this.cachedTheme != null && colorIdx < this.cachedTheme.Sprites.Count)
                    {
                        if (GUILayout.Button(this.cachedTheme.Sprites[colorIdx].texture, GUILayout.Width(btnSize), GUILayout.Height(btnSize)))
                            this.PaintCell(levelData, x, y);
                    }
                    else
                    {
                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("", GUILayout.Width(btnSize), GUILayout.Height(btnSize))) this.PaintCell(levelData, x, y);
                    }
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void PaintCell(LevelData data, int x, int y)
        {
            Undo.RecordObject(data, "Paint Cell");
            if (this.selectedColorIndex == -2) data.SetBlockData(x, y, false, -1);
            else if (this.selectedColorIndex == -1) data.SetBlockData(x, y, true, -1);
            else data.SetBlockData(x, y, true, this.selectedColorIndex);
            EditorUtility.SetDirty(data);
        }

        private void LoadAllShapes()
        {
            string[] guids = AssetDatabase.FindAssets("t:Shape");
            this.availableShapesProperty.ClearArray();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                this.availableShapesProperty.InsertArrayElementAtIndex(i);
                this.availableShapesProperty.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Shape>(path);
            }
        }

        private void DrawWaveVisualizer(LevelData levelData)
        {
            if (levelData.PredefinedWaves == null) return;
            foreach (var wave in levelData.PredefinedWaves)
            {
                EditorGUILayout.BeginHorizontal("box");
                foreach (var s in wave.shapes)
                {
                    if (s == null) continue;
                    var r = GUILayoutUtility.GetRect(40, 40);
                    BlockSmashSystemEditor.DrawMiniShapeInRect(s, r);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}