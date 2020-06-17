using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SimpleColorMaterial : ColorMaterial
{
    public SimpleColorMaterial( string name ) : base( name, "SmartScene/ColorShader" ) {}
}
