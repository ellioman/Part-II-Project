using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class WaveShaderEditor : MaterialEditor
{

    public override void OnInspectorGUI()
    {
        // Draw the default inspector.
        base.OnInspectorGUI();

        if (!isVisible)
        {
            return;
        }

        // get the current keywords from the material
        Material targetMat = target as Material;
        string[] keyWords = targetMat.shaderKeywords;

        // see if redify is set, then show a checkbox
        bool showDebugTexture = keyWords.Contains("SHOW_DEBUG_TEXTURE_ON");
        EditorGUI.BeginChangeCheck();
        showDebugTexture = EditorGUILayout.Toggle("Show debug texture", showDebugTexture);
        if (EditorGUI.EndChangeCheck())
        {
            // if the checkbox is changed, reset the shader keywords
            if (showDebugTexture)
            {
                var keywords = new List<string> { "SHOW_DEBUG_TEXTURE_ON" };
                targetMat.shaderKeywords = keywords.ToArray();
                EditorUtility.SetDirty(targetMat);
            } else
            {
                targetMat.shaderKeywords = new string[0];
                EditorUtility.SetDirty(targetMat);
            }
        }
    }
}
