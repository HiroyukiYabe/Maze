using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CreateSimplePlane : MonoBehaviour {
	
	[MenuItem("MyTools/Create Simple Plane/Z forward")]
	static void CreateZ()
	{
		GameObject obj = new GameObject("SimplePlaneZ");
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		Mesh m = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Mesh/SimplePlaneZ.asset", typeof(Mesh));
		if(m == null)
		{
			m = new Mesh();
			m.name = "SimplePlaneZ";
			Vector3[] vertices = new Vector3[]
			{
				new Vector3( 0.5f,  0.5f, 0.0f),
				new Vector3(-0.5f, -0.5f, 0.0f),
				new Vector3(-0.5f,  0.5f, 0.0f),
				new Vector3( 0.5f, -0.5f, 0.0f)
			};
			int[] triangles = new int[]
			{
				0, 1, 2,
				3, 1, 0
			};
			Vector2[] uv = new Vector2[]
			{
				new Vector2(1.0f, 1.0f),
				new Vector2(0.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 0.0f)
			};
			m.vertices = vertices;
			m.triangles = triangles;
			m.uv = uv;
			m.RecalculateNormals();
			if (!Directory.Exists("Assets/Mesh"))
				Directory.CreateDirectory("Assets/Mesh");
			AssetDatabase.CreateAsset(m, "Assets/Mesh/SimplePlaneZ.asset");
			AssetDatabase.SaveAssets();
		}
		meshFilter.sharedMesh = m;
		m.RecalculateBounds();
	}


	[MenuItem("MyTools/Create Simple Plane/Y forward")]
	static void CreateY()
	{
		GameObject obj = new GameObject("SimplePlaneY");
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		Mesh m = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Mesh/SimplePlaneY.asset", typeof(Mesh));
		if(m == null)
		{
			m = new Mesh();
			m.name = "SimplePlaneY";
			Vector3[] vertices = new Vector3[]
			{
				new Vector3( 0.5f,  0.0f, 0.5f),
				new Vector3(-0.5f,  0.0f, -0.5f),
				new Vector3(-0.5f,  0.0f, 0.5f),
				new Vector3( 0.5f,  0.0f, -0.5f)
			};
			int[] triangles = new int[]
			{
				0, 1, 2,
				3, 1, 0
			};
			Vector2[] uv = new Vector2[]
			{
				new Vector2(1.0f, 1.0f),
				new Vector2(0.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 0.0f)
			};
			m.vertices = vertices;
			m.triangles = triangles;
			m.uv = uv;
			m.RecalculateNormals();
			if (!Directory.Exists("Assets/Mesh"))
				Directory.CreateDirectory("Assets/Mesh");
			AssetDatabase.CreateAsset(m, "Assets/Mesh/SimplePlaneY.asset");
			AssetDatabase.SaveAssets();
		}
		meshFilter.sharedMesh = m;
		m.RecalculateBounds();
	}

}