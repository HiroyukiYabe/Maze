#define MY_DEBUG

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


public class Maze{


	class MazeGenerator{

		int roomCol,roomRaw;

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
		List<Wall> uncheckedWall;

		public bool[,] generatedData{get; private set;}

		/// <summary>
		/// height,width ともに奇数でなければならない
		/// </summary>
		public	MazeGenerator(int height, int width){
			if(width%2!=1 || height%2!=1){
				Debug.LogError("Height and width must be odd number!");
			}
			generatedData = new bool[height,width];

			roomCol=(height+1)/2;
			roomRaw=(width+1)/2;
			room = new int[roomCol,roomRaw];
			for(int h=0; h<roomCol; h++){
				for(int w=0; w<roomRaw; w++)
					room[h,w]=h*roomRaw+w;
			}

			uncheckedWall = new List<Wall>();
			VerticalWall = new WallState[roomCol,roomRaw-1];
			for(int h=0; h<roomCol; h++){
				for(int w=0; w<roomRaw-1; w++)
					uncheckedWall.Add(new Wall(Wall.Type.VERTICAL,h,w));
			}
			HorizontalWall = new WallState[roomCol-1,roomRaw];
			for(int h=0; h<roomCol-1; h++){
				for(int w=0; w<roomRaw; w++)
					uncheckedWall.Add(new Wall(Wall.Type.HOLIZONTAL,h,w));
			}

			//シャッフルする
			uncheckedWall = uncheckedWall.OrderBy(i => Guid.NewGuid()).ToList();

			ClusterMethod();
			GetMazeData();
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
		void GetMazeData(){
			#if MY_DEBUG
			foreach (var cluster in room) {
				if (cluster!=room[0,0])
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
			#endif
			for(int i=0; i<generatedData.GetLength(0); i++){
				for(int j=0; j<generatedData.GetLength(1); j++){
					if(i%2==0 && j%2==0){ 
						generatedData[i,j]=false;
					}else if(i%2==1 && j%2==1){
						generatedData[i,j]=true;
					}else if(i%2==0 && j%2==1){
						generatedData[i,j]=VerticalWall[i/2,(j-1)/2]==WallState.STAND;
					}else if(i%2==1 && j%2==0){
						generatedData[i,j]=HorizontalWall[(i-1)/2,j/2]==WallState.STAND;
					}
				}
			}

		}

	}

			
	class MazeSolver{
		public bool[,] solvedData{get; private set;}
		public readonly Point start;
		public readonly Point goal;
		readonly bool[,] mazeData;


		public MazeSolver(Point start, Point goal, bool[,] mazeData){
			this.start=start;
			this.goal=goal;
			this.mazeData=mazeData;
		}

		//A*法
		void AstarMethod(){
		}




		/*class AStar{
			readonly bool[,] mazeData;				// 迷路を格納した配列
			List<Node> openNodes;					// 未確定のノード一覧
			Point goal;                 // ゴールの場所
			private var dx:Array = [0, 1, 0, -1];   // X方向移動用配列
			private var dy:Array = [1, 0, -1, 0];   // Y方向移動用配列
			readonly int H;                      // 迷路の縦幅
			readonly int W;                      // 迷路の横幅

			// コンストラクタ
			public AStar(Point start, Point goal, bool[,] mazeData) {

				// 各マスを初期化する
				var start:Node = null;
				maze = [];
				for (var yy:int = 0; yy < H; yy++) {
					maze[yy] = [];
					for (var xx:int = 0; xx < W; xx++) {
						var block:Node = new Node(mazeArray[yy].charAt(xx), xx, yy);
						addChild(block);
						maze[yy][xx] = block;

						// スタート地点を覚えておく
						if (block.isStart) {
							start = block;
						}

						// ゴール地点の場所を記録する
						if (block.isGoal) {
							goal = new Point(block.xx, block.yy);
						}
					}
				}

				// スタート地点のみ gs を 0 とし、open に加える
				if (start == null) return;
				start.gs = 0;
				start.fs = start.gs + hs(start);
				open.push(start);

				// nextStep の定期呼び出しを開始する
				setTimeout(nextStep, 100);
			}

			// ダイクストラ法の１ステップを実行する
			void nextStep(){
				// 未確定ノードの中から、スコアが最小となるノード u を決定する
				var minScore:int = int.MAX_VALUE;
				var minIndex:int = -1;
				var u:Node = null;
				for (var i:int = 0; i < open.length; i++) {
					var block:Node = open[i] as Node;
					if (block.done) continue;
					if (block.fs < minScore) {
						minScore = block.fs;
						minIndex = i;
						u = block;
					}
				}

				// 未確定ノードがなかった場合は終了
				if (u == null) {
					return;
				}

				// ノード u を確定ノードとする
				open.splice(minIndex, 1);
				u.done = true;
				u.draw();

				// ゴールだった場合は終了
				if (u.isGoal) {
					return;
				}

				// ノード u の周りのノードのスコアを更新する
				for (i = 0; i < dx.length; i++) {
					// 境界チェック
					if (u.yy + dy[i] < 0 || u.yy + dy[i] >= H || u.xx + dx[i] < 0 || u.xx + dx[i] >= W) continue;

					// ノード v を取得する
					var v:Node = maze[u.yy + dy[i]][u.xx + dx[i]] as Node;

					// 確定ノードや壁だったときにはパスする
					if (v.done || v.isWall) continue;

					// 既存のスコアより小さいときのみ更新する
					if (u.gs + 1 + hs(v) < v.fs) {
						v.gs = u.gs + 1;
						v.fs = v.gs + hs(v);
						v.prev = u;
						v.draw();

						// open リストに追加
						if (open.indexOf(v) == -1) open.push(v);
					}
				}

				setTimeout(nextStep, 100);
			}

			// h* を計算する
			private function hs(node:Node):Number {
				return Math.abs(node.xx - goal.x) + Math.abs(node.yy - goal.y);
			}


		}
	}

	class Node{
		public var fs:Number;           // ノードの f* の値
		public var gs:Number;           // ノードの g* の値
		public var done:Boolean;        // ダイクストラ法の確定ノード一覧
		public var prev:Node;          // ダイクストラ法の直前の頂点を記録
		public var isWall:Boolean;      // 壁かどうか
		public var isGoal:Boolean;      // ゴール地点かどうか
		public var isStart:Boolean;     // スタート地点かどうか
		public var isRoute:Boolean;     // スタートからゴールへのルート上の点かどうか
		public var xx:int;              // マスの x 方向インデックス
		public var yy:int;              // マスの y 方向インデックス


		// 描画する
		public function draw():void {
			graphics.clear();

			// 確定したノードはスコアに応じた色にする
			graphics.beginFill(isWall ? WALL : 
				done ? new ColorHSV(fs * 10, .5).value : NORMAL);
			graphics.drawRect(-SIZE / 2, - SIZE / 2, SIZE, SIZE);
			graphics.endFill();

			// prev ノードが存在する場合は矢印を描画する
			if (prev) {
				graphics.lineStyle(0, isRoute ? 0x000000 : new ColorHSV(fs * 10, 1, .8).value);
				graphics.moveTo(SIZE * .4, 0);
				graphics.lineTo(-SIZE * .4, 0);
				graphics.lineTo(-SIZE * .2, SIZE * .1);
				graphics.lineTo(-SIZE * .4, 0);
				graphics.lineTo(-SIZE * .2, -SIZE * .1);
				if (prev.xx < xx) rotation = 0;
				if (prev.xx > xx) rotation = 180;
				if (prev.yy < yy) rotation = 90;
				if (prev.yy > yy) rotation = 270;
			}

			// ゴールが確定したときには、手前のノードを全て辿って
			// isRoute を true にする
			if (isGoal && done) {
				var b:Node = prev;
				while (b) {
					b.isRoute = true;
					b.draw();
					b = b.prev;
				}
			}
		}
	}*/





	}


	public bool[,] mazeData{get; private set;}
	public bool[,] solvedData{get; private set;}
	public readonly int height;
	public readonly int width;
	public bool isGenerated{get; private set;}
	public bool isSolved{get; private set;}


	public Maze(int height=11, int width=11){
		this.height=height;
		this.width=width;
		isGenerated = false;
		isSolved = false;

		GenerateMaze();
		//SolveMaze();
	}


	void GenerateMaze(){
		if(width%2!=1 || height%2!=1){
			Debug.LogError("Height and width must be odd number!");
			return;
		}
		Thread thread = new Thread(()=>{
			Timer timer = new Timer(); timer.Start();

			MazeGenerator MG = new MazeGenerator(height,width);
			mazeData = MG.generatedData;
			if(mazeData.GetLength(0)!=height || mazeData.GetLength(1)!=width)
				Debug.LogError("Generate Maze Error!!");

			timer.Stop();
			Debug.Log("Generate Maze:"+timer.sec+"[s]");
			isGenerated=true;
		});
		thread.Start();
	}


	void SolveMaze(Point start, Point goal){
		Thread thread = new Thread(()=>{
			while(!isGenerated){}

			Timer timer = new Timer(); timer.Start();

			MazeSolver MS = new MazeSolver(start,goal,this.mazeData);
			solvedData = MS.solvedData;
			if(solvedData.GetLength(0)!=height || solvedData.GetLength(1)!=width)
				Debug.LogError("Solve Maze Error!!");

			timer.Stop();
			Debug.Log("Solve Maze:"+timer.sec+"[s]");
			isSolved=true;
		});
		thread.Start();
	}


	class Timer{
		System.Diagnostics.Stopwatch stopwatch;
		public float sec{get{return stopwatch.ElapsedMilliseconds/1000f;}}

		public Timer(){stopwatch = new System.Diagnostics.Stopwatch();}
		public void Start(){stopwatch.Start();}
		public void Stop(){stopwatch.Stop();}
	}


	public struct Point{
		public int x{get; private set;}
		public int y{get; private set;}
		public Point(int x, int y){this.x=x;this.y=y;}
	}





//	void OnApplicationQuit(){
//		thread.Abort ();
//		thread.Join ();
//	}

}
