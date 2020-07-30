using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[Serializable]
public class TwoTeamDistanceMaterial : ColorMaterial
{
    [SerializeField]
    List<Vector3> redTeam;
    [SerializeField]
    List<Vector3> blueTeam;
    [SerializeField]
    float maxDist;
    [SerializeField]
    float showDist = 0.0f;
    [SerializeField]
    bool showRed = true;
    [SerializeField]
    bool showBlue = true;
    [SerializeField]
    bool showMeetingpoint = false;

    public TwoTeamDistanceMaterial ( String name ) : base ( name, "SmartScene/TwoTeamDist") {
        this.redTeam = new List<Vector3>();
        this.blueTeam = new List<Vector3>();
    }

    public override void Bake( GridMesh mesh ) {
        this.mesh = mesh;

        maxDist = float.NegativeInfinity;
        float[] redDist = new float[mesh.Size];
        float[] blueDist = new float[mesh.Size];

        NavMeshPath path = new NavMeshPath();

        for ( int i = 0; i < mesh.Size; i++ ) {
            redDist[i] = float.PositiveInfinity;
            blueDist[i] = float.PositiveInfinity;

            foreach ( Vector3 vec in redTeam ) {
                if ( NavMesh.CalculatePath ( vec, mesh[i], NavMesh.AllAreas, path) ) {
                    redDist[i] = Mathf.Min ( redDist[i], GetPathLength ( path ) );
                }
            }

            foreach ( Vector3 vec in blueTeam ) {
                if ( NavMesh.CalculatePath ( vec, mesh[i], NavMesh.AllAreas, path) ) {
                    blueDist[i] = Mathf.Min ( blueDist[i], GetPathLength ( path ) );
                }
            }

            if ( redDist[i] != float.PositiveInfinity ) {
                maxDist = Mathf.Max ( maxDist, redDist[i]);
            }
            if ( blueDist[i] != float.PositiveInfinity ) {
                maxDist = Mathf.Max ( maxDist, blueDist[i]);
            }
        }

        colors = new Color[mesh.Size];
        for ( int i = 0; i < mesh.Size; i++ ) {
            colors[i] = new Color (
                redDist[i] / maxDist,                           //r
                0.0f,                                           //g
                blueDist[i] / maxDist,                          //b
                (redDist[i] + blueDist[i] > 0.0f) ? 1.0f : 0.0f //a
            );
        }

        // mesh.AddPerVertexFloatAttribute( redDistanceAttribute, redDist );
        // mesh.AddPerVertexFloatAttribute( blueDistanceAttribute, blueDist );
        // mesh.AddOffGridVertexGroup( blueSpawns, blueTeam.ToArray() );
        // mesh.AddOffGridVertexGroup( redSpawns, redTeam.ToArray() );

        isBaked = true;
        if ( showDist > maxDist || showDist == 0.0f ) 
            showDist = 0.001f;
    }

    public override void PreDraw( GridMesh mesh ) {

        if( isBaked ) {
            mesh.Mesh.colors = colors;

            material.SetFloat("_maxDistance", maxDist);
            material.SetFloat("_showDistance", showDist);
            material.SetInt("_showRed", showRed ? 1 : 0);
            material.SetInt("_showBlue", showBlue ? 1 : 0);
            material.SetInt("_showMeeting", showMeetingpoint ? 1 : 0);
        }
    }

    //https://forum.unity.com/threads/getting-the-distance-in-nav-mesh.315846/
    public static float GetPathLength( NavMeshPath path )
    {
        float lng = 0.0f;
       
        if (( path.status != NavMeshPathStatus.PathInvalid ) && ( path.corners.Length > 1 ))
        {
            for ( int i = 1; i < path.corners.Length; ++i )
            {
                lng += Vector3.Distance( path.corners[i-1], path.corners[i] );
            }
        }
       
        return lng;
    }

    public override void DrawGUI() {
        Transform selected = Selection.activeTransform;
        if ( selected != null ) {
            GUILayout.Label("Selected Object: \n" + selected.name );
        }
        else {
            GUILayout.Label("Select an object to edit attributes");
        }
        GUILayout.Space(20);

        GUILayout.Label("Add Transform position as:");
        if ( GUILayout.Button("red spawn") && selected != null ) {
            redTeam.Add(selected.position);
        }
        if ( GUILayout.Button("blue spawn")  && selected != null ) {
            blueTeam.Add(selected.position);
        }
        GUILayout.Space(10);

        GUILayout.Label("Red Spawns:");
        foreach ( Vector3 pos in redTeam ) {
            GUILayout.Label ( "\t" + pos );
        }
        GUILayout.Label("Blue Spawns:");
        foreach ( Vector3 pos in blueTeam ) {
            GUILayout.Label ( "\t" + pos );
        }

        if ( isBaked ) {
            GUILayout.Space(20);
            GUILayout.Label("Distance: [0 .. "+maxDist+"]:");
            showDist = GUILayout.HorizontalSlider(showDist, 0.001f, maxDist );
            string str = GUILayout.TextField( ""+showDist);
            float temp;
            if (float.TryParse(str, out temp)) {
                if ( temp > 0.001f && temp < maxDist ) {
                    showDist = temp;
                }
            }
            showRed = GUILayout.Toggle( showRed, "Show red");
            showBlue = GUILayout.Toggle( showBlue, "Show blue");
            showMeetingpoint = GUILayout.Toggle( showMeetingpoint, "Show Meetingpoint");
            SceneView.RepaintAll();
            GUILayout.Space(20);
        }

        if ( redTeam.Count + blueTeam.Count > 0 ) {
            if ( GUILayout.Button("Clear Spawns") ) {
                redTeam.Clear();
                blueTeam.Clear();
            }
        }
    }

    String blueDistanceAttribute = "blueTeamDistanceFromSpawn";
    String redDistanceAttribute = "redTeamDistanceFromSpawn";
    String blueSpawns = "blueSpawnPositions";
    String redSpawns = "redSpawnPositions";
    String maxDistanceAttribute = "maxDistance";

    public String[] WritesVertexAttributes() {
        return new String[] {
            blueDistanceAttribute,
            redDistanceAttribute
        };
    }
    public String[] WritesLevelAttributes() {
        return new String[] {
            maxDistanceAttribute,
            blueSpawns,
            redSpawns
        };
    }
}
