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
    
    bool marked = false;
    Marking primaryMarking;
    public Vector3 Marking {
        get {
            return primaryMarking.pos;
        }
    }

    int tab = 0;
    int tabInner = 0;
    float dist = 0.0f;
    float playerSpeed = 5.0f;

    Meterstab meterstab;

    int selectedArea;
    public Area SelectedArea {
        get {
            if ( selectedArea >= SmartSceneWindow.db.areas.Count )
            return null;
            return SmartSceneWindow.db.areas[selectedArea];
        }
    }

    String selectedGroup = "";
    bool isCreating = false;
    bool groupMarking;

    public SmartSceneTools ( ) {
        meterstab = new Meterstab();
        selectedArea = 0;

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
                tabInner = GUILayout.Toolbar ( tabInner, new String[] {"Primary Marking", "Marking Groups"} );
                if ( tabInner == 0 )
                {
                    if ( primaryMarking != null ) {
                        GUILayout.Label ( "Marked Position: " + primaryMarking.pos );
                    }
                    groupMarking = false;
                }
                if ( tabInner != 0 ) {
                    selectedGroup = SmartSceneWindow.db.OffGridGroupGUI ( selectedGroup, ref isCreating );
                    groupMarking = true;
                }
                break;
            case 1:
                GUILayout.Label("Player Speed (in Units per Second):");
                string speed = GUILayout.TextField( "" + playerSpeed );
                float parsedSpeed;
                if (float.TryParse(speed, out parsedSpeed) ) {
                playerSpeed = parsedSpeed;
            }
                GUILayout.Label("Distance: " + dist);
                GUILayout.Label("Time: " + (dist / playerSpeed ) + " seconds");
                break;
            case 2:
                break;
            case 3:
                selectedArea = SmartSceneWindow.db.AreaSelectGUI( selectedArea, true );
                SelectedArea?.OnGUI( false );
                int add = SmartSceneWindow.db.AreaAddGUI();
                if ( add != -1 ) selectedArea = add;
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
                if ( groupMarking && e.type != EventType.MouseDrag){
                    if ( SmartSceneWindow.db.offGridVertexGroups.ContainsKey( selectedGroup ) )
                        SmartSceneWindow.db.AddPosition ( selectedGroup, current.pos );
                } else { 
                    primaryMarking = current;
                }

                e.Use(); 
            }
            if ((e.type == EventType.MouseDown) &&  e.button == 0)
            {   
                if ( meterstabMode ) {
                    meterstab.Set( current );
                }
                if ( areaMode ) {
                    SelectedArea?.AddVertex( current );
                }
            }
            if ( (e.type == EventType.MouseDown) &&  e.button == 1) 
            {   
                if( meterstabMode ) {
                    meterstab.Clear();
                }
            }
        }

        if ( markingMode && marked  ) {
            flagMaterial.SetPass(0);
            if ( primaryMarking != null && !groupMarking)
                Graphics.DrawMeshNow( flagMesh, primaryMarking.pos, primaryMarking.rot );

            if ( groupMarking && SmartSceneWindow.db.offGridVertexGroups.ContainsKey( selectedGroup ) ) {
                foreach ( Vector3 pos in SmartSceneWindow.db.offGridVertexGroups[selectedGroup] ) {
                    Graphics.DrawMeshNow ( flagMesh, pos, Quaternion.identity );
                }
            }
        } 

        dist = 0.0f;
        if ( meterstabMode ) {
            dist = meterstab.Draw( markingMesh, markingMaterial, current );
        }
        if ( areaMode ) {
            foreach ( Area area in SmartSceneWindow.db.areas )
                area.Draw ( markingMesh, markingMaterial );
        }

        SceneView.lastActiveSceneView.Repaint();

        Handles.BeginGUI();
        if( meterstabMode ) {
            GUILayout.Label("" + dist);
        }
        Handles.EndGUI();
    }

    public Vector3 SupplyPosition( String msg, Vector3 curr ) {
        if ( marked && primaryMarking != null ) {
            flagMaterial.SetPass(0);
            Graphics.DrawMeshNow( flagMesh, primaryMarking.pos, primaryMarking.rot );

            if ( GUILayout.Button( msg ) ) {
                return primaryMarking.pos;
            }
        }
        return curr;
    } 
}
