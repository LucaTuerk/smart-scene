using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class Meterstab {
    Mesh pegMesh;
    Material pegMaterial;

    public bool firstMarked = false, secondMarked = false;
    public bool savedMarkedMode;
    public Marking first, second;

    public Meterstab () {
        pegMesh = (Mesh) AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tuerk.smartscene/Assets/Models/peg.fbx");
        pegMaterial = (Material) AssetDatabase.LoadAssetAtPath<Material>("Packages/com.tuerk.smartscene/Assets/Models/peg.fbx");
    }

    public void Clear() {
        firstMarked = false;
        secondMarked = false;
    }

    public void Set ( Marking current ) {
        if ( firstMarked ) {
            second = current;
            secondMarked = true;
        } else {
            first = current;
            firstMarked = true;
        }
    }

    public float Draw( Mesh markingMesh, Material markingMaterial, Marking current) {
        float dist = 0.0f;
        if ( firstMarked ) {
            markingMaterial.SetPass(0);
            Graphics.DrawMeshNow( markingMesh, first.pos, first.rot );

            Marking target = secondMarked ? second : current;
            if ( target != null ) {
                NavMeshPath path = new NavMeshPath();
                if ( NavMesh.CalculatePath ( first.pos, target.pos, NavMesh.AllAreas, path ) ) {
                    for ( int i = 1; i < path.corners.Length; i++ ) {
                        Handles.DrawLine( path.corners[i-1] + 0.65f * Vector3.up, path.corners[i] + 0.65f * Vector3.up);
                        float currDist = Vector3.Distance ( path.corners[i-1], path.corners[i]);
                        dist += currDist;
                        if ( currDist > 0.5f && i != path.corners.Length - 1 ) {
                            pegMaterial.SetPass(0);
                            Graphics.DrawMeshNow( pegMesh, path.corners[i], Quaternion.identity);
                        }
                    }
                }
                markingMaterial.SetPass(0);
                Graphics.DrawMeshNow( markingMesh, target.pos, target.rot );
            }
        }
        return dist;
    }
}
