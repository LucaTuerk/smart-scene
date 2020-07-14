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

    String name;
    Color areaColor;

    bool showOutline;
    bool showMesh;

    public Area () {
        markings = new List<Marking>();
        areaMat = new Material ( Shader.Find("SmartScene/AreaShader"));
        areaColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f);
    }

    public void AddVertex( Marking current ) {
        collisionMesh = null;
        markings.Add(current);
    }

    public void RemoveVertex() {
        collisionMesh = null;
        if ( markings.Count != 0 ) {
            markings.RemoveAt(markings.Count - 1);
        }
    }

    public void OnGUI() {
        name = GUILayout.TextField( name );
        areaColor = EditorGUILayout.ColorField( "AreaColor", areaColor );

        if ( GUILayout.Button("Delete Last Vertex")) {
            RemoveVertex();
        }
        if ( GUILayout.Button("Clear Vertices")) {
            markings.Clear();
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
            extrudedVertices[i] = vertices[i] + Vector3.up;
            extrudedVertices[ vertices.Length + i ] = vertices[i] - Vector3.up;
        }

        int[] extrudedTriangles = new int[triangles.Length * 2 + 3 * extrudedVertices.Length ]; 
        for ( int i = 0; i < triangles.Length; i++ ) {
            extrudedTriangles[i] = triangles[i];
            extrudedTriangles[i + triangles.Length] = triangles[i] + vertices.Length;
        }
        for ( int i = triangles.Length*2, j = 0; j < vertices.Length; i+=6, j++) {
            Debug.Log(j);
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
    }
}