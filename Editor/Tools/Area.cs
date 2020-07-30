using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class Area {
    Mesh collisionMesh;

    public List<Marking> markings;
    Material areaMat;

    public String name;
    Color areaColor;

    bool showOutline;
    bool showMesh;

    MeshCollider collider;
    GameObject gameObject;

    int[] vertexGroup;

    public static int WHOLE_MESH {
        get {
            return SmartSceneWindow.db.areas.Count;
        }
    }

    public Area () {
        markings = new List<Marking>();
        areaMat = new Material ( Shader.Find("SmartScene/AreaShader"));
        areaColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f);
    }

    public Area ( int i ) {
        markings = new List<Marking>();
        areaMat = new Material ( Shader.Find("SmartScene/AreaShader"));
        areaColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f);
        name = "Area" + i;
    }


    public void AddVertex( Marking current ) {
        collisionMesh = null;
        vertexGroup = null;
        markings.Add(current);
    }

    public void RemoveVertex() {
        collisionMesh = null;
        vertexGroup = null;
        if ( markings.Count != 0 ) {
            markings.RemoveAt(markings.Count - 1);
        }
    }

    public void Clear() {
        collisionMesh = null;
        vertexGroup = null;
        markings.Clear();
    }

    public void OnGUI( bool allowRename = true ) {
        if (allowRename) name = GUILayout.TextField( name );
        areaColor = EditorGUILayout.ColorField( "AreaColor", areaColor );

        if ( GUILayout.Button("Delete Last Vertex")) {
            RemoveVertex();
        }
        if ( GUILayout.Button("Clear Vertices")) {
            Clear();
        }
    }

    public void Draw(Mesh markingMesh, Material markingMaterial) {
        for ( int i = 0; i < markings.Count; i++ ) {
            markingMaterial.SetPass(0);
            Graphics.DrawMeshNow( markingMesh, markings[i].pos, markings[i].rot );

            areaMat.SetPass(0);
            areaMat.SetColor("_Color", areaColor);
            Graphics.DrawMeshNow( GetCollisionMesh(), Vector3.zero , Quaternion.identity);

            Handles.DrawLine(markings[i].pos + 0.65f * Vector3.up, markings[ (i+1) % markings.Count ].pos + 0.65f * Vector3.up);
        }
    }

    public Mesh GetCollisionMesh() {
        if ( collisionMesh == null ) 
            CreateCollisionMesh();
        return collisionMesh;
    }

    void CreateCollisionMesh() {
        collisionMesh = new Mesh();
        Vector2[] vertices2d = new Vector2[ markings.Count ];
        Vector3[] vertices = new Vector3[ markings.Count ];
        for( int i = 0; i < vertices.Length; i++ ) {
            vertices[i] = markings[i].pos;
            vertices2d[i] = new Vector2( vertices[i].x, vertices[i].z);
        }

        Triangulator triangulator = new Triangulator(vertices2d);
        int[] triangles = triangulator.Triangulate();

        Vector3[] extrudedVertices = new Vector3[ vertices.Length * 2 ];
        for ( int i = 0; i < vertices.Length; i++ ) {
            extrudedVertices[i] = vertices[i] + 2 * Vector3.up;
            extrudedVertices[ vertices.Length + i ] = vertices[i] - 2 * Vector3.up;
        }

        int[] extrudedTriangles = new int[triangles.Length * 2 + 3 * extrudedVertices.Length ]; 
        for ( int i = 0; i < triangles.Length; i++ ) {
            extrudedTriangles[i] = triangles[i];
            extrudedTriangles[i + triangles.Length] = triangles[i] + vertices.Length;
        }
        for ( int i = triangles.Length*2, j = 0; j < vertices.Length; i+=6, j++) {
            extrudedTriangles[i] = j;
            extrudedTriangles[i+1] = ( j + vertices.Length ) % extrudedVertices.Length;
            extrudedTriangles[i+2] = ( j + 1 ) % vertices.Length;
            extrudedTriangles[i+3] = ( j + 1 )  % vertices.Length;
            extrudedTriangles[i+4] = ( j + vertices.Length ) % extrudedVertices.Length;
            extrudedTriangles[i+5] = (j == vertices.Length - 1) ?
                vertices.Length : 
                ( j + vertices.Length + 1 ) % extrudedVertices.Length;
        }

        collisionMesh.vertices = extrudedVertices;
        collisionMesh.triangles = extrudedTriangles;
        collisionMesh.RecalculateNormals();
        collisionMesh.RecalculateTangents();
        collisionMesh.RecalculateBounds();
    }

    public int[] GetVertexGroup( GridMesh mesh ) {
        if ( this.vertexGroup == null ) 
        {
            SetupCollider();

            List<int> vertices = new List<int>();
            for ( int i = 0; i < mesh.Size; i++ ) {
                if ( IsInside( mesh[i] ) ) {
                    vertices.Add(i);
                }
            }

            ClearCollider();
            this.vertexGroup = vertices.ToArray();
        }
        return this.vertexGroup;
    }

    public bool IsInside ( Vector3 pos ) {
        if ( ! GetCollisionMesh().bounds.Contains( pos ))
            return false;

        bool responsibleForClear = false;
        if ( collider == null ) {
            SetupCollider();
            responsibleForClear = true;
        }

        int count = 0;

        for ( int i = 0; i < 2; i++ ) {
            Vector3 start = Vector3.zero;
            if ( i == 0 ) {
                 // CHECK FRONT FACES
                start = pos - Vector3.Scale( Vector3.right, GetCollisionMesh().bounds.size );
            }
            if ( i == 1 ) {
                // CHECK BACK FACES
                start = pos;
                pos = start - Vector3.Scale( Vector3.right, GetCollisionMesh().bounds.size );
            }

            Vector3 dir = pos - start;
            RaycastHit hit = new RaycastHit();
            
            Ray ray = new Ray(start, dir);
            int j = 0;
            while ( collider.Raycast(ray, out hit, Vector3.Distance(start, pos) ) ) {
                start = hit.point + 0.01f * dir;
                ray = new Ray(start, dir);
                j++;
                count++;
                if ( j > 1000 )
                    Debug.Log("ALARM");
            }
        }

        if ( responsibleForClear ) {
            ClearCollider();
        }
        if ( count % 2 == 0 )
            return false;
        return true;
    }

    void SetupCollider() {
        gameObject = new GameObject();
        collider = gameObject.AddComponent<MeshCollider>( );
        collider.sharedMesh = GetCollisionMesh();
    }

    void ClearCollider() {
        collider = null;
        GameObject.DestroyImmediate(gameObject);
    }

    public void ResetVertexGroup() {
        this.vertexGroup = null;
    }   
}