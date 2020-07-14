using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSceneSerializer
{
    public static String Serialize <T> ( T mat ) where T : SmartSceneMaterial {
        return JsonUtility.ToJson(mat);
    }

    public static T Deserialize <T> ( String mat ) where T : SmartSceneMaterial {
        return JsonUtility.FromJson <T> ( mat );
    }
}
