using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoverObject : MonoBehaviour
{
	private Collider sourceCollider;
	private GameObject coverPointsParent;
	private LayerMask coverBetweenThreatMask;

	[Header("Cover Points Auto Generation Settings")]
	[SerializeField] private float spacing = 2f;
	[SerializeField] private float offsetFromSurface = 0.75f;

	[Header("Cover Points List")]
	[SerializeField] private List<Transform> coverPoints;

	[Header("Cover Type")]
	[SerializeField] private CoverType cover;
	public CoverType Cover => cover;

	public enum CoverType
	{
		pelvisHeightCover,
		chestHeightCover,
		FullCover
	}

	private void Awake()
	{
		coverBetweenThreatMask = LayerMask.GetMask("Environment", "EnvironmentCover", "CharacterDetection");
		AssignSourceCollider();
		UpdateCoverPoint();

		if (gameObject.layer != LayerMask.NameToLayer("EnvironmentCover"))
			Debug.LogError($"CoverObject '{name}' is not on 'Environment' layer, set it to 'EnvironmentCover'.");

		if (coverPoints.Count <= 0)
			Debug.LogWarning($"CoverObject '{name}' has no cover points, either add them or remove the component.");
	}

	private void AssignSourceCollider()
	{
		if (sourceCollider == null)
			sourceCollider = GetComponent<Collider>();
	}

	public bool GetClosestPointBehindCover(Vector3 position, Vector3 threatPosition, out Vector3? coverPosition)
	{
		float bestDist = float.MaxValue;
		coverPosition = null;

		for (int i = 0; i < coverPoints.Count - 1; i++)
		{
			Vector3 a = coverPoints[i].position;
			Vector3 b = coverPoints[i + 1].position;

			Vector3 point = GetClosestPointOnSegment(a, b, position);

			if (!Physics.Linecast(threatPosition, point, coverBetweenThreatMask))
				continue;

			float dist = Vector3.Distance(position, point);

			if (dist < bestDist)
			{
				bestDist = dist;
				coverPosition = point;
			}
		}

		return coverPosition != null;
	}
	private Vector3 GetClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
	{
		Vector3 ab = b - a;
		float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
		t = Mathf.Clamp01(t);
		return a + ab * t;
	}

	#region Cover Point Management
	public void AutoGenerateCoverPoints(bool removeExistingPoints)
	{
		AssignSourceCollider();

		if (removeExistingPoints)
			ClearAllCoverPoints(true);

		var (edges, center, extentsMax) = GetEdges();

		GenerateCoverPointsFromEdges(edges, center, extentsMax);
	}
	#region Auto Generation Helpers
	private ((Vector3 a, Vector3 b)[] edges, Vector3 center, float extentsMax) GetEdges()
	{
		Bounds bounds = sourceCollider.bounds;
		Vector3 center = bounds.center;
		float extentsMax = Mathf.Max(bounds.extents.x, bounds.extents.z) + 1f;

		float minX = bounds.min.x;
		float maxX = bounds.max.x;
		float minZ = bounds.min.z;
		float maxZ = bounds.max.z;
		float y = center.y;

		Vector3 bl = new(minX, y, minZ); // bottom-left
		Vector3 br = new(maxX, y, minZ); // bottom-right
		Vector3 tr = new(maxX, y, maxZ); // top-right
		Vector3 tl = new(minX, y, maxZ); // top-left

		var edges = new (Vector3 a, Vector3 b)[] { (bl, br), (br, tr), (tr, tl), (tl, bl) };

		return (edges, center, extentsMax);
	}
	private void GenerateCoverPointsFromEdges((Vector3 a, Vector3 b)[] edges, Vector3 center, float extentsMax)
	{
		for (int ei = 0; ei < edges.Length; ei++)
		{
			Vector3 a = edges[ei].a;
			Vector3 b = edges[ei].b;
			float length = Vector3.Distance(a, b);
			int segments = Mathf.Max(1, Mathf.CeilToInt(length / spacing));

			for (int i = 0; i <= segments; i++)
			{
				if (i == segments && ei < edges.Length - 1) //avoid duplicating corners, skip last sample of every edge except final edge
					continue;

				float t = (segments == 0) ? 0f : i / (float)segments;
				Vector3 sample = Vector3.Lerp(a, b, t);

				Vector3 normal = ComputeEdgeNormal(a, b, center, sample);
				TryCreatePointFromSample(sample, normal, extentsMax);
			}
		}
	}
	// Compute outward-facing normal for an edge sample
	private Vector3 ComputeEdgeNormal(Vector3 a, Vector3 b, Vector3 center, Vector3 sample)
	{
		Vector3 edgeDir = (b - a).normalized;
		Vector3 normal = Vector3.Cross(Vector3.up, edgeDir).normalized;

		if (Vector3.Dot(normal, sample - center) < 0f) //ensure point is outside collider
			normal = -normal;

		return normal;
	}

	// Raycasts from outside toward the collider surface and creates a cover point if the hit is valid
	private bool TryCreatePointFromSample(Vector3 sample, Vector3 normal, float extentsMax)
	{
		Vector3 rayStart = sample + normal * extentsMax;

		if (Physics.Raycast(rayStart, -normal, out RaycastHit hit, extentsMax * 2f))
		{
			if (hit.collider != sourceCollider)
				return false;

			Vector3 point = hit.point + hit.normal * offsetFromSurface;
			point.y = hit.point.y;
			CreateNewCoverPoint(point);
			return true;
		}

		return false;
	}
	#endregion

	public void CreateNewCoverPoint(Vector3 position)
	{
		if (coverPointsParent == null || transform.GetChild(0) == null)
		{
			coverPointsParent = new GameObject("CoverPointsParent");
			coverPointsParent.transform.SetParent(transform);
			coverPointsParent.transform.localPosition = Vector3.zero;
		}

		Transform newPoint = new GameObject($"CoverPoint {coverPoints.Count + 1}").transform;

		coverPointsParent = transform.GetChild(0).gameObject;
		newPoint.SetParent(coverPointsParent.transform);
		newPoint.position = position;
		coverPoints.Add(newPoint);
	}
	public void RemoveLastCoverPoint()
	{
		if (coverPoints.Count > 0)
		{
			Transform lastPoint = coverPoints[^1];
			coverPoints.RemoveAt(coverPoints.Count - 1);
			Destroy(lastPoint.gameObject);
		}
	}
	public void ClearAllCoverPoints(bool editorCall = false)
	{
		foreach (Transform point in coverPoints)
		{
			if (point != null)
			{
				if (editorCall)
					DestroyImmediate(point.gameObject);
				else
					Destroy(point.gameObject);
			}
		}
		coverPoints.Clear();
	}
	public void UpdateCoverPoint()
	{
		for (int i = coverPoints.Count - 1; i >= 0; i--)
		{
			if (coverPoints[i] != null) continue;

			coverPoints.RemoveAt(i);
			Debug.LogWarning("Removed null cover point from list.");
		}
	}
	#endregion

	#region Gizmos
	private void OnDrawGizmos()
	{
		if (coverPoints == null || coverPoints.Count < 2)
			return;

		Gizmos.color = Color.green;

		for (int i = 0; i < coverPoints.Count - 1; i++)
		{
			if (coverPoints[i] != null && coverPoints[i + 1] != null)
			{
				Gizmos.DrawLine(coverPoints[i].position, coverPoints[i + 1].position);
				Gizmos.DrawSphere(coverPoints[i].position, 0.1f);
			}
		}

		Gizmos.DrawSphere(coverPoints[^1].position, 0.1f);
	}
	#endregion
}
