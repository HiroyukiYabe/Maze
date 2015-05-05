//Attach to MazeManager

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour {

	int height;
	int width;

	public int invokeStep = 1000;

	Maze maze;
	GameObject player;
	[Header("Gameobject")]
	public GameObject wallBlock;
	public GameObject floorBlock;
	public GameObject ball;
	public Transform mazeParent;

	[Header("UI")]
	public InputField sizeInput;
	public Button generateButton;


	// Use this for initialization
	void Awake () {
		sizeInput.onEndEdit.AddListener(OnSizeChange);
		generateButton.onClick.AddListener(OnGenerateClick);
	}

	// Update is called once per frame
	void Update () {
		if(player!=null){
			Camera.main.transform.position=player.transform.position+Vector3.up*5;
			Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.down);

			float x = Input.GetAxis("Horizontal");
			float z = Input.GetAxis("Vertical");
			player.GetComponent<Rigidbody>().AddForce(new Vector3(x,0,z),ForceMode.Force);
		}
	}

	void OnSizeChange(string s){
		string[] sary = s.Split(new char[]{',',' ','\t'});
		if(sary.Length<2) return;
		int.TryParse(sary[0],out height);
		int.TryParse(sary[1],out width);
	}
		
	void OnGenerateClick(){
		if(maze==null){
			if(height%2==1&&width%2==1){
				Debug.Log("generate maze");
				maze = new Maze(width,height);
				StartCoroutine(SolveMaze(maze));
//				Camera.main.transform.position=new Vector3((height+1)/2,Mathf.Max(height,width)*1.3f,(width+1)/2);
//				Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.down);
			}
		}
//		else{
//			StopCoroutine(SolveMaze(maze));
//			foreach ( Transform n in mazeParent.GetComponentsInChildren<Transform>() )
//			{
//				if(n!=mazeParent)
//					GameObject.Destroy(n.gameObject);
//			}
//			maze = null;
//		}
	}

	IEnumerator	 CreateMaze(Maze maze){
		while(!maze.isGenerated){yield return null;}
		bool[,] data = maze.mazeData;
		int count =0;

		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				if(data[i,j])
					((GameObject)Instantiate(wallBlock, new Vector3(i,0.5f,j), Quaternion.identity)).transform.SetParent(mazeParent);
				((GameObject)Instantiate(floorBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
			
				if(++count>invokeStep){count=0;yield return null;}
			}
		}
		player = (GameObject)Instantiate(ball,new Vector3(1,1,1),Quaternion.identity);
	}

	IEnumerator	 SolveMaze(Maze maze){
		while(!maze.isGenerated){yield return null;}
		Debug.Log("Solve");
		maze.Solve();
		while(!maze.isSolved){yield return null;}
		bool[,] data = maze.mazeData;
		bool[,] route = maze.routeData;
		int count=0;

		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				if(data[i,j])
					((GameObject)Instantiate(wallBlock, new Vector3(i,0.5f,j), Quaternion.identity)).transform.SetParent(mazeParent);
				GameObject obj = (GameObject)Instantiate(floorBlock, new Vector3(i,0,j), Quaternion.identity);
				obj.transform.SetParent(mazeParent);
				if(route[i,j])	obj.GetComponent<Renderer>().material.color = new Color(1f,0f,0f,1f);

				if(++count>invokeStep){count=0;yield return null;}
			}
		}
		yield return null;
		player = (GameObject)Instantiate(ball,new Vector3(1,1,1),Quaternion.identity);
	}

}
