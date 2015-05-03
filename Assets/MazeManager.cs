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

	public InputField heightInput;
	public InputField widthInput;
	public Button generateButton;

	// Use this for initialization
	void Awake () {
		heightInput.onEndEdit.AddListener(OnHeightChange);
		widthInput.onEndEdit.AddListener(OnWidthChange);
		generateButton.onClick.AddListener(OnGenerateClick);
	}


	// Update is called once per frame
	void Update () {}

	void OnHeightChange(string e){
		int.TryParse(e,out height);
	}

	void OnWidthChange(string e){
		int.TryParse(e,out width);
	}

	void OnGenerateClick(){
		if(height%2!=0&&width%2!=0){
			Debug.Log("generate maze");
			maze = new Maze(width,height);
			StartCoroutine(CreateMaze(maze));
		}
	}

	IEnumerator	 CreateMaze(Maze maze){
		while(!maze.isGenerated){yield return null;}
		bool[,] data = maze.mazeData;
		int mazeHeight = data.GetLength(0);
		int mazeWidth = data.GetLength(1);

		int count =0;
		for(int i=-1; i<mazeHeight+1; i++){
			for(int j=-1; j<mazeWidth+1; j++){
				((GameObject)Instantiate(floorBlock, new Vector3(i,-1,j), Quaternion.identity)).transform.SetParent(mazeParent);
				if(i==-1||i==mazeHeight||j==-1||j==mazeWidth){
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
				}
				if(++count>invokeStep){count=0;yield return null;}
			}
		}

		for(int i=0; i<mazeHeight; i++){
			for(int j=0; j<mazeWidth; j++){
				if(data[i,j]){
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
					if(++count>invokeStep){count=0;yield return null;}
				}
			}
		}
	}

}
