using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Marking {
    public Marking ( Vector3 position, Quaternion rotation ) {
        pos = position;
        rot = rotation;
    }

    public Vector3 pos;
    public Quaternion rot;
}