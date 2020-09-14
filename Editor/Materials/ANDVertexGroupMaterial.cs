using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class LogicOpsVertexGroupMaterial : VertexGroupMaterial
{
    int indexA = 0;
    int indexB = 0;
    bool useAreaA = true;
    bool useAreaB = true;
    int mode = 0;
    String writeTo = "";

    public LogicOpsVertexGroupMaterial (String name) : base(name) {}

    public override void Bake( GridMesh mesh ) {
        colors = new Color[mesh.Size];
      
        int[] groupA = useAreaA ? SmartSceneWindow.db.areas[indexA].GetVertexGroup( mesh ) : SmartSceneWindow.db.gridVertexGroups.ElementAt(indexA).Value;
        int[] groupB = useAreaB ? SmartSceneWindow.db.areas[indexB].GetVertexGroup( mesh ) : SmartSceneWindow.db.gridVertexGroups.ElementAt(indexB).Value;

        List<int> result = new List<int>();

        for ( int i = 0; i < colors.Length; i++ ) {
            if ( 
                ( mode == 0 && (Includes( groupA, i ) && Includes( groupB, i) ) ) || 
                ( mode == 1 && (Includes( groupA, i ) || Includes( groupB, i) ) ) ||
                ( mode == 2 && (Includes( groupA, i ) ^ Includes( groupB, i) ) ) ) {
                result.Add(i);
                colors[i] = new Color(1,1,1,1);
            }
            else {
                colors[i] = new Color(0,0,0,0);
            }
        }

        mesh.Mesh.colors = colors;
        this.mesh = mesh;

        if ( writeTo != null && writeTo != "" ) {
            SmartSceneWindow.db.AddOnGridVertexGroup( writeTo, result.ToArray());
        }
        
        isBaked = true;
    }

    public override void DrawGUI() {
        GUILayout.Label( "Group A");
        indexA = SmartSceneWindow.db.VertexGroupSelectGUI( indexA, ref useAreaA );
        GUILayout.Space(10);
        mode = GUILayout.Toolbar (mode, new String[] {"AND", "OR", "XOR"});
        GUILayout.Space(10);
        GUILayout.Label( "Group B");
        indexB = SmartSceneWindow.db.VertexGroupSelectGUI( indexB, ref useAreaB );
        GUILayout.Space(10);
        GUILayout.Label( "Write to Vertex Group:");
        writeTo = GUILayout.TextField( writeTo );
        if ( writeTo == "" ) {
            GUILayout.Label("Set a name to save result to a vertex group.");
        }
    }
}
