using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public abstract class SmartSceneMaterial
{
    [SerializeField] string displayName;
    public string DisplayName {
        get { return displayName; }
    }

    [SerializeField] string shader;

    [SerializeField] protected bool isBaked;
    public bool IsBaked {
        get { return isBaked; }
    }

    [SerializeField] protected bool autoBake;
    public bool AutoBake {
        get { return autoBake; }
        set { autoBake = value; }
    }

    [SerializeField] protected Material material;
    string materialPath;
    public Material Material {
        get { return material; }
    }

    public SmartSceneMaterial ( string name, string shader ) {
        displayName = name;
        this.shader = shader;
        material = new Material( Shader.Find(shader) );
    }
    

    public void Clear() {
        isBaked = false;
    }
    public abstract void Bake( GridMesh mesh );
    public abstract void PreDraw();
    
    public void Draw( GridMesh mesh, float verticalOffset ) {
        if ( isBaked ) {
            material.SetPass(0);
            Graphics.DrawMeshNow( mesh.Mesh, Vector3.zero + verticalOffset * Vector3.up, Quaternion.identity );
        }
    }

    public void Draw( GridMesh mesh  ) {
        Draw( mesh, 0.125f);
    }

    public abstract void Reload( GridMesh mesh );

    public void LoadShader () {
        material = new Material( Shader.Find(shader) );
    }

    public abstract void DrawGUI();
} 
