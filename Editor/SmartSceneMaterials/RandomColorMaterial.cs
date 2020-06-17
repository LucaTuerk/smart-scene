using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomColorMaterial : SimpleColorMaterial
{
    public RandomColorMaterial( string name ) : base( name ) {}

    public override void Bake( GridMesh mesh ) {
        colors = new Color[mesh.Size];
        for ( int i = 0; i < colors.Length; i++ ) {
            colors[i] = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);           
        }

        mesh.Mesh.colors = colors;
        this.mesh = mesh;
        
        isBaked = true;
    }
}
