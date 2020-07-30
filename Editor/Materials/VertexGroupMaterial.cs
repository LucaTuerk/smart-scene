using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VertexGroupMaterial : SimpleColorMaterial
{
    int index = 0;

    public VertexGroupMaterial (String name) : base(name) {}

    public override void Bake( GridMesh mesh ) {
        colors = new Color[mesh.Size];
      
        int[] group = SmartSceneWindow.db.areas[index].GetVertexGroup( mesh );

        Debug.Log(group.Length);

        for ( int i = 0; i < colors.Length; i++ ) {
            if (Includes( group, i ) ) {
                colors[i] = new Color(1,1,1,1);
            }
            else {
                colors[i] = new Color(0,0,0,0);
            }
        }

        mesh.Mesh.colors = colors;
        this.mesh = mesh;
        
        isBaked = true;
    }


    public static bool Includes( int[] arr, int val ) {
        // binary search for val
        int low = 0, high = arr.Length - 1;
        while ( low <= high ) {
            int mid = (low + high) / 2;
            if ( arr[mid] == val ) {
                return true;
            }
            else if ( arr[mid] < val ) {
                low = mid + 1;
            }
            else if ( arr[mid] > val ) {
                high = mid - 1;
            } 
        } 
        return false;
    }

    public bool saveIncludes( int[] arr, int val ) {
        foreach ( int i in arr ) {
            if ( i == val )
                return true;
        }
        return false;
    }

    public override void DrawGUI() {
        index = SmartSceneWindow.db.AreaSelectGUI(index);
    }
}
