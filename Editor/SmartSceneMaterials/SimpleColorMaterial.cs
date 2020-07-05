using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SimpleColorMaterial : ColorMaterial
{
    public void Init( string name ) {
        base.Init( name, "SmartScene/ColorShader" );
    }

    public override void PreDraw( GridMesh mesh) {
        if( isBaked ) {
            mesh.Mesh.colors = colors;
        }
    }
}
