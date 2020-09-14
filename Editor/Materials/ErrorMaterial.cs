using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[Serializable]
public class ErrorMaterial : VertexGroupMaterial
{
    int baseline = 0;
    int erroniousSet = 0;

    double maxError;
    double meanError;
    double meanSquaredError;
    double standardDeviation;
    double variance;

    public ErrorMaterial (String name) : base(name) {}

    public override void Bake( GridMesh mesh ) {
        float[] baseVal = 
                SmartSceneWindow.db.floatVertexAttributes[SmartSceneWindow.db.floatVertexAttributes.ElementAt(baseline).Key];

        float[] errorVal =
                SmartSceneWindow.db.floatVertexAttributes[SmartSceneWindow.db.floatVertexAttributes.ElementAt(erroniousSet).Key];

        maxError = 0.0;
        meanError = 0.0; 
        meanSquaredError = 0.0;
        standardDeviation = 0.0;
        variance = 0.0;

        for ( int i = 0; i < baseVal.Length; i++ ) {
            double curr = 
                errorVal[i] - baseVal[i];

            maxError = Math.Abs( curr ) > Math.Abs(maxError) ? curr : maxError;
            meanSquaredError += Math.Pow( ( curr ), 2);
            meanError += curr;
        }

        meanError /= baseVal.Length;
        meanSquaredError /= baseVal.Length;


        for ( int i = 0; i < baseVal.Length; i++ ) {
            double curr = 
                errorVal[i] - baseVal[i];

            standardDeviation += curr - meanError;
            variance += Math.Pow(curr - meanError, 2);
        }

        standardDeviation /= baseVal.Length - 1;
        variance /= baseVal.Length - 1;
    }

    public override void DrawGUI() {

        GUILayout.Label( "Baseline: "); baseline = SmartSceneWindow.db.FloatAttributeSelectGUI(baseline);
        GUILayout.Label( "Other: "); erroniousSet = SmartSceneWindow.db.FloatAttributeSelectGUI(erroniousSet);

        GUILayout.Label("Max Error: " + maxError);
         GUILayout.Label("Mean Error: " + meanError );
        GUILayout.Label("Mean Squared Error: " + meanSquaredError);
        GUILayout.Label("Standard Deviation " + standardDeviation);
        GUILayout.Label("Variance: " + variance);
    }
}
