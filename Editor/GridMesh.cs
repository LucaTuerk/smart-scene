using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;
using UnityEditor;
using System.Linq;
using System.Reflection;

[Serializable]
public class GridMesh : ScriptableObject
{
    // Bake Settings
    [SerializeField] bool doneBaking;
    public bool DoneBaking {
        get { return doneBaking; }
    }
    [SerializeField] public int vertLayerLimit;
    public int VertLayerLimit {
        get { return vertLayerLimit; }
    }

    // Mesh data 
    Mesh gridMesh;
    [SerializeField] Vector3[] vertices;
    [SerializeField] int[] triangles;
    public Mesh Mesh {
        get { return gridMesh; }
    }
    public int Size {
        get { return gridMesh == null ? 0 : gridMesh.vertexCount; }
    }

    public int[] WHOLE_MESH {
        get {
            int[] result = new int[Size];
            for(int i = 0; i < result.Length; i++ )
                result[i] = i;
            return result;
        }
    }

    // Materials
    [SerializeField] public List<SmartSceneMaterial> materials;
    [SerializeField] List<String> materialsJson;
    [SerializeField] List<String> materialsType; 
    [SerializeField] List<String> materialNames;

    public String[] Names {
        get {
            return materialNames.ToArray();
        }
    }

    // Scene Params
    [SerializeField] Vector3 dimensions;
    public Vector3 Dimensions {
        get { return dimensions; }
    }
    [SerializeField] Vector3Int gridDimensions;
    public Vector3Int GridDimensions {
        get { return gridDimensions; }
    }
    public float maxY;
    public float minY;

    public void Init ( ) {
         doneBaking = false;
         materials = new List<SmartSceneMaterial>();
         materialNames = new List<String>();
         vertLayerLimit = 40000;
    }

    void OnEnable()
    {
    }

    void OnDisable() {
        SaveMaterials();    
    }

    [SerializeField] public int activeMaterialIndex;
    public SmartSceneMaterial ActiveMaterial {
        get {
            if ( materials.Count == 0 || activeMaterialIndex >= materials.Count ) 
                return null; 
            return materials[activeMaterialIndex]; 
            }
    }
    public int ActiveMaterialIndex {
        get {
            return activeMaterialIndex;
        }
    }

