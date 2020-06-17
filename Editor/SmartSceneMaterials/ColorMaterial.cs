using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMaterial : SmartSceneMaterial
{
   public ColorMaterial( string name, string shader ) : base( name, shader ) {}

    [SerializeField] public Color[] colors;
    protected GridMesh mesh;
    
    public override void PreDraw () {
        if ( IsBaked ) {
            mesh.Mesh.colors = colors;
        }
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
