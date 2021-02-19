using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VATGenerator))]
public class VATGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Generate Texture"))
        {
            foreach (var o in targets)
            {
                var t = (VATGenerator) o;
                t.Generate();
            }
        }
        
        if (GUILayout.Button("Print Texture Values"))
        {
            foreach (var o in targets)
            {
                var t = (VATGenerator) o;
                t.PrintTexture();
            }
        }
    }
}
