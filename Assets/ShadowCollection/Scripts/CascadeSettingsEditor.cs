using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace MyCascaedShadow
{
    [CustomEditor(typeof(ShadowCascades))]
    public class ShadowCascaedEditor : Editor
    {


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            var tar = (ShadowCascades)target;





            tar.Display = GUILayout.Toggle(tar.Display, "Display Corners", "Button");



            if (GUILayout.Button("CalculateTest"))
            {
   
            }




        }
    }
}
