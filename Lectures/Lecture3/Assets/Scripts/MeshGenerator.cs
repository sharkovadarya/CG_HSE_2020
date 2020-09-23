using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public MetaBallField Field = new MetaBallField();
    
    private MeshFilter _filter;
    private Mesh _mesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();

    private const int Cubes = 30;
    private const float Eps = 0.001f;

    /// <summary>
    /// Executed by Unity upon object initialization. <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// </summary>
    private void Awake()
    {
        // Getting a component, responsible for storing the mesh
        _filter = GetComponent<MeshFilter>();
        
        // instantiating the mesh
        _mesh = _filter.mesh = new Mesh();
        
        // Just a little optimization, telling unity that the mesh is going to be updated frequently
        _mesh.MarkDynamic();
    }

    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        
        Field.Update();

        var side = Math.Max(Math.Max(Field.BoundingBox.size.x, Field.BoundingBox.size.y), Field.BoundingBox.size.z) / Cubes;

        for (var i = 0; i < Cubes; i++)
        {
            for (var j = 0; j < Cubes; j++)
            {
                for (var k = 0; k < Cubes; k++)
                {
                    March(Field.BoundingBox.min + new Vector3(i, j, k) * side, side);
                }
            }   
        }

        // Here unity automatically assumes that vertices are points and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        // _mesh.RecalculateNormals(); // Use _mesh.SetNormals(normals) instead when you calculate them
        _mesh.SetNormals(normals);
        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }

    private void March(Vector3 point, float side)
    {
        var caseNumber = 0;
        for (var i = 0; i < MarchingCubes.Tables._cubeVertices.Length; i++)
        {
            if (Field.F(point + MarchingCubes.Tables._cubeVertices[i] * side) > 0)
            {
                caseNumber |= (1 << i);
            }
        }
        
        for (var i = 0; i < MarchingCubes.Tables.CaseToTrianglesCount[caseNumber]; i++)
        {
            var currentTriangle = MarchingCubes.Tables.CaseToVertices[caseNumber][i];
            for (var j = 0; j < 3; j++)
            {
                indices.Add(vertices.Count);
                
                var vertexPos = GetEdgeVertex(point, MarchingCubes.Tables._cubeEdges[currentTriangle[j]], side);
                vertices.Add(vertexPos);
                
                var normal = GetNormalVector(vertexPos);
                normals.Add(normal);
            }
        }
    }

    private Vector3 GetEdgeVertex(Vector3 point, IReadOnlyList<int> edge, float side)
    {
        var v0 = point + MarchingCubes.Tables._cubeVertices[edge[0]] * side;
        var v1 = point + MarchingCubes.Tables._cubeVertices[edge[1]] * side;

        var t = -Field.F(v0) / (Field.F(v1) - Field.F(v0));
        return v0 * (1 - t) + v1 * t;
    }

    private Vector3 GetNormalVector(Vector3 point)
    {
        return new Vector3(
            Field.F(point - new Vector3(Eps, 0, 0)) - Field.F(point + new Vector3(Eps, 0, 0)),
            Field.F(point - new Vector3(0, Eps, 0)) - Field.F(point + new Vector3(0, Eps, 0)),
            Field.F(point - new Vector3(0, 0, Eps)) - Field.F(point + new Vector3(0, 0, Eps))
        ).normalized;
    }
}