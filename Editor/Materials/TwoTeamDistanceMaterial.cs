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
    String redTeam;
    [SerializeField]
    String blueTeam;
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
    [SerializeField]
    bool overlap = true;

    public TwoTeamDistanceMaterial ( String name ) : base ( name, "SmartScene/TwoTeamDist") {
    }

    public override void Bake( GridMesh mesh ) {
        this.mesh = mesh;

        maxDist = float.NegativeInfinity;
        float[] redDist = new float[mesh.Size];
        float[] blueDist = new float[mesh.Size];

        bool[] redExists = new bool[mesh.Size];
        bool[] blueExists = new bool[mesh.Size];

        Vector3[] red = SmartSceneWindow.db.GetOffGridVertexGroup(redTeam);
        Vector3[] blue = SmartSceneWindow.db.GetOffGridVertexGroup(blueTeam);

        NavMeshPath path = new NavMeshPath();

        for ( int i = 0; i < mesh.Size; i++ ) {
            redDist[i] = float.PositiveInfinity;
            blueDist[i] = float.PositiveInfinity;
            redExists[i] = false;
            blueExists[i] = false;

            foreach ( Vector3 vec in red ) {
                if ( NavMesh.CalculatePath ( vec, mesh[i], NavMesh.AllAreas, path) ) {
                    redDist[i] = Mathf.Min ( redDist[i], GetPathLength ( path ) );
                    redExists[i] = true;
                }
            }

            foreach ( Vector3 vec in blue ) {
                if ( NavMesh.CalculatePath ( vec, mesh[i], NavMesh.AllAreas, path) ) {
                    blueDist[i] = Mathf.Min ( blueDist[i], GetPathLength ( path ) );
                    blueExists[i] = true;
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
                redDist[i] != float.PositiveInfinity ? redDist[i] / maxDist : 0.0f,                           //r
                redExists[i] ? 1.0f : 0.0f,                     //g
                blueDist[i] != float.PositiveInfinity ? blueDist[i] / maxDist : 0.0f,                          //b
                blueExists[i] ? 1.0f : 0.0f                     //a
            );
        }

        if ( redDistanceAttribute != null & redDistanceAttribute != "" )
            SmartSceneWindow.db.AddPerVertexFloatAttribute( redDistanceAttribute, redDist );
        if ( blueDistanceAttribute != null & blueDistanceAttribute != "")
            SmartSceneWindow.db.AddPerVertexFloatAttribute( blueDistanceAttribute, blueDist );

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
            material.SetInt("_Overlap", overlap ? 1 : 0 );
        
        }    
    }

    //https://forum.unity.com/threads/getting-the-distance-in-nav-mesh.315846/
    public static float GetPathLength( NavMeshPath path )
    {
        float lng = 0.0f;
       
        if (( path.status == NavMeshPathStatus.PathComplete ) && ( path.corners.Length > 1 ))
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

        GUILayout.Label("Red Spawns:");
        redTeam = SmartSceneWindow.db.OffGridVertexGroupSelectGUI(redTeam);
        GUILayout.Label("Blue Spawns:");
        blueTeam = SmartSceneWindow.db.OffGridVertexGroupSelectGUI(blueTeam);

        if ( isBaked ) {
            GUILayout.Space(20);
            GUILayout.Label("Distance: [0 .. "+maxDist+"]:");
            showDist = GUILayout.HorizontalSlider(showDist, 0.001f, maxDist );
            GUILayout.Space(10);
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
            overlap = GUILayout.Toggle( overlap, "Overlap");
            SceneView.RepaintAll();
            GUILayout.Space(20);
        }

        GUILayout.Label("Blue Distance Vertex Attribute: ");
        blueDistanceAttribute = GUILayout.TextField(blueDistanceAttribute);

        GUILayout.Label("Red Distance Vertex Attribute: ");
        redDistanceAttribute = GUILayout.TextField(redDistanceAttribute);
    }

    String blueDistanceAttribute = "blueTeamDistance";
    String redDistanceAttribute = "redTeamDistance";
    String blueSpawns = "blueSpawnPositions";
    String redSpawns = "redSpawnPositions";
    String maxDistanceAttribute = "maxDistance";

    public override String[] ProvidesVertexAttributes() {
        return new String[] {
            blueDistanceAttribute,
            redDistanceAttribute
        };
    }
    public override String[] WritesLevelAttributes() {
        return new String[] {
            maxDistanceAttribute,
            blueSpawns,
            redSpawns
        };
    }
}
