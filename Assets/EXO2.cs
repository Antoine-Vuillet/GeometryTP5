using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EXO2 : MonoBehaviour
{
    Mesh mesh;
    Edge[] edges;
    [SerializeField] List<Vector3> points = new List<Vector3>();
    Dictionary<(int, int), int> edgeVertexMap = new Dictionary<(int, int), int>();


    struct Edge
    {
        public int a, b, third, fourth, newVertexIndex;

        public Edge(int v1, int v2, int v3)
        {
            a = Mathf.Min(v1, v2);
            b = Mathf.Max(v1, v2);
            third = v3;
            fourth = -1;
            newVertexIndex = -1;
        }

        public void addFourth(int v)
        {
            fourth = v;
        }
    }
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        edges = new Edge[mesh.triangles.Length];
        GetEdges();
        removeDoubles();
        Charles();

    }


    void GetEdges()
    {
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Edge edge1 = new Edge(mesh.triangles[i], mesh.triangles[i + 1], mesh.triangles[i+2]);
            Edge edge2 = new Edge(mesh.triangles[i + 1], mesh.triangles[i + 2], mesh.triangles[i]);
            Edge edge3 = new Edge(mesh.triangles[i + 2], mesh.triangles[i], mesh.triangles[i+1]);
            edges[i] = edge1;
            edges[i + 1] = edge2;
            edges[i + 2] = edge3;
        }
    }

    void removeDoubles()
    {
        List<Edge> uniqueEdges = new List<Edge>();
        bool isUnique = true;
        int fourthVertex = -1;
        int indexToUpdate = -1;
        for (int i = 0; i < edges.Length; i++)
        {
            for (int j = 0; j < uniqueEdges.Count; j++)
            {
                if (edges[i].a == uniqueEdges[j].a && edges[i].b == uniqueEdges[j].b)
                {
                    isUnique = false;
                    fourthVertex = edges[i].third;
                    indexToUpdate = j;
                }
            }
            if (!isUnique)
            {
                uniqueEdges[indexToUpdate].addFourth(fourthVertex);
            }
            if (isUnique)
            {
                uniqueEdges.Add(edges[i]);
            }
            isUnique = true;
        }
        edges = uniqueEdges.ToArray();
    }

    void Charles()
    {
        edgeVertexMap.Clear();

        List<Vector3> oldVertices = new List<Vector3>(mesh.vertices);
        points.Clear();
        points.AddRange(oldVertices);

        foreach (Edge edge in edges)
        {
            int min = edge.a;
            int max = edge.b;
            Vector3 edgePoint;

            if (edge.fourth == -1)
            {
                edgePoint = 0.5f * (oldVertices[min] + oldVertices[max]);
            }
            else
            {
                edgePoint = (3f / 8f) * (oldVertices[min] + oldVertices[max])
                            + (1f / 8f) * (oldVertices[edge.third] + oldVertices[edge.fourth]);
            }

            int newIndex = points.Count;
            points.Add(edgePoint);
            edgeVertexMap[(min, max)] = newIndex;
        }

        for (int i = 0; i < oldVertices.Count; i++)
        {
            points[i] = newVertex(i, oldVertices);
        }

        List<int> newTriangles = new List<int>();
        int[] tris = mesh.triangles;

        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];

            int ab = edgeVertexMap[(Mathf.Min(a, b), Mathf.Max(a, b))];
            int bc = edgeVertexMap[(Mathf.Min(b, c), Mathf.Max(b, c))];
            int ca = edgeVertexMap[(Mathf.Min(c, a), Mathf.Max(c, a))];

            newTriangles.AddRange(new int[] { a, ab, ca });
            newTriangles.AddRange(new int[] { b, bc, ab });
            newTriangles.AddRange(new int[] { c, ca, bc });
            newTriangles.AddRange(new int[] { ab, bc, ca });
        }

        mesh.Clear();
        mesh.vertices = points.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    Vector3 newVertex(int index, List<Vector3> oldVertices)
    {
        List<int> neighbors = new List<int>();
        bool isBoundary = false;

        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i].a == index)
            {
                neighbors.Add(edges[i].b);
                if (edges[i].fourth == -1) isBoundary = true;
            }
            else if (edges[i].b == index)
            {
                neighbors.Add(edges[i].a);
                if (edges[i].fourth == -1) isBoundary = true;
            }
        }

        if (isBoundary)
        {
            Vector3 sumBoundary = Vector3.zero;
            int count = 0;
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].fourth == -1)
                {
                    if (edges[i].a == index) { sumBoundary += oldVertices[edges[i].b]; count++; }
                    else if (edges[i].b == index) { sumBoundary += oldVertices[edges[i].a]; count++; }
                }
            }
            return 0.75f * oldVertices[index] + 0.25f * (sumBoundary / count);
        }
        else
        {
            int n = neighbors.Count;
            float alpha = (n == 3) ? 3f / 16f : (1f / n) * ((5f / 8f) - Mathf.Pow((3f / 8f + 0.25f * Mathf.Cos(2f * Mathf.PI / n)), 2f));
            Vector3 sum = Vector3.zero;
            foreach (int neighbor in neighbors) sum += oldVertices[neighbor];
            return (1 - n * alpha) * oldVertices[index] + alpha * sum;
        }
    }



    int countEdges(int index)
    {
        int count = 0;
        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i].a == index || edges[i].b == index)
            {
                count++;
            }
        }
        return count;
    }

    float calcAlpha(int index, int n)
    {
        float alpha;
        if (n == 3)
        {
            alpha = 3f / 16f;
        }
        else
        {
            alpha = (1f / n) * ((5f / 8f) - Mathf.Pow((3f / 8f) + (1f / 4f) * Mathf.Cos((2f * Mathf.PI) / n), 2f));
        }
        return alpha;
    }

    


    bool edgesHaveFourth(int v1, int v2)
    {
        int min = Mathf.Min(v1, v2);
        int max = Mathf.Max(v1, v2);
        if (!edgeVertexMap.ContainsKey((min, max))) return false;

        foreach (var edge in edges)
        {
            if (edge.a == min && edge.b == max)
                return edge.fourth != -1;
        }
        return false;
    }

}
