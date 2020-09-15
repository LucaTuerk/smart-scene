using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


/*

*/
public class SmartSceneWindow : EditorWindow
{
    // Selected object in scene
    Transform selected;

    // Selected Editor tab
    int tab = 0;
    bool debug;
    bool toolMode = false;


    // GridMesh Settings
    GridMesh mesh;
    int veticalLayerLimit;
    bool optimizeMesh = true;
    float verticalOffset = 0.1f;

    public static SmartSceneTools tools;
    public static SmartSceneDB db;

    [MenuItem("Tools/SmartScene/Settings")]
    static void Init()
    {
        SmartSceneWindow window = (SmartSceneWindow)EditorWindow.GetWindow(typeof(SmartSceneWindow));
        window.name = "Smart Scene Settings";

        Texture icon = (Texture) AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.tuerk.smartscene/Assets/Images/icon.png");

        window.titleContent.text = " Smart Scene";
        window.titleContent.image = icon;

        window.Show();
    }

    void OnEnable() {
        LoadGridMesh();
        SceneView.duringSceneGui += this.OnSceneGUI;
        Camera.onPostRender += this.OnPostRender;
        EditorSceneManager.sceneOpened += this.OnSceneOpened;
    }

    void LoadGridMesh() {
        string scene = EditorSceneManager.GetActiveScene().name;
        Debug.Log(scene);

        if ( mesh == null )
            mesh = AssetDatabase.LoadAssetAtPath( "Assets/Editor/SmartScene/" + scene + "/GridMesh.asset", typeof( GridMesh ) ) as GridMesh;
        // Create Asset Files
        if ( mesh == null ) {
            Debug.Log("SmartScene: Creating GridMesh asset" );

            if ( !AssetDatabase.IsValidFolder( "Assets/Editor" ) ) {
                AssetDatabase.CreateFolder("Assets", "Editor");
            }
            if ( !AssetDatabase.IsValidFolder( "Assets/Editor/SmartScene" ) ) {
                AssetDatabase.CreateFolder("Assets/Editor", "SmartScene"); 
            }
            if( !AssetDatabase.IsValidFolder( "Assets/Editor/SmartScene/" + scene ) ) {
                AssetDatabase.CreateFolder("Assets/Editor/SmartScene", scene); 
            }

            mesh = ScriptableObject.CreateInstance<GridMesh>();
            mesh.Init();
            AssetDatabase.CreateAsset(mesh, "Assets/Editor/SmartScene/" + scene + "/GridMesh.asset");
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(mesh);

            mesh.ClearMaterials();
            mesh.AddSmartSceneMaterial(
                new TwoTeamDistanceMaterial("Two Team Distance")
            );
            mesh.AddSmartSceneMaterial(
                new SingleViewPointVisibilityMaterial("Single Point Visibility")
            );
            mesh.AddSmartSceneMaterial(
                new AreaToAreaVisibilityMaterial("Area To Area Visibility")
            );
            mesh.AddSmartSceneMaterial(
                new RandomColorMaterial("Random")
            );
            mesh.AddSmartSceneMaterial(
                new VertexGroupMaterial("Vertex Group Material")
            );
            mesh.AddSmartSceneMaterial(
                new LogicOpsVertexGroupMaterial("Set Logic Material")
            );
            mesh.AddSmartSceneMaterial(
                new FloatMinMaxToGroupMaterial("Value Range Material")
            );
            mesh.AddSmartSceneMaterial(
                new ErrorMaterial("Error Material")
            );
            mesh.SetActiveMaterial(0);

        } else {
            mesh.ReloadMesh();
            EditorUtility.SetDirty(mesh);
        }
        veticalLayerLimit = mesh.VertLayerLimit;

        tools = new SmartSceneTools();
        if( mesh.db == null ) {
            mesh.db = new SmartSceneDB();
        }
        mesh.db.SetGridMesh(mesh);
        db = mesh.db;
    }

    void OnDisable() {
        mesh?.SaveMaterials();
        mesh?.SaveDB();
        SceneView.duringSceneGui -= this.OnSceneGUI;
        Camera.onPostRender -= this.OnPostRender;
        EditorSceneManager.sceneOpened -= this.OnSceneOpened;    
    }

    void OnSceneOpened(Scene scene, OpenSceneMode mode) {
        Debug.Log("Active Scene Changed");
        // Save GridMesh Data
        mesh?.SaveMaterials();
        mesh?.SaveDB();
        AssetDatabase.SaveAssets();
        // Make sure changes dont get saved to disk
        EditorUtility.ClearDirty(mesh);
        // Nullify Mesh
        mesh = null;
        this.LoadGridMesh();
    }

