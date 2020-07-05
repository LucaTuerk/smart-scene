using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ColorMaterial : SmartSceneMaterial
{
    [SerializeField] public Color[] colors;
    protected GridMesh mesh;

    public new void Init ( string name, string shader ) {
        base.Init( name, shader );
    }

    public override void Bake ( GridMesh mesh ) { 
        this.mesh = mesh;
    }

    public override void Reload ( GridMesh mesh ) {
        this.mesh = mesh;
    }

    public override void DrawGUI() {
        
    }
}
