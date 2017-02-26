using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TestMapGenerator : MonoBehaviour {
	public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
	[SerializeField] GameObject tileViewPrefab;
	[SerializeField] GameObject tileSelectionIndicatorPrefab;
	int maxheight = 50;
	int size = 50;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				RaycastHit hit;
				float height =0;
				if (Physics.Raycast (new Vector3 (.5f+i-size/2, maxheight, .5f+j-size/2), Vector3.down, out hit)) {
					
					height = (maxheight - hit.distance-1);
					//Debug.DrawLine (new Vector3 (i - size / 2, maxheight, j - size / 2), new Vector3 (i - size / 2, height, j - size / 2));
					if (height > 0 && i-size/2==3 && j-size/2==3) {
						Debug.DrawLine (new Vector3 (.5f+i - size / 2, maxheight, .5f+j - size / 2), new Vector3 (3.5f, height, 3.5f));
						Debug.Log (height);
					}
					Tile t = Create();

					Point p = new Point (i-size/2, j-size/2);
					t.Load(p, height);
					tiles.Add(p, t);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	Tile GetOrCreate (Point p)
	{
		if (tiles.ContainsKey(p))
			return tiles[p];

		Tile t = Create();
		t.Load(p, 0);
		tiles.Add(p, t);

		return t;
	}
	Tile Create ()
	{
		GameObject instance = Instantiate(tileViewPrefab) as GameObject;
		instance.transform.parent = transform;
		return instance.GetComponent<Tile>();
	}
}