    void OnGUI()
    {   
        debug = GUILayout.Toggle( debug, "Enable Debug Options" );

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;

        GUIStyle subTitle = new GUIStyle(GUI.skin.label);
        subTitle.alignment = TextAnchor.MiddleLeft;
        subTitle.fontStyle = FontStyle.BoldAndItalic;
        subTitle.fontSize -= 2;

        GUIStyle rightAlign = new GUIStyle(GUI.skin.label);
        rightAlign.alignment = TextAnchor.MiddleRight;

        GUIStyle rightAlignField = new GUIStyle(GUI.skin.textField);
        rightAlignField.alignment = TextAnchor.MiddleRight;
    

        tab = GUILayout.Toolbar (tab, new String[] {"Material", "GridMesh", "Tools", "Database"});
        switch (tab)
        {
            case 0:
                toolMode = false;
                GUILayout.Label("Material Settings", title );

                mesh.SetActiveMaterial(
                    EditorGUILayout.Popup ( mesh.ActiveMaterialIndex, mesh.Names ) 
                ); 

                if ( mesh.ActiveMaterial != null ) {
                    GUILayout.Label ( mesh.ActiveMaterial.DisplayName, subTitle );
                    mesh.ActiveMaterial.DrawGUI();

                    printWrites( "Vertex Attributes: ", mesh.ActiveMaterial.ProvidesVertexAttributes() );
                    printWrites( "Level Attributes: ", mesh.ActiveMaterial.WritesLevelAttributes() );

                    GUILayout.Space (20);
                    mesh.ActiveMaterial.AutoBake = GUILayout.Toggle( mesh.ActiveMaterial.AutoBake, "Auto Rebake on GridMesh Bake");
                    if ( GUILayout.Button("Bake Material")) {
                        mesh.ActiveMaterial.Bake( mesh );
                    }
                    
                }

                break;
            case 1:
                toolMode = false;
                GUILayout.Label("Grid Mesh Settings", title);
                GUILayout.Label("Current Mesh:", subTitle);
                GUILayout.Label( mesh.DoneBaking ? "Scene: " + mesh.sceneName : "", rightAlign);
                GUILayout.Label( mesh.DoneBaking ? "Baking finished" : "Baking outstanding", rightAlign );
                GUILayout.Label( mesh.DoneBaking ? mesh.Size + " vertices" : "" , rightAlign );
                GUILayout.Label( mesh.DoneBaking ? "Dimensions: " + mesh.Dimensions : "", rightAlign );
                GUILayout.Label( mesh.DoneBaking ? "Grid Dimensions: " + mesh.GridDimensions : "", rightAlign );
                
                GUILayout.Space(20);
                GUILayout.Label("General Render Settings:", subTitle);
                GUILayout.Label("Vertical Offset");
                verticalOffset = GUILayout.HorizontalSlider( verticalOffset, 0.05f, 2.0f );

                GUILayout.Space(20);
                GUILayout.Label("Bake Settings:", subTitle);
                GUILayout.Label("maximum number of vertices per vertical layer: ");
                
                int temp;
                String str = GUILayout.TextField( veticalLayerLimit == 0 ? "" : ""+veticalLayerLimit, rightAlignField);
                if ( Int32.TryParse( str, out temp ) ) {
                    veticalLayerLimit = temp;
                }
                else if ( str == "" ) {
                    veticalLayerLimit = 0;
                }
                GUILayout.Label("Expected asset size:\t\t" + SmartSceneUtilities.SizeInMiB(veticalLayerLimit) + " MiB");
                GUILayout.Label("Size per float data set:\t" + SmartSceneUtilities.DataSizeInMiB(veticalLayerLimit) + " MiB");
                GUILayout.Space(10);
                optimizeMesh = GUILayout.Toggle( optimizeMesh, "Optimize Mesh for Rendering");

                GUILayout.Label("Vertex Attributes:");
                foreach( String attr in db.floatVertexAttributes.Keys )
                    GUILayout.Label("\t" + attr);
                 foreach( String attr in db.stringVertexAttributes.Keys )
                    GUILayout.Label("\t" + attr);

                GUILayout.Space(10);
                if ( GUILayout.Button( "Bake Grid Mesh")) {
                    db.ResetAreaVertexGroups();
                    mesh.Bake(veticalLayerLimit, optimizeMesh);
                }

                if (debug ) {
                    if ( GUILayout.Button( "(DEBUG) Print Mesh to GameObject") ) {
                        GameObject obj = new GameObject();
                        MeshCollider coll = obj.AddComponent<MeshCollider>();
                        coll.sharedMesh = mesh.Mesh;
                    }
                }
                break;
            case 2:
                toolMode = true;
                tools.OnGUI();
                break;
            case 3:
                toolMode = false;
                printKeys( "Float Vertex Attributes", db.floatVertexAttributes.Keys.GetEnumerator() );
                printKeys( "String Vertex Attributes", db.stringVertexAttributes.Keys.GetEnumerator() );
                printKeys( "Float Level Attributes", db.floatLevelAttributes.Keys.GetEnumerator() );
                printKeys( "Vertex Groups", db.gridVertexGroups.Keys.GetEnumerator() );
                break;
            default:
                break;
        }
    }

    public void printWrites( String name, String[] attr ) {
        GUILayout.Label(name);
        if ( attr.Length == 0 )
            GUILayout.Label("\tnone");
        foreach(String s in attr) {
            GUILayout.Label("\t" + s);
        } 
    }

    public void printKeys( String name, IEnumerator<string> enumerator ) {
        GUILayout.Label(name);
        while (enumerator.MoveNext()) {
            GUILayout.Label("\t" + (string) enumerator.Current );
        }
    }

    void OnSceneGUI( SceneView sceneView ) {
        SmartSceneMaterial material = mesh?.ActiveMaterial;
        material?.PreDraw(mesh);
        material?.Draw(mesh, verticalOffset);

        if(toolMode) tools.OnSceneGUI(sceneView);

        Handles.BeginGUI();
        Handles.EndGUI();
    }

    void OnPostRender( ) {
        OnPostRender(null);
    }

    void OnPostRender( Camera cam ) {
        
    }

    void Update() {
        mesh?.Update();
        Repaint();
    }
}
