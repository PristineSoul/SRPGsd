using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TestMapGenerator : MonoBehaviour {
	public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
	[SerializeField] GameObject tileViewPrefab;
	[SerializeField] GameObject tileSelectionIndicatorPrefab;
	float movLooper = 0.15f;
	[SerializeField] Point pos;

	[SerializeField] LevelData levelData;
	Transform marker
	{
		get
		{
			if (_marker == null)
			{
				GameObject instance = Instantiate(tileSelectionIndicatorPrefab) as GameObject;
				_marker = instance.transform;
			}
			return _marker;
		}
	}
	Transform _marker;




	void FixedUpdate (){
		MoveMarker ();
	}

	void MoveMarker (){
		if (Input.GetAxisRaw ("Horizontal") != 0 || (Input.GetAxis ("Vertical") != 0)) {
			if (movLooper < .15f) {
				movLooper += Time.deltaTime;
			} else if (movLooper >= .15f) {
				{		
					pos.y += Mathf.RoundToInt (Input.GetAxisRaw ("Horizontal"));
					pos.x += Mathf.RoundToInt (Input.GetAxisRaw ("Vertical"));
					UpdateMarker ();
					movLooper = 0;
				}
			}
		}
	}
	int maxheight = 50;
	int size = 50;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				RaycastHit hit;
				float height =0;
				if (Physics.Raycast (new Vector3 (i - size / 2, maxheight, j - size / 2), Vector3.down, out hit)) {
					height = (maxheight - hit.distance);
					//Debug.DrawLine (new Vector3 (i - size / 2, maxheight, j - size / 2), new Vector3 (i - size / 2, height, j - size / 2));
					if (height > 0 && i - size / 2 == 3 && j - size / 2 == 3) {
						Debug.DrawLine (new Vector3 (.5f + i - size / 2, maxheight, .5f + j - size / 2), new Vector3 (3.5f, height, 3.5f));
					
					}
					Tile t = Create ();

					for (int k = 0; k < (int)Tile.TerrainTypes.DirectMax; k++) {
				//	for (int k = 0; k < Tile.TerrainTypes; k++) {
						//		if (Enum.IsDefined(typeof(Tile.TerrainTypes), hit.collider.gameObject.tag))
						if (hit.collider.gameObject.CompareTag (((Tile.TerrainTypes)k).ToString ()))
							t.myType = (Tile.TerrainTypes)k;
					//	Debug.Log(((Tile.TerrainTypes)k).ToString());
					}
					if (hit.collider.gameObject.CompareTag ("Brush")) { //REMEMBER TO CHANGE THIS WHEN YOU HAVE MORE UNDER X TILES
						Point p = new Point (i - size / 2, j - size / 2);

						Physics.Raycast (new Vector3 (i - size / 2, height-Mathf.Epsilon, j - size / 2), Vector3.down, out hit);
						height = (height - hit.distance);
						t.myType = Tile.TerrainTypes.UnderBrush;
						t.Load (p, height);
						tiles.Add (p, t);

					} else {
						Point p = new Point (i - size / 2, j - size / 2);
						t.Load (p, height);
						tiles.Add (p, t);
					}
				}
			}
		}
		Save ();
		Slope ();
		Aspect ();
		Hillshade (45,90);
	}
	public void UpdateMarker ()
	{
		Tile t = tiles.ContainsKey(pos) ? tiles[pos] : null;
		marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0, pos.y);
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

	void CreateSaveDirectory ()
	{
		string filePath = Application.dataPath + "/Resources";
		if (!Directory.Exists(filePath))
			AssetDatabase.CreateFolder("Assets", "Resources");
		filePath += "/Levels";
		if (!Directory.Exists(filePath))
			AssetDatabase.CreateFolder("Assets/Resources", "Levels");
		AssetDatabase.Refresh();
	}
	public void Clear ()
	{
		for (int i = transform.childCount - 1; i >= 0; --i)
			DestroyImmediate(transform.GetChild(i).gameObject);
		tiles.Clear();
	}

	public void Save ()
	{
		string filePath = Application.dataPath + "/Resources/Levels";
		if (!Directory.Exists(filePath))
			CreateSaveDirectory ();

		LevelData board = ScriptableObject.CreateInstance<LevelData>();
		board.tiles = new List<Vector3>( tiles.Count );
		foreach (Tile t in tiles.Values)
			board.tiles.Add( new Vector3(t.pos.x, t.height, t.pos.y) );

		string fileName = string.Format("Assets/Resources/Levels/{1}.asset", filePath, name);
		AssetDatabase.CreateAsset(board, fileName);

	}

	public void Load ()
	{
		Clear();
		if (levelData == null)
			return;

		foreach (Vector3 v in levelData.tiles)
		{
			Tile t = Create();
			t.Load(v);
			tiles.Add(t.pos, t);
		}
	}

	 void Slope (){
		for (int i = 0; i < tiles.Count; i++) {
			float maxSlope = 0;
			Point localPos = tiles.Keys.ElementAt (i);
			Tile localTile = tiles.ContainsKey (localPos) ? tiles [localPos] : null;
			if ((int)localTile.myType >= (int)Tile.TerrainTypes.SlopeMax)
				localTile.slope = 0;
			else {
				for (int j = 1; j < 10; j++) {
					Point punto = CheckPointZ (localPos, j, true);
					Tile targetTile = tiles.ContainsKey (punto) ? tiles [punto] : null;
					if (targetTile != null && (int)targetTile.myType < (int)Tile.TerrainTypes.SlopeMax) {
						float slope = Mathf.Rad2Deg * (Mathf.Atan (Mathf.Abs ((localTile.height - targetTile.height) / 1)));
						if (slope > maxSlope)
							maxSlope = slope;
						}
				}
				localTile.slope = maxSlope;
			}
		}
	}

	 void Aspect (){
		for (int i = 0; i < tiles.Count; i++) {
			
			Point localPos = tiles.Keys.ElementAt (i);
			Tile localTile = tiles.ContainsKey (localPos) ? tiles [localPos] : null;
			float[] myZ = new float[9];
			for (int j = 1; j < 10; j++) {
				Point punto = CheckPointZ (localPos, j, true);
				Tile targetTile = tiles.ContainsKey (punto) ? tiles [punto] : null;
				myZ [j - 1] = 0;
				if (targetTile != null)
					myZ [j - 1] = targetTile.height;
				else {
					punto = CheckPointZ (localPos, j, false);
					targetTile = tiles.ContainsKey (punto) ? tiles [punto] : null;
					if (targetTile != null)
						myZ[j-1] = localTile.height + (localTile.height - targetTile.height);
					else
						myZ[j-1] = localTile.height;
				}
			}
		    float p = (myZ[3-1]+2*myZ[6-1]+myZ[9-1]-myZ[1-1]-2*myZ[4-1]-myZ[7-1])/8;
			float q = (myZ [1 - 1] + 2 * myZ [2 - 1] + myZ [3 - 1] - myZ [7 - 1] - 2 * myZ [8 - 1] - myZ [9 - 1]) / 8;
			int aspect = (int)Mathf.Round (180 - Mathf.Rad2Deg * Mathf.Atan (q / p) + 90 * (p / Mathf.Abs (p)));
			if (aspect < 0 || aspect > 360) {
			aspect = 0;}
			localTile.aspect = aspect;
		}
	}
	void Hillshade(float elevHorizon, float sunAzimuth){
		int maxHS = 0;
		for (int i = 0; i < tiles.Count; i++) {

			Point localPos = tiles.Keys.ElementAt (i);
			Tile localTile = tiles.ContainsKey (localPos) ? tiles [localPos] : null;
			float first = ((100 * Mathf.Tan (Mathf.Deg2Rad * localTile.slope) / (Mathf.Sqrt (1 + Mathf.Pow(Mathf.Tan (Mathf.Deg2Rad *(localTile.slope)) , 2)))));
			int hillshade = Mathf.RoundToInt(first*((Mathf.Sin(Mathf.Deg2Rad*elevHorizon)/Mathf.Tan(Mathf.Deg2Rad*elevHorizon))-Mathf.Cos(Mathf.Deg2Rad*elevHorizon)*Mathf.Sin(Mathf.Deg2Rad*(sunAzimuth-localTile.aspect))));
			localTile.hillshade = hillshade;
			if (hillshade > maxHS)
				maxHS = hillshade;
		}

	
	}

	Point CheckPointZ (Point currentTile, int Zval,bool direct){
		Point punto = new Point (currentTile.x, currentTile.y);
		if (!direct)
			Zval = 10 - Zval;
		switch (Zval) {
		case 1: punto  = new Point (currentTile.x - 1, currentTile.y + 1);
			break;
		case 2: punto  = new Point (currentTile.x , currentTile.y + 1);
			break;
		case 3: punto  = new Point (currentTile.x + 1, currentTile.y + 1);
			break;
		case 4: punto = new Point (currentTile.x - 1, currentTile.y );
			break;
		case 5: punto  = new Point (currentTile.x , currentTile.y );
			break;
		case 6: punto = new Point (currentTile.x + 1, currentTile.y );
			break;
		case 7: punto = new Point (currentTile.x - 1, currentTile.y - 1);
			break;
		case 8: punto  = new Point (currentTile.x , currentTile.y - 1);
			break;
		case 9: punto = new Point (currentTile.x + 1, currentTile.y - 1);
			break;
		}
		return punto;
			}

}
