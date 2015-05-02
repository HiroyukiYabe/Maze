using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Maze{

	//迷路生成クラス
	class MazeField{

		int[,] room;	//int = cluster number

		enum WallState{UNCHECKED, STAND, BROKEN}
		WallState[,] VerticalWall;	//true=unbroken, false=broken
		WallState[,] HorizontalWall;	//true=unbroken, false=broken

		struct Wall {
			public enum Type{VERTICAL,HOLIZONTAL}
			public Type type{get;private set;}
			public int colPos{get;private set;}
			public int rawPos{get;private set;}
			public Wall(Type type, int col, int raw){this.type=type;colPos=col;rawPos=raw;}
		}
		List<Wall> uncheckedWall = new List<Wall>();

		/// <summary>
		/// height,width ともに壁の厚さを無視した場合の値であることに注意
		/// </summary>
		public	MazeField(int height, int width){
			room = new int[height,width];
			for(int h=0; h<height; h++){
				for(int w=0; w<width; w++)
					room[h,w]=h*width+w;
			}

			VerticalWall = new WallState[height,width-1];
			for(int h=0; h<height; h++){
				for(int w=0; w<width-1; w++)
					uncheckedWall.Add(new Wall(Wall.Type.VERTICAL,h,w));
			}
			HorizontalWall = new WallState[height-1,width];
			for(int h=0; h<height-1; h++){
				for(int w=0; w<width; w++)
					uncheckedWall.Add(new Wall(Wall.Type.HOLIZONTAL,h,w));
			}

			//シャッフルする
			uncheckedWall = uncheckedWall.OrderBy(i => Guid.NewGuid()).ToList();
		}

		//クラスター法による迷路生成
		void ClusterMethod(){
			while (uncheckedWall.Count>0) {
				Wall tmpwall = uncheckedWall.ElementAt(0);
				uncheckedWall.RemoveAt(0);
				int col = tmpwall.colPos;
				int raw = tmpwall.rawPos;

				switch(tmpwall.type){
				case Wall.Type.VERTICAL:
					if(room[col,raw]==room[col,raw+1]){
						VerticalWall[col,raw] = WallState.STAND;
					}else{
						VerticalWall[col,raw] = WallState.BROKEN;
						int min = Math.Min(room[col,raw],room[col,raw+1]);
						int max = Math.Max(room[col,raw],room[col,raw+1]);
						for(int h=0; h<room.GetLength(0); h++){
							for(int w=0; w<room.GetLength(1); w++)
								if(room[h,w]==max)	room[h,w]=min;
						}
					}
					break;
				case Wall.Type.HOLIZONTAL:
					if(room[col,raw]==room[col+1,raw]){
						HorizontalWall[col,raw] = WallState.STAND;
					}else{
						HorizontalWall[col,raw] = WallState.BROKEN;
						int min = Math.Min(room[col,raw],room[col+1,raw]);
						int max = Math.Max(room[col,raw],room[col+1,raw]);
						for(int h=0; h<room.GetLength(0); h++){
							for(int w=0; w<room.GetLength(1); w++)
								if(room[h,w]==max)	room[h,w]=min;
						}
					}
					break;
				}
			}
		}

		/// <summary>
		/// true=wall, false=path
		/// </summary>
		/// <returns>The maze.</returns>
		public bool[,] CreateMaze(){
			ClusterMethod();

			string s="";
			foreach(var val in room){
				s+=val.ToString()+",";
			}
			Debug.Log(s);
			s="";
			foreach(var val in VerticalWall){
				s+=((int)val).ToString()+",";
			}
			Debug.Log(s);
			s="";
			foreach(var val in HorizontalWall){
				s+=((int)val).ToString()+",";
			}
			Debug.Log(s);

			foreach (var cluster in room) {
				if (cluster!=0)
					Debug.LogError("room error");
			}
			foreach (var state in VerticalWall) {
				if (state==WallState.UNCHECKED)
					Debug.LogError("vertical wall error");
			}
			foreach (var state in HorizontalWall) {
				if (state==WallState.UNCHECKED)
					Debug.LogError("horizontal wall error");
			}

			bool[,] createdMaze = new bool[room.GetLength(0)*2-1,room.GetLength(1)*2-1];
			for(int i=0; i<createdMaze.GetLength(0); i++){
				for(int j=0; j<createdMaze.GetLength(1); j++){
					if(i%2==0 && j%2==0){ 
						createdMaze[i,j]=false;
					}else if(i%2==1 && j%2==1){
						createdMaze[i,j]=true;
					}else if(i%2==0 && j%2==1){
						createdMaze[i,j]=VerticalWall[i/2,(j-1)/2]==WallState.STAND;
					}else if(i%2==1 && j%2==0){
						createdMaze[i,j]=HorizontalWall[(i-1)/2,j/2]==WallState.STAND;
					}
				}
			}

			return createdMaze;
		}

	}
			

	public bool[,] mazeData{get; private set;}
	public int height{get; private set;}
	public int width{get; private set;}

	public Maze(int height=11, int width=11){
		this.height=height;
		this.width=width;
		this.mazeData=new bool[height,width];
	}

	public void Create(){
		if(width%2!=1 || height%2!=1){
			Debug.LogError("Height and width must be odd number!");
			return;
		}

		MazeField field = new MazeField((height+1)/2,(width+1)/2);
		mazeData = field.CreateMaze();
	}



//	public void GenMaze() {
//		var id = 1;
//		var area_arr = new Array();
//		var tmp_wall_arr = new Array();
//		
//		wall_hash['x'] = new Array();
//		wall_hash['y'] = new Array();
//		
//		for (var y = 0; y < y_num; y++) {
//			area_arr[y] = new Array();
//			wall_hash['x'][y] = new Array();
//			wall_hash['y'][y] = new Array();
//			
//			for (var x = 0; x < x_num; x++) {
//				// 壁情報配列初期化
//				if (x > 0) {
//				tmp_wall_arr.push({ 'type':'y', 'x':x, 'y':y });
//				}
//				if (y > 0) {
//				tmp_wall_arr.push({ 'type':'x', 'x':x, 'y':y });
//				}
//				
//				// 領域情報配列初期化
//				area_arr[y][x] = id++;
//			}
//		}
//		
//		// 壁情報配列シャッフル
//		tmp_wall_arr.shuffle();
//		
//		//
//		while (tmp_wall_arr.length) {
//			var wall_obj = tmp_wall_arr.pop();
//			var x = wall_obj.x;
//			var y = wall_obj.y;
//			
//			if (wall_obj.type == 'x') {
//				if (area_arr[y - 1][x] != area_arr[y][x]) {
//					var from = area_arr[y - 1][x];
//					var to = area_arr[y][x];
//					if (area_arr[y - 1][x] < area_arr[y][x]) {
//						from =  area_arr[y][x];
//						to = area_arr[y - 1][x];
//					}
//					
//					for (var y = 0; y < y_num; y++) {
//						for (var x = 0; x < x_num; x++) {
//							if (area_arr[y][x] == from) {
//								area_arr[y][x] = to;
//							}
//						}
//					}
//				}
//				else {
//					wall_arr.push(wall_obj);
//					wall_hash['x'][y][x] = 1;
//				}
//			}
//			else if (wall_obj.type == 'y') {
//				if (area_arr[y][x - 1] != area_arr[y][x]) {
//					var from = area_arr[y][x - 1];
//					var to = area_arr[y][x];
//					if (area_arr[y][x - 1] < area_arr[y][x]) {
//						from =  area_arr[y][x];
//						to = area_arr[y][x - 1];
//					}
//					
//					for (var y = 0; y < y_num; y++) {
//						for (var x = 0; x < x_num; x++) {
//							if (area_arr[y][x] == from) {
//								area_arr[y][x] = to;
//							}
//						}
//					}
//				}
//				else {
//					wall_arr.push(wall_obj);
//					wall_hash['y'][y][x] = 1;
//				}
//			}
//		}
//	};
//	
//	
//	//------------------------------------------------------------------------------
//	// private: 経路探索
//	//------------------------------------------------------------------------------
//	pri.searchRoute = function () {
//		var area_arr = new Array();
//		var tmp_wall_arr = new Array();
//		
//		for (var y = 0; y < y_num; y++) {
//			area_arr[y] = new Array();
//			
//			for (var x = 0; x < x_num; x++) {
//				// 領域情報配列初期化
//				area_arr[y][x] = 1;
//			}
//		}
//		
//		for (var y = 0; y < y_num; y++) {
//			for (var x = 0; x < x_num; x++) {
//				if (x == 0 && y == 0) {
//					continue;
//				}
//				
//				var xx = x;
//				var yy = y;
//				do {
//					var open_wall_side = isClose(xx, yy, area_arr);
//					if (open_wall_side > 0) {
//						area_arr[yy][xx] = 0;
//					}
//					switch (open_wall_side) {
//					case 1: yy--; break;
//					case 2: xx++; break;
//					case 3: yy++; break;
//					case 4: xx--; break;
//					}
//				} while (open_wall_side > 0);
//			}
//		}
//		
//		route_arr = area_arr;
//	};
//	
//	
//	//------------------------------------------------------------------------------
//	// private: 行き止まり判定
//	//------------------------------------------------------------------------------
//	pri.isClose = function (x, y, area_arr) {
//		// 無効なエリアの場合は-1を返す
//		if (area_arr[y][x] == 0) {
//			return -1;
//		}
//		
//		// スタート地点の場合は-1を返す
//		if (y == 0 && x == 0) {
//			return -1;
//		}
//		
//		// ゴール地点の場合は-1を返す
//		if (y == y_num - 1 && x == x_num - 1) {
//			return -1;
//		}
//		
//		arround_wall_num = 0;
//		open_wall_side = 0;
//		
//		// 上
//		if (y == 0 || (y > 0 && wall_hash['x'][y][x] == 1) || area_arr[y - 1][x] == 0) {
//			arround_wall_num++;
//		}
//		else {
//			open_wall_side = 1;
//		}
//		
//		// 下
//		if (y == y_num - 1 || (y < y_num - 1 && wall_hash['x'][y + 1][x] == 1) 
//		    || area_arr[y + 1][x] == 0) {
//			arround_wall_num++;
//		}
//		else {
//			open_wall_side = 3;
//		}
//		
//		// 左
//		if (x == 0 || (x > 0 && wall_hash['y'][y][x] == 1) || area_arr[y][x - 1] == 0) {
//			arround_wall_num++;
//		}
//		else {
//			open_wall_side = 4;
//		}
//		
//		// 右
//		if (x == x_num - 1 || (x < x_num - 1 && wall_hash['y'][y][x + 1] == 1)
//		    || area_arr[y][x + 1] == 0) {
//			arround_wall_num++;
//		}
//		else {
//			open_wall_side = 2;
//		}
//		
//		if (arround_wall_num == 3) {
//			return open_wall_side;
//		}
//		
//		return 0;
//	}
//}}}


}
