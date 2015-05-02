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
			maze.Create();
			StartCoroutine(CreateMaze(maze.mazeData));
		}
	}

	IEnumerator	 CreateMaze(bool[,] data){
		int height = data.GetLength(0);
		int width = data.GetLength(1);

		for(int i=-1; i<height+1; i++){
			for(int j=-1; j<width+1; j++){
				((GameObject)Instantiate(floorBlock, new Vector3(i,-1,j), Quaternion.identity)).transform.SetParent(mazeParent);
				yield return null;
				if(i==-1||i==height||j==-1||j==width){
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
					yield return null;
				}
			}
		}

		for(int i=0; i<height; i++){
			for(int j=0; j<width; j++){
				if(data[i,j]){
					((GameObject)Instantiate(wallBlock, new Vector3(i,0,j), Quaternion.identity)).transform.SetParent(mazeParent);
					yield return null;
				}
			}
		}
	}

}
