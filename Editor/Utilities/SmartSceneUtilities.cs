using UnityEngine;
using System.Collections.Generic;

public class SmartSceneUtilities
{
    public static float SizeInMiB ( int num ) {
        return (float) ( (double) ( 16 * num ) / 1048576 );
    }

    public static float DataSizeInMiB ( int num ) {
        return (float) ( (double) ( 4 * num ) / 1048576 );
    }

    public static Vector3 VectorAbs ( Vector3 vec ) {
        return new Vector3 (
            Mathf.Abs ( vec.x ),
            Mathf.Abs ( vec.y ),
            Mathf.Abs ( vec.z)
        );
    }
}

public class Vec3IntComparer : IEqualityComparer<Vector3Int> {
    public bool Equals(Vector3Int vec1, Vector3Int vec2)
    {
        if ( Vector3Int.Distance( vec1, vec2 ) == 0 )
            return true;
        return false;
    }

    public int GetHashCode(Vector3Int vec)
    {
        // http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf
        int p1 = 73856093;
        int p2 = 19349663;
        int p3 = 83492791;

        return ( (p1 * vec.x) ^ (p2 * vec.y) ^ (p3 * vec.z ));
    }
}
