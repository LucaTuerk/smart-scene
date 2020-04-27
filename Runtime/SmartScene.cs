using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartScene
{
    Dictionary < GameObject, List< GameObject > > edges;
    Dictionary < GameObject, List< GameObject > > children;
    Dictionary < (GameObject from, GameObject to), List < Attribute > > relations;

    public int Count {
        get {
            return edges.Keys.Count;
        }
    }
    
    public int EdgeCount {
        get {
            int count = 0;
            foreach ( List< GameObject > list in edges.Values ) {
                count += list.Count;
            }
            return count;
        }
    }

    public int RelationCount {
        get {
            int count = 0;
            foreach ( List < Attribute > list in relations.Values ) {
                count += list.Count;
            }
            return count;
        }
    }

    public SmartScene () {
        this.edges = new Dictionary<GameObject, List<GameObject>>();
        this.children = new Dictionary<GameObject, List<GameObject>>();
        this.relations = new Dictionary< (GameObject from, GameObject to), List<Attribute>>();
    }

    public bool AddNode ( GameObject obj ) {
        if ( ! edges.ContainsKey (obj) ) {

            SmartSceneCleanUp clean = obj.AddComponent <SmartSceneCleanUp> ( );
            clean.Scene = this;
            
            edges.Add ( obj, new List< GameObject > () );
            return true;
        }
        return false;
    }

    public void RemoveNode ( GameObject obj ) {
        RemoveRelationsFrom ( obj );
        RemoveEdgesTo ( obj );
        edges.Remove ( obj );
        RemoveChildRelationshipFromAll ( obj );
        children.Remove ( obj );
    }

    void RemoveRelationsFrom ( GameObject obj ) {
        if ( edges.ContainsKey ( obj ) ) { 
            foreach ( GameObject other in edges[ obj ] ) {
                relations.Remove ( ( obj, other ) );
            }
        }
    }

    void RemoveEdgesTo ( GameObject obj ) {
        List<GameObject> from = edges.Where ( item => item.Value.Contains ( obj ) ).Select ( item => item.Key ).ToList();
        foreach ( GameObject other in from ) {
            relations.Remove ( ( other, obj ) );
            edges[ other ].Remove( obj );
        }
    }

    void RemoveChildRelationshipFromAll ( GameObject obj ) {
        children.Remove ( obj );
        List<GameObject> parents = children.Where ( item => item.Value.Contains ( obj ) ).Select ( item => item.Key ).ToList();
        foreach ( GameObject parent in parents ) {
            children[ parent ].Remove ( obj );
        }
    }

    public bool AddEdge ( GameObject from, GameObject to ) {
        AddNode( from );
        
        if ( ! edges[ from ].Contains ( to )) {
            edges[ from ].Add ( to );
            return true;
        }
        return false;
    }

    public void AddChild ( GameObject parent, GameObject child ) {
        if ( ! children.ContainsKey ( parent ) ) {
            children.Add ( parent, new List< GameObject > ( ) { child } );
        }
        else {
            children[ parent ].Add ( child );
        }
    }

    public void AddAttribute ( GameObject from, GameObject to, Attribute attr) {
        AddEdge ( from, to );
        (GameObject from, GameObject to) key = ( from, to );

        if ( ! relations.ContainsKey ( key ) ) {
            relations.Add ( key, new List< Attribute > ( ) { attr } );
        }
        else {
            relations [ key ].Add ( attr );
        }
    }

    public List< GameObject > GetNeighbours ( GameObject from ) {
        if ( edges.ContainsKey ( from ) ) {
            return edges [ from ];
        }
        return null;
    }

    public List< GameObject > GetChildren ( GameObject parent ) {
        if ( children.ContainsKey ( parent ) ) {
            return children [ parent ];
        }
        return null;
    }

    public List< Attribute > GetAttributes ( GameObject from, GameObject to ) {
        (GameObject from, GameObject to) key = (from, to);
        
        if ( relations.ContainsKey ( key ) ) {
            return relations [ key ];
        }
        return null;
    }

    public List< Attribute > GetAttributesByName ( GameObject from, GameObject to, String name) {
        List< Attribute > res = GetAttributes ( from, to);
        if ( res != null ) {
            return res.Where ( x => x.Name == name ).ToList();
        }
        return null;
    }

    public List< GameObject > GetNodes ( ) {
        return edges.Keys.ToList();
    }
} 
