using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SmartSceneWindow : EditorWindow
{
    GridMesh mesh;
    
    Transform selected;
    int num;

    int tab = 0;
    bool debug;
    bool optimizeMesh = true;
    Material mat;
    float verticalOffset = 0.1f;

    [MenuItem("Window/Smart Scene/Settings")]
    static void Init()
    {
        SmartSceneWindow window = (SmartSceneWindow)EditorWindow.GetWindow(typeof(SmartSceneWindow));
        window.name = "Smart Scene Settings";

        window.Show();
    }

    void OnEnable() {
        if ( mesh == null )
            mesh = AssetDatabase.LoadAssetAtPath( "Assets/Editor/SmartScene/GridMesh.asset", typeof( GridMesh ) ) as GridMesh;
        // Create Asset Files
        if ( mesh == null ) {
            Debug.Log("SmartScene: Creating GridMesh asset" );

            if ( !AssetDatabase.IsValidFolder( "Assets/Editor" ) ) {
                AssetDatabase.CreateFolder("Assets", "Editor");
            }
            if ( !AssetDatabase.IsValidFolder( "Assets/Editor/SmartScene" ) ) {
                AssetDatabase.CreateFolder("Assets/Editor", "SmartScene"); 
            }
            if ( !AssetDatabase.IsValidFolder( "Assets/Editor/SmartScene/SmartSceneMaterials" ) ) {
                AssetDatabase.CreateFolder("Assets/Editor/SmartScene", "SmartSceneMaterials"); 
            }
            mesh = ScriptableObject.CreateInstance<GridMesh>();
            mesh.Init();
            AssetDatabase.CreateAsset(mesh, "Assets/Editor/SmartScene/GridMesh.asset");
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(mesh);

            mesh.ClearMaterials();
            mesh.AddSmartSceneMaterial(
                new TwoTeamDistanceMaterial()
            );
            mesh.SetActiveMaterial(0);
            ( mesh.ActiveMaterial as TwoTeamDistanceMaterial ).Init("Distance");
        } else {
            mesh.ReloadMesh();
            EditorUtility.SetDirty(mesh);
        }
        num = mesh.VertLayerLimit;

        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        Camera.onPostRender += this.OnPostRender;
    }

    void OnDisable() {
        mesh?.SaveMaterials();

        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        Camera.onPostRender -= this.OnPostRender;
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
    

        tab = GUILayout.Toolbar (tab, new String[] {"Material", "GridMesh", "Areas", "Events"});
        switch (tab)
        {
            case 0:
                // selected = Selection.activeTransform;
                // if ( selected != null ) {
                //     GUILayout.Label("Selected Object: \n" + selected.name );
                // }
                // else {
                //     GUILayout.Label("Select an object to edit attributes");
                // }
                GUILayout.Label("Material Settings", title );
                if ( mesh.ActiveMaterial != null ) {
                    GUILayout.Label ( mesh.ActiveMaterial.DisplayName, subTitle );
                    mesh.ActiveMaterial.DrawGUI();

                    GUILayout.Space (20);
                    mesh.ActiveMaterial.AutoBake = GUILayout.Toggle( mesh.ActiveMaterial.AutoBake, "Auto Rebake on GridMesh Bake");
                    if ( GUILayout.Button("Bake Material")) {
                        mesh.ActiveMaterial.Bake( mesh );
                    }
                }

                break;
            case 1:
                GUILayout.Label("Grid Mesh Settings", title);
                GUILayout.Label("Current Mesh:", subTitle);
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
                String str = GUILayout.TextField( num == 0 ? "" : ""+num, rightAlignField);
                if ( Int32.TryParse( str, out temp ) ) {
                    num = temp;
                }
                else if ( str == "" ) {
                    num = 0;
                }
                GUILayout.Label("Expected asset size:\t\t" + SmartSceneUtilities.SizeInMiB(num) + " MiB");
                GUILayout.Label("Size per float data set:\t" + SmartSceneUtilities.DataSizeInMiB(num) + " MiB");
                GUILayout.Space(10);
                optimizeMesh = GUILayout.Toggle( optimizeMesh, "Optimize Mesh for Rendering");
                GUILayout.Space(10);
                if ( GUILayout.Button( "Bake Grid Mesh")) {
                    mesh.Bake(num, optimizeMesh);
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
                break;
            case 3:
                break;
            default:
                break;
        }
    }

    void OnSceneGUI( SceneView sceneView ) {
        Handles.BeginGUI();
        Handles.EndGUI();
    }

    void OnPostRender( ) {
        OnPostRender(null);
    }

    void OnPostRender( Camera cam ) {
        SmartSceneMaterial material = mesh?.ActiveMaterial;
        material?.PreDraw(mesh);
        material?.Draw(mesh, verticalOffset);
    }

    void Update() {
        Repaint();
    }
}
