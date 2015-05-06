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
	GameObject[,] mazeField;
	GameObject player;
	Rigidbody playerRB;
	Transform cam;
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
		cam = Camera.main.transform;
	}

	// Update is called once per frame
	void Update () {
		if(player!=null){
			cam.position=player.transform.position+Vector3.up*5;
			cam.rotation = Quaternion.LookRotation(Vector3.down);

			UpdateRoute(player.transform.position);

			float x = Input.GetAxis("Horizontal");
			float z = Input.GetAxis("Vertical");
			playerRB.AddForce(new Vector3(x,0,z)*20f*Time.deltaTime,ForceMode.Force);
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
				StartCoroutine(CreateMaze());
			}
		}
		else{
			StopCoroutine(CreateMaze());
			foreach ( Transform n in mazeParent.GetComponentsInChildren<Transform>() )
			{
				if(n!=mazeParent)
					GameObject.Destroy(n.gameObject);
			}
			if(player!=null) GameObject.Destroy(player);
			maze = null;
		}
	}
		

	IEnumerator	 CreateMaze(){
		maze = new Maze(width,height);
		bool[,] data = maze.wallData;
		bool[,] route = maze.routeData;
		int[,] length = maze.routeLengthData;
		int H = data.GetLength(0);
		int W = data.GetLength(1);
		mazeField = new GameObject[H,W];

		int count=0;
		for(int i=0; i<H; i++){
			for(int j=0; j<W; j++){
				GameObject obj = data[i,j] ?
					(GameObject)Instantiate(wallBlock, new Vector3(i,0.5f,j), Quaternion.identity)	:
					(GameObject)Instantiate(floorBlock, new Vector3(i,0,j), Quaternion.identity);
				obj.transform.SetParent(mazeParent);
				mazeField[i,j] = obj;
				if(!data[i,j]){
					float param = length[i,j]/(float)maze.routeLength;
					obj.GetComponent<Renderer>().material.color = new Color(1-Mathf.Abs(param-0.5f)*2,1-param,param,1f);
				}

//				if(data[i,j])
//					((GameObject)Instantiate(wallBlock, new Vector3(i,0.5f,j), Quaternion.identity)).transform.SetParent(mazeParent);
//				GameObject obj = (GameObject)Instantiate(floorBlock, new Vector3(i,0,j), Quaternion.identity);
//				obj.transform.SetParent(mazeParent);
//				if(route[i,j])	obj.GetComponent<Renderer>().material.color = new Color(1f,0f,0f,1f);

				if(++count>invokeStep){count=0;yield return null;}
			}
		}

		player = (GameObject)Instantiate(ball,new Vector3(1,1,1),Quaternion.identity);
		playerRB = player.GetComponent<Rigidbody>();
	}


	void UpdateRoute(Vector3 playerPos){
		Maze.Point start = new Maze.Point((int)Math.Round(playerPos.x),(int)Math.Round(playerPos.z));
		Maze.Point goal = new Maze.Point(maze.height,maze.width);
		if(maze.Search(start,null)){
			bool[,] data = maze.wallData;
			bool[,] route = maze.routeData;
			int[,] length = maze.routeLengthData;
			for(int i=0; i<route.GetLength(0); i++){
				for(int j=0; j<route.GetLength(1); j++){
					if(!data[i,j]){
						float param = length[i,j]/(float)maze.routeLength;
						mazeField[i,j].GetComponent<Renderer>().material.color = new Color(1-Mathf.Abs(param-0.5f)*2,1-param,param,1f);
					}
					//else mazeField[i,j].GetComponent<Renderer>().material.color = new Color(1f,1f,1f,1f);
				}
			}
		}
	}

}
