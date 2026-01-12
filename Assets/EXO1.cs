using System.Collections.Generic;
using UnityEngine;

public class EXO1 : MonoBehaviour
{
    Mesh mesh;
    [SerializeField]List<Vector3> points = new List<Vector3>();
    [SerializeField]List<Vector3> original = new List<Vector3>();


    struct Edge
    {
        public int a, b;

        public Edge(int v1, int v2)
        {
            a = Mathf.Min(v1, v2);
            b = Mathf.Max(v1, v2);
        }
    }


    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        /*
        points.Add(new Vector3(0,0,0));
        points.Add(new Vector3(1, 1, 0));
        points.Add(new Vector3(2, 0, 0));
        points.Add(new Vector3(1, -1, 0));
        */
        original.AddRange(points);
        for (int i = 0; i < 3; i++)
        {
            points = StartChaikin();
        }
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

    }

    List<Vector3> StartChaikin()
    {
        List<Vector3> newList = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 firstPoint = FirstPoint(points[i], points[i + 1]);
            Vector3 lastPoint = LastPoint(points[i], points[i + 1]);
            newList.Add(firstPoint);
            newList.Add(lastPoint);
        }
        Vector3 firstPointLoop = FirstPoint(points[points.Count - 1], points[0]);
        Vector3 lastPointLoop = LastPoint(points[points.Count - 1], points[0]);
        newList.Add(firstPointLoop);
        newList.Add(lastPointLoop);
        return newList;
    }

    Vector3 FirstPoint(Vector3 parent1, Vector3 parent2)
    {
        Vector3 answer;
        answer = 0.75f*parent1 + 0.25f*parent2;
        return answer;

    }

    Vector3 LastPoint(Vector3 parent1, Vector3 parent2)
    {
        Vector3 answer;
        answer = 0.25f * parent1 + 0.75f * parent2;
        return answer;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.DrawLine(points[points.Count - 1], points[0]);
        Gizmos.color = Color.green;
        for (int i = 0; i < original.Count - 1; i++)
        {
            Gizmos.DrawLine(original[i], original[i + 1]);
        }
        Gizmos.DrawLine(original[original.Count - 1], original[0]);
    }
}
