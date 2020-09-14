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
    public List<Marking> markings;

    public Meterstab () {
        pegMesh = (Mesh) AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tuerk.smartscene/Assets/Models/peg.fbx");
        pegMaterial = (Material) AssetDatabase.LoadAssetAtPath<Material>("Packages/com.tuerk.smartscene/Assets/Models/peg.fbx");
        markings = new List<Marking>();
    }

    public void Clear() {
        markings.Clear();
    }

    public void Set ( Marking current ) {
        markings.Add(current);
    }

    public float Draw( Mesh markingMesh, Material markingMaterial, Marking current) {
        float dist = 0.0f;
        if ( markings.Count > 0 ) {
            markingMaterial.SetPass(0);
            Graphics.DrawMeshNow( markingMesh, markings[0].pos, markings[0].rot );

            for(int i = 0; i < markings.Count; i++ ) {
                dist += i == markings.Count - 1 ?
                    DrawSegment(markings[i], current, markingMesh, markingMaterial) :
                    DrawSegment(markings[i], markings[i+1], markingMesh, markingMaterial);
            }
        }
        return dist;
    }

    public float DrawSegment( Marking from, Marking to, Mesh markingMesh, Material markingMaterial ) {
        float dist = 0.0f;
        NavMeshPath path = new NavMeshPath();
        if ( NavMesh.CalculatePath ( from.pos, to.pos, NavMesh.AllAreas, path ) ) {
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
        Graphics.DrawMeshNow( markingMesh, to.pos, to.rot );
        return dist;
    }
}
