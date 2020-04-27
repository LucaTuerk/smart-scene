using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SmartSceneRuntime
    {
        int testSize = 20000;

        [Test]
        public void AddGameObjects () {
            SmartScene testScene = new SmartScene();

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            for ( int i = 0; i < testSize; i++ ) {
                GameObject obj = new GameObject ( "" + i );
                stopwatch.Start();
                testScene.AddNode ( obj );
                stopwatch.Stop();
            }
            stopwatch.Stop();
            long dt = stopwatch.ElapsedMilliseconds;
            UnityEngine.Debug.Log ( "Testsize: " + testSize );
            UnityEngine.Debug.Log ( "Total Add Time (ms): " + dt );
            UnityEngine.Debug.Log ( "Avg Add Time (ms): " + ( (double) dt / testSize ) );

            Assert.AreEqual ( testSize, testScene.Count );
        }

        [Test]
        public void AddEdgesTest () {
                SmartScene testScene = new SmartScene();
                int added = 0;

                for ( int i = 0; i < testSize; i++ ) {
                    testScene.AddNode ( new GameObject ( "" + i) );
                }

                List<GameObject> nodes = testScene.GetNodes();

                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                for ( int i = 0; i < testSize; i++ ) {
                    int node1 = UnityEngine.Random.Range(0, testSize);
                    int node2 = UnityEngine.Random.Range(0, testSize);
                    GameObject obj1 =  nodes[ node1 ];
                    GameObject obj2 =  nodes[ node2 ];
                    bool success = false;

                    stopwatch.Start();
                    success = testScene.AddEdge ( obj1, obj2 );
                    stopwatch.Stop();

                    if ( success ) {
                        added++;
                    }
                }
                stopwatch.Stop();
                long dt = stopwatch.ElapsedMilliseconds;
                UnityEngine.Debug.Log ( "Testsize: " + testSize );
                UnityEngine.Debug.Log ( "Total Add Time (ms): " + dt );
                UnityEngine.Debug.Log ( "Avg Add Time (ms): " + ( (double) dt / testSize ) );

                Assert.AreEqual ( testSize, testScene.Count );
                Assert.AreEqual ( added, testScene.EdgeCount );
        }

        [Test]
        public void AddAttributeTest () {
                SmartScene testScene = new SmartScene();
                Dictionary< (GameObject from, GameObject to), Attribute > testDict = 
                    new Dictionary< (GameObject from, GameObject to), Attribute>();

                for ( int i = 0; i < testSize; i++ ) {
                    testScene.AddNode ( new GameObject ( "" + i) );
                }

                List<GameObject> nodes = testScene.GetNodes();

                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                for ( int i = 0; i < testSize; i++ ) {
                    int node1 = UnityEngine.Random.Range(0, testSize);
                    int node2 = UnityEngine.Random.Range(0, testSize);
                    if ( testDict.ContainsKey ( ( nodes[ node1 ], nodes[ node2 ] ) ) ) {
                        i--;
                        continue;
                    }
                    Attribute attr = new BooleanAttribute ( "" + i, true );
                    
                    GameObject obj1 =  nodes[ node1 ];
                    GameObject obj2 =  nodes[ node2 ];

                    stopwatch.Start();
                    testDict.Add (
                        ( obj1, obj2 ),
                        attr
                    );
                    stopwatch.Stop();
                    
                    testScene.AddAttribute ( nodes[ node1 ], nodes[ node2 ], attr );
                }
                stopwatch.Stop();

                long dt = stopwatch.ElapsedMilliseconds;
                UnityEngine.Debug.Log ( "Testsize: " + testSize );
                UnityEngine.Debug.Log ( "Total Add Time (ms): " + dt );
                UnityEngine.Debug.Log ( "Avg Add Time (ms): " + ( (double) dt / testSize ) );

                Assert.AreEqual ( testSize, testScene.Count );
                Assert.AreEqual ( testSize, testScene.RelationCount );

                foreach ( (GameObject from, GameObject to) tuple in testDict.Keys ) {
                    Assert.IsTrue (
                        testScene.GetAttributes ( tuple.Item1, tuple.Item2 ).Contains ( testDict[ tuple ] )
                    );
                }
        }

        [Test]
        public void RemoveGameObjects () {
                SmartScene testScene = new SmartScene();

                for ( int i = 0; i < testSize; i++ ) {
                    testScene.AddNode ( new GameObject ( "" + i) );
                }

                List<GameObject> nodes = testScene.GetNodes();

                for ( int i = 0; i < nodes.Count - 1; i++) {
                    testScene.AddAttribute ( nodes[i], nodes[i+1], new BooleanAttribute ("connected", true ));
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Stop();
                foreach ( GameObject obj in nodes ) {
                    stopwatch.Start();
                    testScene.RemoveNode ( obj );
                    stopwatch.Stop();
                }
                stopwatch.Stop();
                long dt = stopwatch.ElapsedMilliseconds;
                UnityEngine.Debug.Log ( "Testsize: " + testSize );
                UnityEngine.Debug.Log ( "Total Remove Time (ms): " + dt );
                UnityEngine.Debug.Log ( "Avg Remove Time (ms): " + ( (double) dt / testSize ) );

                Assert.AreEqual ( 0, testScene.Count );
                Assert.AreEqual ( 0, testScene.EdgeCount );
                Assert.AreEqual ( 0, testScene.RelationCount );
        }

        [UnityTest]
        public IEnumerator SmartCleanUp () {
            SmartScene testScene = new SmartScene();

            for ( int i = 0; i < testSize; i++ ) {
                testScene.AddNode ( new GameObject ( "" + i) );
            }

            List<GameObject> nodes = testScene.GetNodes();

            for ( int i = 0; i < nodes.Count; i++) {
                testScene.AddAttribute ( nodes[i], nodes[ (i+1) % testSize ], new BooleanAttribute ("connected", true ));
            }

            foreach ( GameObject obj in testScene.GetNodes() ) {
                GameObject.Destroy ( obj );
            }

            yield return null;

            Assert.AreEqual ( 0, testScene.Count );
            Assert.AreEqual ( 0, testScene.EdgeCount );
            Assert.AreEqual ( 0, testScene.RelationCount );
        }
    }
}

