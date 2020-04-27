using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSceneCleanUp : MonoBehaviour
{
    SmartScene scene;
    public SmartScene Scene {
        get {
            return scene;
        }
        set {
            scene = value;
        }
    }
    // Make sure the node is removed upon Destruction to avoid zombie nodes in smart scene
    void OnDestroy() {
        if ( scene != null ) {
            scene.RemoveNode ( gameObject );
        }
    }
}
