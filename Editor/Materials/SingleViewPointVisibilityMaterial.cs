using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[Serializable]
public class SingleViewPointVisibilityMaterial : ColorMaterial
{
    [SerializeField]
    Vector3 viewPoint;

    [SerializeField]
    float fullHeight = 1.0f;
    [SerializeField]
    float crouchHeight = 0.5f;
    [SerializeField]
    bool onlyFullToFull = false;
    [SerializeField]
    bool invert = false;

    public SingleViewPointVisibilityMaterial ( String name ) : base ( name, "SmartScene/ColorShader" ) {}

    public override void Bake( GridMesh mesh ) {
        this.mesh = mesh;
        colors = new Color[mesh.Size];
        
        RaycastHit hit;
        Vector3 viewPos = viewPoint;
        // Find ground pos from viewPoint
        if ( Physics.Raycast(viewPoint, Vector3.down, out hit, fullHeight ) ) {
            
            viewPos = hit.point;
        }

        for ( int i = 0; i < mesh.Size; i++ ) {
            Vector3 target = mesh[i];

            bool f2f = false, f2c = false, c2f = false, c2c = false;
            Vector3 full = Vector3.up * fullHeight;
            Vector3 crouch = Vector3.up * crouchHeight;
            Vector3 
                Af = viewPoint + full, 
                Ac = viewPoint + crouch, 
                Bf = target + full, 
                Bc = target + crouch;

            f2f =
                !Physics.Raycast( Af, (Bf - Af).normalized , out hit, Vector3.Distance( Af, Bf ) );

            if ( !onlyFullToFull ) {
                f2c =
                    !Physics.Raycast( Af, (Bc - Af).normalized , out hit, Vector3.Distance( Af, Bc ) );
                c2f =
                    !Physics.Raycast( Ac, (Bf - Ac).normalized , out hit, Vector3.Distance( Ac, Bf) );
                c2c =
                    !Physics.Raycast( Ac, (Bc - Ac).normalized , out hit, Vector3.Distance( Ac, Bc) );
            }

            colors[i] =     new Color(0,0,0,0) +
                            ( f2f ? new Color(0.25f,0,0,(onlyFullToFull ? 1.0f : 0.25f)) : new Color(0,0,0,0) ) +
                            ( f2c ? new Color(0.25f,0,0,0.25f) : new Color(0,0,0,0) ) +
                            ( c2f ? new Color(0,0,0.25f,0.25f) : new Color(0,0,0,0) ) +
                            ( c2c ? new Color(0,0,0.25f,0.25f) : new Color(0,0,0,0) );
            if (invert) {
                colors[i] = new Color(1,1,1,1) - colors[i];
            }
        }
        isBaked = true;
    }

    public override void PreDraw( GridMesh mesh ) {

        if( isBaked ) {
            mesh.Mesh.colors = colors;
        }
    }

    public override void DrawGUI() {
        Transform selected = Selection.activeTransform;
        if ( selected != null ) {
            GUILayout.Label("Selected Object: \n" + selected.name );
        }
        else {
            GUILayout.Label("Select an object to edit attributes");
        }
        GUILayout.Space(20);

        if ( GUILayout.Button("Add as viewpoint") && selected != null ) {
            viewPoint = selected.position;
        }
        
        viewPoint = SmartSceneWindow.tools.SupplyPosition( "Use marked position", viewPoint );

        onlyFullToFull = GUILayout.Toggle( onlyFullToFull, "Only Render Full to Full Height Visibility");

        GUILayout.Space(10);

        GUILayout.Label("ViewPoint:");
        GUILayout.Label ( "\t" + viewPoint );
        
    }
}
