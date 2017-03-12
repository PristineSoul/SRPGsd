using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour 
{
	public enum TerrainTypes{
		Grass,
		Rock,
		Lava,
		SlopeMax, //below this line are the items that are not part of the terrain -> don't affect slope, aspect and hillshade
		Tree,
		DirectMax, //below this are the items you can stand below, use one more raycast
		UnderBrush
	}

	#region Const
//	public const float stepHeight = 0.15f;
	#endregion

	#region Fields / Properties
	public TerrainTypes myType;
	public Texture myTexture;
	public Point pos;
	public float height;
	public float slope;
	public int aspect;
	public int hillshade;
	public Vector3 center { get { return new Vector3(pos.x, height, pos.y); }}
	public GameObject content;
	[HideInInspector] public Tile prev;
	[HideInInspector] public int distance;
	#endregion

	#region Public


	void Update (){
		//if (hillshade != 0) 
		{
			float col = (float)hillshade / 50;
			GetComponentInChildren<SpriteRenderer> ().color = new Color (col,col,col);
		}
	}



	public void Grow ()
	{
		height++;
		Match();
	}


	
	public void Shrink ()
	{
		height--;
		Match ();
	}

	public void Load (Point p, float h)
	{
		pos = p;
		height = h;
		Match();
	}

	public void SetTileTexture (Texture t){
		GetComponent<Renderer>().material.mainTexture = t;
	}

	public int ReturnCost (GameObject Walker){
		return 1; //need to change based on who walks and the type properties
	}

	
	public void Load (Vector3 v)
	{
		Load (new Point((int)v.x, (int)v.z), (int)v.y);
	}
	#endregion

	#region Private
 void Match ()
	{
		GameObject holder = GameObject.Find ("Holder");
		if (holder) {
			string str = myType.ToString ();
			if (str == "Tree")
				this.gameObject.GetComponent<MeshRenderer> ().material = holder.GetComponent<HolderTest> ().Tree;
			else if (str == "UnderBrush")
				this.gameObject.GetComponent<MeshRenderer> ().material = holder.GetComponent<HolderTest> ().Brush;
			else if (str == "Lava")
				this.gameObject.GetComponent<MeshRenderer> ().material = holder.GetComponent<HolderTest> ().Lava;
			else
				this.gameObject.GetComponent<MeshRenderer> ().material = holder.GetComponent<HolderTest> ().Grass;
		}
		transform.localPosition = new Vector3 (pos.x, height / 2f, pos.y);
		transform.localScale = new Vector3 (1, height, 1);

	}
	#endregion
}
