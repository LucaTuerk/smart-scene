using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[Serializable]
public class FloatMinMaxToGroupMaterial : VertexGroupMaterial
{
    int attr = 0;

    float min;
    float max;
    float selectorMin = 0.0f;
    float selectorMax = 1000.0f;
    bool excludeMin = false;
    bool excludeMax = false;

    String readFrom = "";
    String writeTo = "";

    bool chooseExisting = false;

    public FloatMinMaxToGroupMaterial (String name) : base(name) {}

    public override void Bake( GridMesh mesh ) {
        colors = new Color[mesh.Size];

        List<int> result = new List<int>();

        if (readFrom != null)
        {
            float[] values = 
                SmartSceneWindow.db.floatVertexAttributes[readFrom];

            if (values != null) {

                for ( int i = 0; i < colors.Length; i++ ) {
                    if ( 
                        ( ( !excludeMin && values[i] >= min ) || ( excludeMin && values[i] > min ) )
                        && 
                        ( ( !excludeMax && values[i] <= max ) || ( excludeMax && values[i] < max ) ) ){
                        result.Add(i);
                        colors[i] = new Color(1,1,1,1);
                    }
                    else {
                        colors[i] = new Color(0,0,0,0);
                    }
                }

            }
        }


        mesh.Mesh.colors = colors;
        this.mesh = mesh;

        if ( writeTo != "" ) {
            SmartSceneWindow.db.AddOnGridVertexGroup( writeTo, result.ToArray());
        }
        
        isBaked = true;
    }

    public override void DrawGUI() {

        GUILayout.Label( "Float Attribute: ");
        if ( chooseExisting ) {
            attr = SmartSceneWindow.db.FloatAttributeSelectGUI(attr);
            if ( attr < SmartSceneWindow.db.floatVertexAttributes.Count ) 
             readFrom = SmartSceneWindow.db.floatVertexAttributes.ElementAt(attr).Key;
        }
        else {
            readFrom = GUILayout.TextField(readFrom);
        }

        GUILayout.Space(10);
        chooseExisting = GUILayout.Toggle(chooseExisting, "Choose from existing attributes (vs by name).");
        GUILayout.Space(20);

        EditorGUILayout.MinMaxSlider( ref min, ref max, selectorMin, selectorMax );

        GUILayout.Label("min: " + min + ", max: " + max);
        GUILayout.Label("limits: [" + selectorMin + ", " + selectorMax + "]");
        excludeMin = GUILayout.Toggle( excludeMin, "Exclude Minimum");
        excludeMax = GUILayout.Toggle( excludeMax, "Exclude Maximum");

        if ( GUILayout.Button("Set selector MinMax from data") ) {
            if (readFrom != null)
            {
                float[] values = 
                    SmartSceneWindow.db.floatVertexAttributes[readFrom];

                if (values != null) {
                    selectorMin = float.PositiveInfinity;
                    selectorMax = float.NegativeInfinity;

                    foreach( float value in values ) {
                        selectorMin = Math.Min( selectorMin, value );
                        selectorMax = Math.Max( selectorMax, value );
                    }
                }

            }
        }

        GUILayout.Label( "Write to Vertex Group:");
        writeTo = GUILayout.TextField( writeTo );
        if ( writeTo == "" ) {
            GUILayout.Label("Set a name to save result to a vertex group.");
        }
    }
}