    // Bake a new GridMesh
    public void Bake( int numberOfNodes, bool optimizeMesh) {
        // Reset Data
        doneBaking = false;
        maxY = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        foreach ( SmartSceneMaterial mat in materials ) {
            mat.Clear();
        }

        // Get Triangulation to Physics Raycast against 
        NavMeshTriangulation trig = UnityEngine.AI.NavMesh.CalculateTriangulation();
        Mesh nav = new Mesh();
        nav.vertices = trig.vertices;
        nav.triangles = trig.indices;
        GameObject go = new GameObject();
        MeshCollider coll = go.AddComponent<MeshCollider>();
        coll.sharedMesh = nav;

        // Get Scene Dimensions
        Vector3 minPos = Vector3.one * float.PositiveInfinity;
        Vector3 maxPos = Vector3.one * float.NegativeInfinity;
        Vector3 centerOfMass = Vector3.zero;
        foreach ( Vector3 vec in nav.vertices ) {
            minPos = Vector3.Min ( minPos, vec );
            maxPos = Vector3.Max ( maxPos, vec );
            centerOfMass += vec;
        }
        centerOfMass /= nav.vertices.Length;
        maxY = Math.Max( maxPos.y, maxY );
        minY = Math.Min( minPos.y, minY );
        dimensions = SmartSceneUtilities.VectorAbs( minPos - maxPos );

        int xNum, zNum;
        // Get side length of grid for desired number of nodes
        // dimensions.x / dimesions.z = xNum / zNum holds for uniform grid 
        xNum = (int) Mathf.Sqrt( ( dimensions.z / dimensions.x ) * numberOfNodes );
        zNum = (int) ( (float) xNum * ( dimensions.z / dimensions.x ) );
        gridDimensions = new Vector3Int(xNum, 0, zNum);
        float stepSize = dimensions.x / gridDimensions.x;
        // Get approximate vertices along vertical rays from Triangulation
        Vector3[,][] positions = new Vector3[xNum, zNum][];
        float startX =  - ( dimensions.x / 2 );
        float startZ =  - ( dimensions.z / 2 );
        float constY = maxY + 1.0f;


        NavMeshHit hit = new NavMeshHit();

        for ( int i = 0; i < xNum; i++ ) {
            for ( int j = 0; j < zNum; j++ ) {
                Vector3 scanHead = new Vector3 (
                    Mathf.Lerp ( minPos.x, maxPos.x, (float) i / (float) ( xNum - 1 ) ),
                    constY,
                    Mathf.Lerp ( minPos.z, maxPos.z, (float) j / (float) ( zNum - 1 ) )
                );

                positions[i,j] = GetHitsAlongRay( scanHead, coll );
                gridDimensions = Vector3Int.Max ( gridDimensions, new Vector3Int( 0, positions[i,j].Length, 0) );
                
                // Get Vertices shrunkwrapped onto the NavMesh
                for ( int w = 0; w < positions[i,j].Length; w++ ) {
                    Vector3 currPos = positions[i,j][w];
                    if ( NavMesh.SamplePosition ( currPos, out hit, 10 * stepSize , NavMesh.AllAreas ) ) {
                        currPos = hit.position;
                    }
                    positions[i,j][w] = currPos;
                }
            }
        }

        // Find Triangles
        List<Vector3Int> indicesVert = new List<Vector3Int>();
        
        for ( int i = 0; i < xNum - 1; i++ ) {
            for ( int j = 0; j < zNum - 1; j++ ) {
                Vector3 pos;
                int left, right, up, down;
                int count = 0;
                
                // Check this trig:
                    //  up 
                    //  |
                    //  |
                    // [i,j] --- right   
                for( int w = 0; w < positions[i,j].Length; w++ ) {
                    pos = positions[i,j][w];
                    up = GetConnectedVertex ( pos, positions[i,j+1], hit );
                    right = GetConnectedVertex ( pos, positions[i+1,j], hit );
                    
                    if ( up != -1 && right != -1) {
                        // Add Triangle
                        if ( IsConnected (positions[i,j+1][up], positions[i+1,j][right], hit ) ) {
                            count++;
                            indicesVert.Add( new Vector3Int(i,w,j) ); 
                            indicesVert.Add( new Vector3Int(i,up,j+1)); 
                            indicesVert.Add( new Vector3Int(i+1,right,j));
                        }
                    }    
                }

                // Check the other trig
                    //  left --- [i+1, j+1]
                    //                |
                    //                |
                    //              down
                for( int w = 0; w < positions[i+1,j+1].Length; w++ ) {
                        pos = positions[i+1,j+1][w];
                        left = GetConnectedVertex( pos, positions[i,j+1], hit );
                        down = GetConnectedVertex( pos, positions[i+1,j], hit );
                        
                        if ( left != -1 && down != -1 ) {
                            // Add Triangle
                            if ( IsConnected (positions[i,j+1][left], positions[i+1,j][down], hit ) ) {
                                count++;
                                indicesVert.Add( new Vector3Int(i+1,w,j+1) ); 
                                indicesVert.Add( new Vector3Int(i+1,down,j) ); 
                                indicesVert.Add( new Vector3Int(i,left,j+1) );
                            }
                        }
                    }

                // if neither trig is possible try the other diagonal
                //              up                   up  --- right
                //              |                     |
                //              |                     |   
                // [i,j] --- right      AND         [i,j]
                if ( count == 0 ) {
                    for( int w = 0; w < positions[i,j].Length; w++ ) {
                        pos = positions[i,j][w];
                        up = GetConnectedVertex ( pos, positions[i+1,j+1], hit );
                        right = GetConnectedVertex ( pos, positions[i+1,j], hit );

                        if ( up != -1 && right != -1 ) {
                            if ( IsConnected(positions[i+1,j+1][up], positions[i+1,j][right], hit ) ) {
                                count++;
                                indicesVert.Add( new Vector3Int(i,w,j) ); 
                                indicesVert.Add( new Vector3Int(i+1,up,j+1)); 
                                indicesVert.Add( new Vector3Int(i+1,right,j));
                            }
                        }
                    }

                    if ( count == 0 ) {
                    for( int w = 0; w < positions[i,j].Length; w++ ) {
                        pos = positions[i,j][w];
                        up = GetConnectedVertex ( pos, positions[i+1,j+1], hit );
                        right = GetConnectedVertex ( pos, positions[i+1,j], hit );

                        if ( up != -1 && right != -1 ) {
                            if ( IsConnected(positions[i+1,j+1][up], positions[i+1,j][right], hit ) ) {
                                indicesVert.Add( new Vector3Int(i,w,j) ); 
                                indicesVert.Add( new Vector3Int(i+1,up,j+1)); 
                                indicesVert.Add( new Vector3Int(i+1,right,j));
                            }
                        }
                    }
                }
                }
            }
        }

        // Minimize Vertex and Index Arrays disregarding unused vertices and pulling to one d
        Vec3IntComparer comparer = new Vec3IntComparer();
        Dictionary< Vector3Int, int > dict = new Dictionary<Vector3Int, int>(comparer);
        List<Vector3> OneDVertices = new List<Vector3>();
        List<int> OneDTriangles = new List<int>();

        int index = 0; // Index into vertices
        for( int j = 0; j < indicesVert.Count; j++ ) {
            if ( ! dict.ContainsKey( indicesVert[j] ) ) {
                dict.Add( indicesVert[j], index++ );
                OneDVertices.Add( positions[ indicesVert[j].x, indicesVert[j].z ][ indicesVert[j].y ]);
            }
            int key = dict[ indicesVert[ j ] ];
            OneDTriangles.Add( key );
        }

        gridMesh = new Mesh();
        if ( OneDVertices.Count > 65534 ) {
            gridMesh.indexFormat = IndexFormat.UInt32;
        }
        gridMesh.vertices = OneDVertices.ToArray();
        gridMesh.triangles = OneDTriangles.ToArray();
        gridMesh.RecalculateNormals();

        if (optimizeMesh) {
            gridMesh.Optimize();
        }
        SaveMesh();

        // Destroy stray gameObject
        DestroyImmediate(go, true);

        // Set Flag
        doneBaking = true;
        vertLayerLimit = numberOfNodes;

        // Bake materials
        foreach( SmartSceneMaterial mat in materials) {
            if (mat.AutoBake) {
                mat.Bake( this );
            }
        }
    }

