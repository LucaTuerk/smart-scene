using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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

    public bool AddFloatLevelAttribute ( String key, float value ) {
        floatLevelAttributes[key] = value;
        return true;
    }

    public int FloatAttributeSelectGUI( int index ) {
        int attr = index;
        List<string> choiceList = new List<string>();
        IEnumerator<string> enumerator = floatVertexAttributes.Keys.GetEnumerator();
        while (enumerator.MoveNext()) {
            choiceList.Add( enumerator.Current );
        }
    
        string[] choices = choiceList.ToArray();
        attr = EditorGUILayout.Popup( attr, choices );
        return attr;
    }

    public int VertexGroupSelectGUI( int index, ref bool useAreas ) {
        int tab = useAreas ? 0 : 1;

        string[] choices;
        int newTab = GUILayout.Toolbar (tab, new String[] {"Areas", "Other Vertexgroups"});
        int group = index;
        if ( newTab != tab)
            group = 0;

        switch ( newTab ) {
            case 0:
                group = AreaSelectGUI(group, false, true);
                break;
            case 1:
                List<string> choiceList = new List<string>();
                IEnumerator<string> enumerator = gridVertexGroups.Keys.GetEnumerator();
                while (enumerator.MoveNext()) {
                    choiceList.Add( enumerator.Current );
                }
            
                choices = choiceList.ToArray();
                group = EditorGUILayout.Popup( group, choices );
                break;
            default:
                break;
        }

        useAreas = newTab == 0;
        return group;
    }

    public int[] GetVertexGroup ( int index, ref bool useAreas, GridMesh mesh ) {
        if (useAreas) {
            if ( index < areas.Count && index >= 0 ) {
                return areas[index].GetVertexGroup( mesh );
            } else {
                return mesh.WHOLE_MESH;
            }
        }
        else {
            if ( index < gridVertexGroups.Count ) {
                return gridVertexGroups.ElementAt(index).Value;
            }
        }
        return null;
    }

    public String GetVertexGroupName ( int index, bool useAreas ) {
        if ( useAreas ) {
            if ( index < areas.Count && index >= 0 ) {
                return areas[index].name;
            }
            else {
                return "ALL";
            }
        }
        else {
            if ( index < gridVertexGroups.Count && index >= 0 ) {
                return gridVertexGroups.ElementAt(index).Key;
            }
            else {
                return "";
            }
        }
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

    public String OffGridGroupGUI ( String key, ref bool isCreating) {
        GUILayout.Space(20);
        if ( isCreating ) {
            GUILayout.Label("Enter name and press ok.");
            String name = GUILayout.TextField( key );
            if ( GUILayout.Button("Ok") && !offGridVertexGroups.ContainsKey(name)) {
                offGridVertexGroups[name] = new Vector3[0];
                isCreating = false;
                return name;
            }
            return name;
        }
        List<string> choiceList =
            offGridVertexGroups.Keys.ToList();
        if ( choiceList.Count > 0 ) {
            int selected = choiceList.IndexOf( key );
            selected = EditorGUILayout.Popup ( "Marking Group: ", selected, choiceList.ToArray());
            key = choiceList.ElementAt(selected);
        }

        if ( !isCreating ) {
            if ( offGridVertexGroups.ContainsKey( key ) ) {
                GUILayout.Label("Add positions by clicking in the scene.");
                int i = 0;
                foreach ( Vector3 pos in offGridVertexGroups[key] ) {
                    if ( i++ < 10 ) GUILayout.Label("\t"+pos);
                    else { GUILayout.Label("\t +" + (offGridVertexGroups[key].Length - 10)); break; }
                }
                
                if (GUILayout.Button("Clear positions")) {
                    offGridVertexGroups[key] = new Vector3[0];
                }
            }
            GUILayout.Space(20);
            if ( GUILayout.Button("Add new Group")) {
                isCreating = true;
                return "";
            }
        }
        return key;
    }

    public String OffGridVertexGroupSelectGUI ( String key ) {
        List<string> choiceList =
            offGridVertexGroups.Keys.ToList();
        if ( choiceList.Count > 0 ) {
            if ( !choiceList.Contains(key) )
                key = choiceList[0];
            int selected = choiceList.IndexOf( key );
            selected = EditorGUILayout.Popup ( "", selected, choiceList.ToArray());
            key = choiceList.ElementAt(selected);
        }
        else {
            GUILayout.Label("Add Vertex Groups in the Tools tab.");
        }
        return key;
    }

    public Vector3[] GetOffGridVertexGroup( String key ) {
        if ( offGridVertexGroups.ContainsKey(key)) {
            return offGridVertexGroups[key];
        }
        return new Vector3[]{};
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

    public void AddPosition ( String key, Vector3 pos ) {
        List<Vector3> list = 
            offGridVertexGroups.ContainsKey( key ) ? 
            new List<Vector3>( offGridVertexGroups[key] ) : 
            new List<Vector3>();
        list.Add(pos);
        offGridVertexGroups[key] = list.ToArray();
    }

    public void Reload() {
        if( areas == null ) areas = new List<Area>();
        if( areaNames == null ) areaNames = new List<String>();

        if( floatVertexAttributes == null ) floatVertexAttributes = new SerializableDictionary<String, float[]>();
        if( stringVertexAttributes == null ) stringVertexAttributes = new SerializableDictionary<String, String[]>();

        if( gridVertexGroups == null ) gridVertexGroups = new SerializableDictionary<String, int[]>();
        if( gridVertexGroups == null ) offGridVertexGroups = new SerializableDictionary<String, Vector3[]>();

        if( floatLevelAttributes == null ) floatLevelAttributes = new SerializableDictionary<String, float>();
        if( stringGridVertexGroups == null) stringGridVertexGroups = new SerializableDictionary<String, String>();
    
        foreach ( Area area in areas) {
            area.ReloadShader();
        }
    }
}
