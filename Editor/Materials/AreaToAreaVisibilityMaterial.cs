using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;

[Serializable]
public class AreaToAreaVisibilityMaterial : ColorMaterial
{
    [SerializeField]
    Vector3 viewPoint;

    [SerializeField]
    float fullHeight;
    [SerializeField]
    float crouchHeight;
    [SerializeField]
    bool onlyFullToFull = false;
    [SerializeField]
    bool invert = false;
    
    Vector3 full;

    int areaA, areaB;
    int samples;
    int max;
    int current;
    int[] aIndices, bIndices;
    JobHandle[] handles;
    bool baking;
    bool scheduled;
    NativeArray<RaycastHit>[] results;
    NativeArray<RaycastCommand>[] commands;
    Vector3[][] targets;
    int distinctTargetsCount = 100;
    static int batches= 100;
    static int perUpdate = 10;
    int scheduledCount;

    int sampleIndex;
    int distinctIndex;
    System.Random random;

    public AreaToAreaVisibilityMaterial ( String name ) : base ( name, "SmartScene/ColorShader" ) {
        fullHeight = 2.0f;
        crouchHeight = 1.0f;
        samples = 10;
        baking = false;
        sampleIndex = 0;
        distinctIndex = 0;
    }

    public override void Bake( GridMesh mesh ) {
        if ( !baking && areaA <= SmartSceneWindow.db.areas.Count && areaB <= SmartSceneWindow.db.areas.Count ) {
            this.mesh = mesh;
            colors = new Color[mesh.Size];

            random = new System.Random();
            
            if( areaA != Area.WHOLE_MESH ) aIndices = SmartSceneWindow.db.areas[areaA].GetVertexGroup(mesh);
            else aIndices = mesh.WHOLE_MESH;
            
            if( areaB != Area.WHOLE_MESH ) bIndices = SmartSceneWindow.db.areas[areaB].GetVertexGroup(mesh);
            else bIndices = mesh.WHOLE_MESH;

            max = Math.Min(aIndices.Length, samples);

            handles = new JobHandle[batches];
            results = new NativeArray<RaycastHit>[batches];
            commands = new NativeArray<RaycastCommand>[batches];
            
            targets = new Vector3[distinctTargetsCount][];

            full = Vector3.up * this.fullHeight;

            for( int j = 0; j < distinctTargetsCount; j++ ) {
                targets[j] = new Vector3[max];
                List<int> source = new List<int>(aIndices);

                for( int i = 0; i < max; i++ ) {
                    int index = random.Next(source.Count);
                    targets[j][i] = mesh[source[index]];
                    source.RemoveAt(index);
                }
            }

            for(int i = 0; i < batches; i++) {
                results[i] = new NativeArray<RaycastHit>(max, Allocator.TempJob);
                commands[i] = new NativeArray<RaycastCommand>(max, Allocator.TempJob);
            }
            
            current = 0;
            baking = true;
            isBaked = false;
            scheduled = false;
            scheduledCount = 0;
            progress = 0.0f;
        }
    }

    public JobHandle Schedule( Vector3 origin, Vector3[] targets, int index ) 
    {
        Vector3 A = origin + full;

        for( int i = 0; i < targets.Length; i++ ) {

            Vector3 dir = targets[i] + full - A;

            commands[index][i] = 
                new RaycastCommand( A, dir.normalized, dir.magnitude );
        }

        return RaycastCommand.ScheduleBatch(commands[index], results[index], 1, default(JobHandle));
    }

    public override void Update() {
        
         if(!scheduled && baking && !isBaked) {
                int i = 0;
                for (; scheduledCount < batches && i < perUpdate && current + scheduledCount < bIndices.Length; i++, scheduledCount++) {
                    //targets[random.Next(targets.Length)]
                    handles[scheduledCount] = Schedule( mesh[bIndices[current+scheduledCount]], targets[ (current + scheduledCount) % targets.Length ], scheduledCount );
                }
                if ( scheduledCount >= batches - 1 || current + scheduledCount >= bIndices.Length - 1) { 
                    scheduledCount = 0;
                    scheduled = true;
                }
            }
        
        for ( int j = 0; j < perUpdate; j++ ) {
           
            if( scheduled && baking && !isBaked && current < bIndices.Length && handles[current%batches].IsCompleted )
            {  
                handles[current%batches].Complete();

                int f2f = 0;
                foreach( RaycastHit h in results[current%batches] ) {
                    f2f += (h.collider == null) ? 1 : 0;
                }
                float f2ffac = (float) f2f / (float) ( max );
                colors[bIndices[current]] = new Color( f2ffac, f2ffac, f2ffac, 1.0f );
                
                current++;
                if(current % batches == 0)
                    scheduled = false;
    
                progress = (float) current / bIndices.Length;

                if ( current == bIndices.Length ) {
                    baking = false;
                    isBaked = true;
                    for ( int i = 0; i < batches; i++ ) {
                        results[i].Dispose();
                        commands[i].Dispose();
                    }
                }
            } 
        }
    }

    public override void PreDraw( GridMesh mesh ) {
        if( isBaked ) {
            mesh.Mesh.colors = colors;
        }
    }

    public override void DrawGUI() {
        if ( !baking ) {

            GUILayout.Label("Area A:");
            areaA = SmartSceneWindow.db.AreaSelectGUI(areaA, false, true);
            GUILayout.Label("Area B:");
            areaB = SmartSceneWindow.db.AreaSelectGUI(areaB, false, true);

            String[] sampleChoise = {"32", "64", "128", "256", "512", "1024", "2048"};
            String[] distinctChoise = {"1", "5", "25", "50", "100", "200"};
            
            GUILayout.Label("Samples: ");
            sampleIndex = GUILayout.Toolbar (sampleIndex, sampleChoise);
            GUILayout.Label("Distinct Target Sets: ");
            distinctIndex = GUILayout.Toolbar (distinctIndex, distinctChoise);
            
            GUILayout.Label("Batches: " + batches);
            batches = (int) GUILayout.HorizontalSlider ( (float) batches, 10.0f, 20_000.0f );
            GUILayout.Space(10);
            GUILayout.Label("Per Update: " + perUpdate);
            perUpdate = Math.Min( (int) GUILayout.HorizontalSlider ( (float) perUpdate, 1.0f, batches ), batches );

            switch( sampleIndex ) {
                case 0: samples = 32;
                        break;
                case 1: samples = 64;
                        break;
                case 2: samples = 128;
                        break;
                case 3: samples = 256;
                        break;
                case 4: samples = 512;
                        break;
                case 5: samples = 1024;
                        break;
                case 6: samples = 2048;
                    break;
                default: break;
            }

            switch( distinctIndex) {
                case 0: distinctTargetsCount = 1;
                        break;
                case 1: distinctTargetsCount = 5;
                        break;
                case 2: distinctTargetsCount = 25;
                        break;
                case 3: distinctTargetsCount = 50;
                        break;
                case 4: distinctTargetsCount = 100;
                        break;
                case 5: distinctTargetsCount = 200;
                        break;
                default: break;
            }

            GUILayout.Space(10);
            GUILayout.Space(10);
        }
        else {
            GUILayout.Label("Baking ... " + (int) ( Progress * 100.0f) + "%" );
        }

    }

    public void Initialize () {
        if ( areaA != areaB && areaA < SmartSceneWindow.db.areas.Count && areaB < SmartSceneWindow.db.areas.Count ) {
            aIndices = SmartSceneWindow.db.areas[areaA].GetVertexGroup(mesh);
            bIndices = SmartSceneWindow.db.areas[areaB].GetVertexGroup(mesh);
        }
    }
}