    int GetConnectedVertex( Vector3 pos, Vector3[] neighbors, NavMeshHit hit) {
        int index = -1;
        float dist = float.PositiveInfinity;
        for ( int i = 0; i < neighbors.Length; i++ ) {
            if ( ! NavMesh.Raycast(pos, neighbors[i], out hit, NavMesh.AllAreas) ) {
                float currDist = Vector3.Distance(pos, neighbors[i]);
                if ( currDist < dist && currDist < 10.0f) {
                    index = i;
                    dist = currDist;
                }
            }
        }
        return index;
    }

    bool IsConnected( Vector3 pos, Vector3 neighbor, NavMeshHit hit) {
        if ( ! NavMesh.Raycast(pos, neighbor, out hit, NavMesh.AllAreas) ) {
            return true;
        }
        return false;
    } 

    public Vector3[] GetHitsAlongRay ( Vector3 origin, Collider coll ) {
        List<Vector3> hits = new List<Vector3>();
        
        RaycastHit hit;
        Ray ray = new Ray ( origin, Vector3.down );
        int i = 0; 
        while ( coll.Raycast( ray, out hit, float.PositiveInfinity ) ) {
            if ( i++ > 5 ) break;
            hits.Add(hit.point);
            ray = new Ray ( hit.point + 0.01f * Vector3.down, Vector3.down);
        }
        return hits.ToArray();
    }

