using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class SmartSceneTools
{
    Mesh selectorMesh;
    Material selectorMaterial;

    Mesh markingMesh;
    Material markingMaterial;

    Mesh flagMesh;
    Material flagMaterial;

    bool toolMode = true;
    
    bool markingMode = false;
    bool marked = false;
    Marking primaryMarking;
    public Vector3 Marking {
        get {
            return primaryMarking.pos;
        }
    }

    int tab = 0;
    float dist = 0.0f;

    bool meterstabMode = false;
    Meterstab meterstab;
    Area testArea;

    public SmartSceneTools () {
        meterstab = new Meterstab();
        testArea = new Area();

        selectorMesh = (Mesh) AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tuerk.smartscene/Assets/Models/selector.fbx");
        selectorMaterial = (Material) AssetDatabase.LoadAssetAtPath<Material>("Packages/com.tuerk.smartscene/Assets/Models/selector.fbx");

        markingMesh = (Mesh) AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tuerk.smartscene/Assets/Models/marking.fbx");
        markingMaterial = (Material) AssetDatabase.LoadAssetAtPath<Material>("Packages/com.tuerk.smartscene/Assets/Models/marking.fbx");

        flagMesh = (Mesh) AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tuerk.smartscene/Assets/Models/flag.fbx");
        flagMaterial = (Material) AssetDatabase.LoadAssetAtPath<Material>("Packages/com.tuerk.smartscene/Assets/Models/flag.fbx");
    }
    
    public void OnGUI () {

        tab = GUILayout.Toolbar (tab, new String[] {"Mark", "Path Length", "Meterstab", "Mark Area"});
        switch (tab)
        {
            case 0:
                if ( primaryMarking != null ) {
                    GUILayout.Label ( "Marked Position: " + primaryMarking.pos );
                }
                break;
            case 1:
                GUILayout.Label("Distance: " + dist);
                break;
            case 2:
                break;
            case 3:
                testArea.OnGUI();
                break;
            default:
                break;
        }
    }

    public void OnSceneGUI( SceneView sceneView ) {
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID (FocusType.Passive);

        Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
        RaycastHit hit;

        Marking current = null;

        bool markingMode = ( tab == 0 );
        bool meterstabMode = ( tab == 1 );
        bool areaMode = ( tab == 3 );

        if( Physics.Raycast(ray, out hit) ) {
            HandleUtility.AddDefaultControl(controlID);

            selectorMaterial.SetPass(0);

            current = new Marking ( hit.point, Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90,0,0));

            Graphics.DrawMeshNow( selectorMesh, current.pos + 0.25f * hit.normal, current.rot );

            if (markingMode && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) &&  e.button == 0)
            {
                marked = true;
                primaryMarking = current;
                e.Use(); 
            }
            if ((e.type == EventType.MouseDown) &&  e.button == 0)
            {   
                if ( meterstabMode ) {
                    meterstab.Set( current );
                }
                if ( areaMode ) {
                    testArea.AddVertex( current );
                }
            }
            if ( (e.type == EventType.MouseDown) &&  e.button == 1) 
            {   
                if( meterstabMode ) {
                    meterstab.Clear();
                }
            }
        }

        if ( markingMode && marked && primaryMarking != null ) {
            flagMaterial.SetPass(0);
            Graphics.DrawMeshNow( flagMesh, primaryMarking.pos, primaryMarking.rot );
        }

        dist = 0.0f;
        if ( meterstabMode ) {
            dist = meterstab.Draw( markingMesh, markingMaterial, current );
        }
        if ( areaMode ) {
            testArea.Draw ( markingMesh, markingMaterial );
        }

        SceneView.lastActiveSceneView.Repaint();

        Handles.BeginGUI();
        if( meterstabMode ) GUILayout.Label("" + dist);
        Handles.EndGUI();
    }

    public void SupplyPosition( String msg, out Vector3 pos ) {
        if ( marked && primaryMarking != null ) {
            flagMaterial.SetPass(0);
            Graphics.DrawMeshNow( flagMesh, primaryMarking.pos, primaryMarking.rot );

            if ( GUILayout.Button( msg ) ) pos = primaryMarking.pos;
        }
    } 
}
