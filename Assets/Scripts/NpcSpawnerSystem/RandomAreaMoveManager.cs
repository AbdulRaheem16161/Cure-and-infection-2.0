using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways] // this script willl now run both in Play Mode and in Edit Mode
public class RandomAreaMoveManager : MonoBehaviour
{
	private int _pointsHash = 0;
	private List<Triangle> _cachedTriangles = new();

	public GameObject AreaPointsFolder;
	public List<GameObject> AreaPoints = new();

	public bool ShowGizmoz;

    // Cached triangulation
    private struct Triangle
    {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        public float area;
    }

	readonly System.Random systemRandom = new();

    public void CreateNewAreaPoint()
    {
        GameObject areaPoint = new($"Point{transform.childCount + 1}");
        areaPoint.transform.SetParent(AreaPointsFolder != null ? AreaPointsFolder.transform : transform);
        areaPoint.transform.localPosition = Vector3.zero;
        Selection.activeGameObject = areaPoint;

        AreaPoints.Add(areaPoint);
        // force rebuild on next request
        _pointsHash = 0;
    }

	#region get random Vector3 move point from random point in a random triangle
	public Vector3 GetRandomLocationInArea()
    {
        EnsureCachedTrianglesUpToDate();

        if (_cachedTriangles == null || _cachedTriangles.Count == 0)
			return transform.position;

        Triangle triangle = _cachedTriangles[systemRandom.Next(0, _cachedTriangles.Count)];

        // Random point inside triangle using barycentric coordinates
        float u = Random.value;
        float v = Random.value;
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }

        Vector2 point2D = triangle.a + u * (triangle.b - triangle.a) + v * (triangle.c - triangle.a);
        Vector3 sampleCenter = new(point2D.x, transform.position.y + 1f, point2D.y);

        if (NavMesh.SamplePosition(sampleCenter, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            return hit.position;

        return new(point2D.x, transform.position.y, point2D.y);
    }
	#endregion

	#region keep cached triangles upto date with hash + rebuild if area point change
	private void EnsureCachedTrianglesUpToDate()
    {
		if (AreaPoints == null) { Debug.LogError("Area Points null"); return; }
		if (AreaPoints.Count < 3) { Debug.LogError($"Not Enough Area Points to create area: {AreaPoints.Count}/3"); return; }

        for (int i = AreaPoints.Count - 1; i > 0; i--)
        {
            if (AreaPoints[i] == null)
                AreaPoints.RemoveAt(i);
        }

		int hash = ComputePointsHash();
        if (_cachedTriangles.Count > 0 && hash == _pointsHash)
            return;

		RebuildTriangleCache();
        _pointsHash = hash;
    }
    private int ComputePointsHash()
    {
        unchecked
        {
            int hash = (AreaPoints != null) ? AreaPoints.Count : 0;
            if (AreaPoints != null)
            {
                foreach (var go in AreaPoints)
                {
                    if (go == null)
                    {
                        hash = hash * 397;
                        continue;
                    }

                    hash = hash * 397 ^ go.transform.position.GetHashCode();
                }
            }
            return hash;
        }
    }
	#endregion

	#region rebuilding and caching area triangles based on area points from level design
	// Build triangles from AreaPoints using ear clipping and cache them with their areas
	private void RebuildTriangleCache()
    {
        _cachedTriangles.Clear();

        List<Vector2> poly = new();
        foreach (var go in AreaPoints)
        {
            if (go == null) continue;
            Vector3 p = go.transform.position;
            poly.Add(new Vector2(p.x, p.z));
        }

        List<int> tris = Triangulate(poly);
        if (tris == null || tris.Count < 3)
            return;

        for (int i = 0; i < tris.Count; i += 3)
        {
            Vector2 a = poly[tris[i + 0]];
            Vector2 b = poly[tris[i + 1]];
            Vector2 c = poly[tris[i + 2]];

            float area = Mathf.Abs((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) * 0.5f;
            if (area <= Mathf.Epsilon) continue;

            Triangle t = new() { a = a, b = b, c = c, area = area };
            _cachedTriangles.Add(t);
        }
    }

    // Ear clipping triangulation for a simple polygon (may be concave). Returns list of indices (triplets).
    private List<int> Triangulate(List<Vector2> poly)
    {
        int n = poly.Count;
        List<int> indices = new(n);
        for (int i = 0; i < n; i++) indices.Add(i);

        List<int> result = new();

        // Determine polygon orientation (signed area)
        float signedArea = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % n];
            signedArea += (a.x * b.y) - (b.x * a.y);
        }
        bool isCCW = signedArea > 0f;

        int guard = 0;
        while (indices.Count > 3 && guard < n * n)
        {
            bool earFound = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prevIndex = indices[(i - 1 + indices.Count) % indices.Count];
                int currIndex = indices[i];
                int nextIndex = indices[(i + 1) % indices.Count];

                Vector2 a = poly[prevIndex];
                Vector2 b = poly[currIndex];
                Vector2 c = poly[nextIndex];

                if (!IsConvex(a, b, c, isCCW))
                    continue;

                bool hasPointInside = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    int vi = indices[j];
                    if (vi == prevIndex || vi == currIndex || vi == nextIndex) continue;
                    if (PointInTriangle(poly[vi], a, b, c))
                    {
                        hasPointInside = true;
                        break;
                    }
                }

                if (hasPointInside) continue;

                // ear found
                result.Add(prevIndex);
                result.Add(currIndex);
                result.Add(nextIndex);
                indices.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound)
            {
                // Failed to find an ear: polygon might be invalid or degenerate. Abort.
                break;
            }
            guard++;
        }

        if (indices.Count == 3)
        {
            result.Add(indices[0]);
            result.Add(indices[1]);
            result.Add(indices[2]);
        }

        return result;
    }

    private bool IsConvex(Vector2 a, Vector2 b, Vector2 c, bool isCCW)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return isCCW ? (cross > Mathf.Epsilon) : (cross < -Mathf.Epsilon);
    }

    private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Barycentric technique
        Vector2 v0 = c - a;
        Vector2 v1 = b - a;
        Vector2 v2 = p - a;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float denom = dot00 * dot11 - dot01 * dot01;
        if (Mathf.Abs(denom) < Mathf.Epsilon) return false;

        float invDenom = 1f / denom;
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0f) && (v >= 0f) && (u + v <= 1f);
    }
	#endregion

	#region gizmos to draw area points + triangles
	private void OnDrawGizmos()
    {
        if (!ShowGizmoz) return;
		DrawAreaPointsAndTriangles();
    }

    public void DrawAreaPointsAndTriangles()
    {
		EnsureCachedTrianglesUpToDate();
		Gizmos.color = new Color(0.2f, 0.4f, 1f);

        for (int i = 0; i < AreaPoints.Count; i++)
        {
			if (AreaPoints[i] == null) continue;
			Gizmos.DrawSphere(AreaPoints[i].transform.position, 1f); //draw a Sphere on every point
        }

		for (int i = 0; i < _cachedTriangles.Count; i++)
		{
			var t = _cachedTriangles[i];
			Vector3 a = new(t.a.x, transform.position.y, t.a.y);
			Vector3 b = new(t.b.x, transform.position.y, t.b.y);
			Vector3 c = new(t.c.x, transform.position.y, t.c.y);

			Gizmos.DrawLine(a, b);
			Gizmos.DrawLine(b, c);
			Gizmos.DrawLine(c, a);
		}
	}
	#endregion
}