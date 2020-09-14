using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public abstract class SmartSceneMaterial
{
    [SerializeField] String displayName;
    public string DisplayName {
        get { return displayName; }
    }

    [SerializeField] public String shader;

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

    protected float progress;
    public float Progress {
        get {return progress; }
    }

    public SmartSceneMaterial( string name, string shader ) {
        displayName = name;
        this.shader = shader;
        this.autoBake = false;
        this.isBaked = false;
        material = new Material( Shader.Find(shader) );
    }

    public void Clear() {
        isBaked = false;
    }

    public abstract void PreDraw( GridMesh mesh );
    public abstract void Bake( GridMesh mesh );
    
    public void Draw( GridMesh mesh, float verticalOffset ) {
        if ( isBaked ) {
            material.SetPass(0);
            Graphics.DrawMeshNow( mesh.Mesh, Vector3.zero + verticalOffset * Vector3.up, Quaternion.identity );
        }
    }

    public void Draw( GridMesh mesh  ) {
        Draw( mesh, 0.125f);
    }

    public virtual void Update() {

    }

    public abstract void Reload( GridMesh mesh );

    public void LoadShader () {
        material = new Material( Shader.Find(shader) );
    }

    public void Rename ( String name ) {
        displayName = name;
    }

    public abstract void DrawGUI();

    public virtual String[] ProvidesVertexAttributes()      { return new String[0]; }
    public virtual String[] WritesLevelAttributes()         { return new String[0]; }
    public virtual String[] ProvidesGridVertexGroups()      { return new String[0]; }
    public virtual String[] ProvidesOffGridVertexGroups()   { return new String[0]; }
} 
