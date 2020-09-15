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
    float fullHeight;
    Vector3 full;

    int indexA, indexB;
    bool useAreaA, useAreaB;

    int samples;
    int max;
    int current;
    int finished;
    int[] aIndices, bIndices;
    JobHandle[] handles;
    bool baking;
    NativeArray<RaycastHit>[] results;
    NativeArray<RaycastCommand>[] commands;
    Vector3[][] targets;
    float[] visibility;
    float maxVisibility;
    int distinctTargetsCount = 100;
   
    static int numberOfJobs;

    Stack<int> availableHandles;
    Stack<int> usedHandles;
    Stack<int> vertexIndices;
    Stack<int> confirmedPenumbra;
    bool firstStage;
    int check = 64;
    long castRays;

    float t;
    System.Diagnostics.Stopwatch watch;

    int sampleIndex;
    int distinctIndex;
    int penumbraIndex;
    System.Random random;

    String vertexAttrName;
    String wroteAttribute;
    String maxVisibAttrName;

    public AreaToAreaVisibilityMaterial ( String name ) : base ( name, "SmartScene/VisibilityShader" ) {
        fullHeight = 2.0f;
        samples = 10;
        baking = false;
        sampleIndex = 0;
        distinctIndex = 0;
    }

    public override void Bake( GridMesh mesh ) {
        if ( !baking ) {
            this.mesh = mesh;
            colors = new Color[mesh.Size];

            random = new System.Random();

            aIndices = SmartSceneWindow.db.GetVertexGroup ( indexA, ref useAreaA, mesh);
            bIndices = SmartSceneWindow.db.GetVertexGroup ( indexB, ref useAreaB, mesh);
            
            if ( aIndices != null && bIndices != null && aIndices.Length > 0 && bIndices.Length > 0) {
                
                max = Math.Min(aIndices.Length, samples );
                check = Math.Min(max,check);
                int arraySize = 2 * check > max ? max : max - check;

                handles = new JobHandle[numberOfJobs];
                results = new NativeArray<RaycastHit>[numberOfJobs];
                commands = new NativeArray<RaycastCommand>[numberOfJobs];
                
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

                availableHandles = new Stack<int>();
                usedHandles = new Stack<int>();
                vertexIndices = new Stack<int>();
                confirmedPenumbra = new Stack<int>();
                visibility = new float[ mesh.Size ];
                maxVisibility = 0.0f;

                for(int i = 0; i < numberOfJobs; i++) {
                    results[i] = new NativeArray<RaycastHit>(arraySize, Allocator.Persistent);
                    commands[i] = new NativeArray<RaycastCommand>(arraySize, Allocator.Persistent);
                    availableHandles.Push(i);
                }
                
                current = 0;
                finished = 0;
                baking = true;
                isBaked = false;
                progress = 0.0f;
                watch = System.Diagnostics.Stopwatch.StartNew();
                t = 0.0f;
                castRays = 0;
                firstStage = true;
            }
        }
    }

    public JobHandle Schedule( Vector3 origin, Vector3[] targets, int index, int minIndex, int maxIndex) 
    {
        Vector3 A = origin + full;

        for( int i = minIndex, j = 0; i < targets.Length && j < maxIndex; i++, j++ ) {

            Vector3 dir = targets[i] + full - A;

            commands[index][j] = 
                new RaycastCommand( A, dir.normalized, dir.magnitude );
                
            castRays++;
        }

        return RaycastCommand.ScheduleBatch(commands[index], results[index], 1, default(JobHandle));
    }

    public override void Update() {
        
        if (baking ) {
            
            if ( finished + confirmedPenumbra.Count == bIndices.Length ) {
                firstStage = false;
            }

            while( usedHandles.Count != 0 && handles[usedHandles.Peek()].IsCompleted ) {
                    int index = usedHandles.Pop();
                    availableHandles.Push(index);
                    handles[index].Complete();

                    int f2f = 0;
                    int c = 0;
                    int localMax = firstStage ? check : max - check;

                    foreach( RaycastHit h in results[index] ) {
                        f2f += (h.collider == null) ? 1 : 0;
                        if ( ++c == localMax ) 
                            break;
                    }
                    float f2ffac = (float) f2f / (float) ( firstStage ? check : max - check);
                    int vert = bIndices[vertexIndices.Pop()];

                    if ( firstStage ) {
                        if (f2f == 0 || f2f == localMax || max == check) {
                            finished ++;
                        } else {
                            confirmedPenumbra.Push(vert);
                        }
                        visibility[vert] = f2ffac;
                    } else {
                        double  ratio = (double) check / (double) max;
                        visibility[vert] = (float) (ratio * (double) visibility[vert] + (1-ratio) * (double) f2ffac);
                        finished++;
                    }

                    colors[vert] = new Color( visibility[vert] , visibility[vert] , visibility[vert] , 1.0f );
                    maxVisibility = Mathf.Max( maxVisibility, visibility[vert] );
        
                    progress = (float) finished / bIndices.Length;

                    if ( finished == bIndices.Length ) {
                        watch.Stop();
                        t = (float) ((double) watch.ElapsedMilliseconds / 1000.0);

                        SmartSceneWindow.db.AddPerVertexFloatAttribute( vertexAttrName, visibility );
                        SmartSceneWindow.db.AddFloatLevelAttribute( maxVisibAttrName, maxVisibility );

                        baking = false;
                        isBaked = true;
                        

                        for ( int i = 0; i < numberOfJobs; i++ ) {
                            results[i].Dispose();
                            commands[i].Dispose();
                        }
                    }
            }

            while( availableHandles.Count != 0 && ( current != bIndices.Length || ( !firstStage && confirmedPenumbra.Count != 0 ))) {
                    int index = availableHandles.Pop();

                    usedHandles.Push(index);
                    
                    int currentVert = firstStage ? current : confirmedPenumbra.Pop();
                    vertexIndices.Push(currentVert);

                    int mini, maxi;
                    if (firstStage) {
                        mini = 0; maxi = check;
                    } else {
                        mini = check; maxi = max;
                    }

                    handles[index] = Schedule( mesh[bIndices[currentVert]], targets[ (currentVert) % targets.Length ], index, mini, maxi);
                    
                    if ( firstStage ) current++;
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
            indexA = SmartSceneWindow.db.VertexGroupSelectGUI(indexA, ref useAreaA);
            GUILayout.Label("Area B:");
            indexB = SmartSceneWindow.db.VertexGroupSelectGUI(indexB, ref useAreaB);

            
            String a = SmartSceneWindow.db.GetVertexGroupName( indexA, useAreaA );
            String b = SmartSceneWindow.db.GetVertexGroupName( indexB, useAreaB );

            vertexAttrName = a + "_TO_" + b + "_VISIBILITY_"+samples + "_" + check;
            maxVisibAttrName = a + "_TO_" + b + "_MAX_VISIBILITY";
            
        
            String[] sampleChoise = {"32", "64", "128", "256", "512", "1024", "2048"};
            String[] penumbraChoise = { "8", "16", "32", "64", "128", "256", "512"};
            String[] distinctChoise = {"1", "5", "25", "50", "100", "200"};

            GUILayout.Label("Samples: ");
            int temp = sampleIndex;
            sampleIndex = GUILayout.Toolbar (sampleIndex, sampleChoise);
            if ( sampleIndex != temp ) penumbraIndex = sampleIndex;
            GUILayout.Label("Penumbra Samples: ");
            penumbraIndex = GUILayout.Toolbar (penumbraIndex, penumbraChoise);
            GUILayout.Label("Distinct Target Sets: ");
            distinctIndex = GUILayout.Toolbar (distinctIndex, distinctChoise);

            
            GUILayout.Label("Number of Jobs: " + numberOfJobs);
            numberOfJobs = Math.Min( Math.Max( (int) GUILayout.HorizontalSlider ( (float) numberOfJobs, 10.0f, 5000.0f ), 10 ), 5000);
            GUILayout.Space(10);
            GUILayout.Label("Decrease for Editor Responsiveness \nIncrease for faster render times [citation needed].");
            GUILayout.Space(10);

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

            switch( penumbraIndex ) {
                case 0: check = 8;
                        break;
                case 1: check = 16;
                        break;
                case 2: check = 32;
                        break;
                case 3: check = 64;
                        break;
                case 4: check = 128;
                        break;
                case 5: check = 256;
                        break;
                case 6: check = 512;
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
            if( t != 0.0f ) {
                GUILayout.Label("Rendertime: " + t +" seconds");
                GUILayout.Label("Cast Rays: " + castRays);
            }
        }
        else {
            GUILayout.Label("Baking ... " + (int) ( Progress * 100.0f) + "%" );
        }

    }

    public override String[] ProvidesVertexAttributes() { 
        return new String[] { vertexAttrName };
    }
    public override String[] WritesLevelAttributes()         { 
        return new String[] { maxVisibAttrName };
    }
    public override String[] ProvidesGridVertexGroups()      { return new String[0]; }
    public override String[] ProvidesOffGridVertexGroups()   { return new String[0]; }
}