    public Vector3 this[int i] {
        get { return Mesh.vertices[i]; }
    }

    // Get the closest grid position to the given position
    public Vector3 SamplePosition ( Vector3 position ) {
        int index = SampleIndex(position);
        if ( index != -1 ) 
            return gridMesh.vertices[index];
        return Vector3.zero;
    }

    public int SampleIndex ( Vector3 position ) {
        float dist = float.PositiveInfinity;
        float currDist;
        int index = -1;

        for ( int i = 0; i < gridMesh.vertexCount; i++ ) {
            currDist = Vector3.Distance(position, gridMesh.vertices[i] );
            if ( currDist < dist ) {
                index = i;
                dist = currDist;
            }
        }

        return index;
    }

    public int[] SampleIndicesInRange ( Vector3 positions, float distance ) {
        List<int> indices = new List<int>();
        Vector3 curr;
        for( int i = 0; i < gridMesh.vertexCount; i++ ) {
            if ( Vector3.Distance ( positions, gridMesh.vertices [i] ) < distance ) {
                indices.Add(i);
            }   
        }
        return indices.ToArray();
    }

    public Vector3[] SamplePositionsInRange ( Vector3 positions, float distance ) {
        List<Vector3> vertices = new List<Vector3>();
        Vector3 curr;
        for( int i = 0; i < gridMesh.vertexCount; i++ ) {
            curr = gridMesh.vertices [i];
            if ( Vector3.Distance ( positions, curr ) < distance ) {
                vertices.Add(curr);
            }   
        }
        return vertices.ToArray();
    }

    public int AddSmartSceneMaterial ( SmartSceneMaterial material ) {
        materials.Add(material);
        materialNames.Add(material.DisplayName);
        int index = materials.Count - 1;
        return materials.Count - 1;
    }

    public void SetActiveMaterial ( int index ) {
        activeMaterialIndex = index;
    }

    public void ClearMaterials () {
        materials.Clear();
        activeMaterialIndex = -1;
    }

    public void RenameActive (String name) {
        materials[activeMaterialIndex].Rename(name);
        materialNames[activeMaterialIndex] = name;
    }

    public void ReloadMesh() {
        gridMesh = new Mesh();
        gridMesh.vertices = vertices;
        gridMesh.triangles = triangles;
        LoadMaterials();

        foreach ( SmartSceneMaterial mat in materials )
            mat.Reload (this);
    }

    public void BakeMaterial ( int index ) {
        materials[ index ].Bake ( this );
    }

    public void SelectMaterial ( int index ) {
        if ( index < materials.Count )
            this.activeMaterialIndex = index;
    }

    public void SaveMesh() {
        vertices = gridMesh.vertices;
        triangles = gridMesh.triangles;
    }

    public void SaveMaterials() {

        materialsJson = new List<String>();
        materialsType = new List<String>();

        foreach( SmartSceneMaterial mat in materials ) {
            materialsJson.Add ( JsonUtility.ToJson( mat ) );
            materialsType.Add ( mat.GetType().FullName );
        }
    }

    public void LoadMaterials() {
        materials = new List<SmartSceneMaterial>();
        for ( int i = 0; i < materialsJson.Count && i < materialsType.Count; i++ ) {

            MethodInfo method = typeof(SmartSceneSerializer).GetMethod ( "Deserialize" ).MakeGenericMethod ( Type.GetType( materialsType[i] ) );

            object[] args = 
                new object[1];
            
            args[0] = materialsJson[i];

            AddSmartSceneMaterial (
                (SmartSceneMaterial) method.Invoke( null, args )
            );
        }

        for( int i = 0; i < materials.Count; i++ ) {
            materials[i].LoadShader();
        }
    }

    public void Update() {
        foreach( SmartSceneMaterial material in materials ) {
            material.Update();
        }
    }
 }
