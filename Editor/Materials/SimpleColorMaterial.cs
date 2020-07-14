using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SimpleColorMaterial : ColorMaterial
{
    public SimpleColorMaterial ( String name ) : base ( name, "SmartScene/ColorShader" ) {}

    public override void PreDraw( GridMesh mesh) {
        if( isBaked ) {
            mesh.Mesh.colors = colors;
        }
    }
}
