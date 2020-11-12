using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Stan.Osmos
{
    [CustomEditor(typeof(GameController))]
    public class GameControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save Settings"))
            {
                ((GameController)target).Settings.SaveToFile();
            }

            if (GUILayout.Button("Load Settings"))
            {
                ((GameController)target).Settings.LoadFromFile();
                GUI.FocusControl("empty");
            }
        }
    }
}
