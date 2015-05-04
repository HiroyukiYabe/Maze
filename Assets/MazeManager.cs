//Attach to MazeManager

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour {

	[SerializeField]
	int height;
	[SerializeField]
	int width;

	public int invokeStep = 1000;

	Maze maze;
	public GameObject wallBlock;
	public GameObject floorBlock;
	public Transform mazeParent;

	public InputField sizeInput;
	//public InputField widthInput;
	public Button generateButton;

	// Use this for initialization
	void Awake () {
		sizeInput.onEndEdit.AddListener(OnSizeChange);
		//widthInput.onEndEdit.AddListener(OnWidthChange);
		generateButton.onClick.AddListener(OnGenerateClick);
	}


	// Update is called once per frame
	void Update () {}

	void OnSizeChange(string s){
		string[] sary = s.Split(new char[]{',',' ','\t'});
		if(sary.Length<2) return;
		int.TryParse(sary[0],out height);
		int.TryParse(sary[1],out width);
	}

//	void OnHeightChange(string e){
//		int.TryParse(e,out height);
//	}
//
//	void OnWidthChange(string e){
//		int.TryParse(e,out width);
//	}

	void OnGenerateClick(){
		if(maze==null){
			if(height%2==1&&width%2==1){
				Debug.Log("generate maze");
				maze = new Maze(width,height);
				StartCoroutine(SolveMaze(maze));
			}
			Camera.main.transform.position=new Vector3((height+1)/2,Mathf.Max(height,width)*1.5f,(width+1)/2);
			Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.down);
		}
		else{
			StopCoroutine(SolveMaze(maze));
			foreach ( Transform n in mazeParent.GetComponentsInChildren<Transform>() )
			{
				if(n!=mazeParent)
					GameObject.Destroy(n.gameObject);
			}
			maze = null;
		}
	}

	IEnumerator	 CreateMaze(Maze maze){
		while(!maze.isGenerated){yield return null;}
		bool[,] data = maze.mazeData;
		int count =0;

		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				if(data[i,j])
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
				((GameObject)Instantiate(floorBlock, new Vector3(i,-1,j), Quaternion.identity)).transform.SetParent(mazeParent);
			
				if(++count>invokeStep){count=0;yield return null;}
			}
		}
	}

	IEnumerator	 SolveMaze(Maze maze){
		while(!maze.isSolved){yield return null;}
		bool[,] data = maze.mazeData;
		bool[,] route = maze.routeData;
		int count=0;

		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				if(data[i,j])
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
				GameObject obj = (GameObject)Instantiate(floorBlock, new Vector3(i,-1,j), Quaternion.identity);
				obj.transform.SetParent(mazeParent);
				if(route[i,j])	obj.GetComponent<Renderer>().material.color = new Color(1f,0f,0f,1f);

				if(++count>invokeStep){count=0;yield return null;}
			}
		}
	}

}
