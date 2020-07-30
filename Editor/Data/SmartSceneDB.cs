using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class SmartSceneDB
{
    [SerializeField] public List<Area> areas;
    [SerializeField] public List<String> areaNames;
    // Attributes
    // Per Vertex Attributes
    [SerializeField] public SerializableDictionary< String, float[] > floatVertexAttributes;
    [SerializeField] public SerializableDictionary< String, String[] > stringVertexAttributes;

    // Vertex Groups
    [SerializeField] public SerializableDictionary< String, int[] > gridVertexGroups;
    [SerializeField] public SerializableDictionary< String, Vector3[] > offGridVertexGroups;

    // Level Attributes
    [SerializeField] public SerializableDictionary< String, float > floatLevelAttributes;
    [SerializeField] public SerializableDictionary< String, String > stringGridVertexGroups;

    static GridMesh gridMesh;

    public SmartSceneDB () {
        areas = new List<Area>();
        areaNames = new List<String>();

        floatVertexAttributes = new SerializableDictionary<String, float[]>();
        stringVertexAttributes = new SerializableDictionary<String, String[]>();

        gridVertexGroups = new SerializableDictionary<String, int[]>();
        offGridVertexGroups = new SerializableDictionary<String, Vector3[]>();

        floatLevelAttributes = new SerializableDictionary<String, float>();
        stringGridVertexGroups = new SerializableDictionary<String, String>();
    }


    public void SetGridMesh( GridMesh mesh ) {
        gridMesh = mesh;
    }

    public bool AddPerVertexFloatAttribute ( String key, float[] values ) {
        if ( gridMesh != null && values.Length == gridMesh.Size ) {
            floatVertexAttributes[key] = values;
            return true;
        }
        return false;
    }

    public bool AddPerVertexStringAttribute ( String key, String[] values ) {
        if ( gridMesh != null && values.Length == gridMesh.Size ) {
            stringVertexAttributes[key] = values;
            return true;
        }
        return false;
    }

    public bool AddOnGridVertexGroup ( String key, int[] vertices ) {
        gridVertexGroups[key] = vertices;
        return true;
    }

    public bool AddOffGridVertexGroup ( String key, Vector3[] vertices ) {
        offGridVertexGroups[key] = vertices;
        return true;
    }

    public int AreaSelectGUI( int selected, bool allowName = false, bool allowAll = false) {
        if ( areas.Count > 0 || allowAll ) {
            List<String> names = new List<String>(areaNames);
            if(allowAll) names.Add("WHOLE MESH");
            int i = EditorGUILayout.Popup ( selected, names.ToArray() );

            if( i != Area.WHOLE_MESH ) 
            {
                Area selectedArea = areas[i];
                if ( allowName ) {
                    String temp = GUILayout.TextField( selectedArea.name );
                    if ( temp != selectedArea.name ) {
                        int count = 0;
                        foreach( String area in areaNames ) {
                            if ( area == temp ) {
                                count++;
                            }
                        }
                        if ( count == 0 ) {
                            selectedArea.name = temp;
                            areaNames[i] = temp;
                        }
                    }
                }
            }
            return i;
        }
        return 0;
    }

    public int AreaAddGUI() {
        GUILayout.Space(10);
        if( GUILayout.Button("Add Area")) {
            AddArea();
            return areas.Count - 1;
        }
        return -1;
    }

    public void AddArea() {
        areas.Add( new Area ( areas.Count ) );
        areaNames.Add( areas[areas.Count - 1].name );
    }

    public void AddArea(String name) {
        areas.Add( new Area ( ) );
        areas[areas.Count - 1].name = name;
        areaNames.Add( areas[areas.Count - 1].name );
    }

    public void ResetAreaVertexGroups() {
        foreach ( Area area in areas ) {
            area.ResetVertexGroup();
        }
    }
}
