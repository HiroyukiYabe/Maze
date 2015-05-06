#define MY_DEBUG

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;


public class Maze{

	//迷路生成用クラス
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
		System.Random rand = new System.Random();

		int height,width;
		/// <summary>
		/// 外周なしの迷路. trueが壁、falseが通路
		/// </summary>
		/// <value>The generated data.</value>
		public bool[,] generatedData{get; private set;}
		/// <summary>
		/// 外周ありの迷路. trueが壁、falseが通路
		/// </summary>
		/// <value>The generated data with wall.</value>
		public bool[,] generatedDataWithWall{get; private set;}

		/// <summary>
		/// height,width ともに奇数でなければならない
		/// </summary>
		public	MazeGenerator(int height, int width){
			if(width%2!=1 || height%2!=1){
				Debug.LogError("Height and width must be odd number!");
			}
			this.height=height;
			this.width=width;

			roomCol=(height+1)/2;
			roomRaw=(width+1)/2;
			room = new int[roomCol,roomRaw];
			for(int h=0; h<roomCol; h++){
				for(int w=0; w<roomRaw; w++)
					room[h,w]=h*roomRaw+w;
			}

			VerticalWall = new WallState[roomCol,roomRaw-1];
			HorizontalWall = new WallState[roomCol-1,roomRaw];

			uncheckedWall = new List<Wall>();
			for(int h=0; h<roomCol; h++){
				for(int w=0; w<roomRaw-1; w++)
					uncheckedWall.Add(new Wall(Wall.Type.VERTICAL,h,w));
			}
			for(int h=0; h<roomCol-1; h++){
				for(int w=0; w<roomRaw; w++)
					uncheckedWall.Add(new Wall(Wall.Type.HOLIZONTAL,h,w));
			}
			//シャッフルする
			uncheckedWall = uncheckedWall.OrderBy(i => Guid.NewGuid()).ToList();

			ClusterMethod();
			GetMazeData();
			ConvertMazeWithWall();
		}

		//クラスター法による迷路生成
		void ClusterMethod(){
			while (uncheckedWall.Count>0) {
				Wall tmpwall = uncheckedWall.ElementAt(0);
				uncheckedWall.RemoveAt(0);
				//壁を壊す確率を操作する
				if(Probability(tmpwall)<rand.NextDouble()){
					uncheckedWall.Add(tmpwall);
					continue;
				}

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

			generatedData = new bool[height,width];
			for(int i=0; i<height; i++){
				for(int j=0; j<width; j++){
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

		void ConvertMazeWithWall(){
			generatedDataWithWall = new bool[height+2,width+2];
			for(int i=0; i<generatedDataWithWall.GetLength(0); i++){
				for(int j=0; j<generatedDataWithWall.GetLength(1); j++){
					if(i==0||i==generatedDataWithWall.GetLength(0)-1||j==0||j==generatedDataWithWall.GetLength(1)-1)
						generatedDataWithWall[i,j]=true;
					else
						generatedDataWithWall[i,j]=generatedData[i-1,j-1];
				}
			}
		}

		void GetMazeWithWall(){
			generatedDataWithWall = new bool[height+2,width+2];
			for(int i=0; i<generatedDataWithWall.GetLength(0); i++){
				for(int j=0; j<generatedDataWithWall.GetLength(1); j++){
					if(i==0||i==generatedDataWithWall.GetLength(0)-1||j==0||j==generatedDataWithWall.GetLength(1)-1)
						generatedDataWithWall[i,j]=true;
					else{
						int k=i-1;int l=j-1;
						if(k%2==0 && l%2==0){ 
							generatedDataWithWall[i,j]=false;
						}else if(k%2==1 && l%2==1){
							generatedDataWithWall[i,j]=true;
						}else if(k%2==0 && l%2==1){
							generatedDataWithWall[i,j]=VerticalWall[k/2,(l-1)/2]==WallState.STAND;
						}else if(k%2==1 && l%2==0){
							generatedDataWithWall[i,j]=HorizontalWall[(k-1)/2,l/2]==WallState.STAND;
						}
					}
				}
			}
		}

		double Probability(Wall wall){
			return 1;
			//return (wall.type==Wall.Type.HOLIZONTAL) ?1.0 : 0.3;
		}

	}
		


	//迷路のルート解析用クラス
	class MazeSolver{
		/// <summary>
		/// 正解ルートを格納する配列. ルートはtrue、他はfalse
		/// </summary>
		/// <value>The route data.</value>
		public bool[,] routeData{get{return _routeData==null ? null : (bool[,])_routeData.Clone();}}
		private bool[,] _routeData;
		/// <summary>
		/// 正解ルートのスタートからの長さを格納する配列
		/// </summary>
		/// <value>The route length data.</value>
		public int[,] routeLengthData{get{return _routeLengthData==null ? null : (int[,])_routeLengthData.Clone();}}
		private int[,] _routeLengthData;
		/// <summary>
		/// 正解ルートの長さ. 失敗時-1
		/// </summary>
		/// <value>The length of the route.</value>
		public int routeLength{get; private set;}


		public MazeSolver(Point start, Point goal, bool[,] mazeData, bool useDijkstra=false){
			ASter aster = useDijkstra ? new Dijkstra(start,goal,mazeData) : new ASter(start,goal,mazeData);
			this._routeData = aster.routeData;
			this._routeLengthData=aster.routeLengthData;
			this.routeLength = aster.routeLenght;
		}

		public static bool SolveMinimum(Point start, Point goal, bool[,] mazeData, 
			out bool[,] routeData, out int[,] routeLengthData, out int routeLength){
			ASter aster = new ASter(start,goal,mazeData);
			if(!aster.isSolved){
				routeData = null;
				routeLengthData = null;
				routeLength = -1;
				return false;
			}
			routeData = aster.routeData;
			routeLengthData=aster.routeLengthData;
			routeLength = aster.routeLenght;
			return true;
		}

		public static bool SearchAll(Point start, Point? goal, bool[,] mazeData, 
			out bool[,] routeData, out int[,] routeLengthData, out int maxLength){
			SearchWholeMaze swm = new SearchWholeMaze(start,goal,mazeData);
			if(swm.routeLengthData==null){
				routeData = null;
				routeLengthData = null;
				maxLength = -1;
				return false;
			}
			routeData = swm.routeData;
			routeLengthData = swm.routeLengthData;
			maxLength = swm.maxLenght;
			return true;
		}
			


		//A*法を計算するクラス
		class ASter{

			protected class Node{
				public int fs;           // ノードの f* の値
				public int gs;           // ノードの g* の値
				public bool done;        // ダイクストラ法の確定ノード一覧
				public Node prev;        // ダイクストラ法の直前の頂点を記録
				public bool isWall;      // 壁かどうか
				public bool isRoute;     // スタートからゴールへのルート上の点かどうか
				public Point pos;        // マスの 位置

				public Node(Point pos,bool isWall){
					this.pos=pos;
					this.isWall=isWall;
					fs=int.MaxValue;
					gs=int.MaxValue;
					prev=null;
					isRoute=false;
				}
			}
			Node[,] mazeNodes;							// 迷路を格納した配列
			List<Node> openNodes = new List<Node>();	// 探索中のノード一覧
			Node goalNode;             				    // ゴールの場所
			Point goalPos;								//ゴールの位置
			readonly int H;                      		// 迷路の縦幅
			readonly int W;                      		// 迷路の横幅
			Point[] nearPoint = {Point.up,Point.down,Point.right,Point.left};

			/// <summary>
			/// 正解ルートを格納する配列. ルートはtrue、他はfalse
			/// </summary>
			/// <value>The route data.</value>
			public bool[,] routeData{get; private set;}
			/// <summary>
			/// 正解ルートのスタートからの長さを格納する配列
			/// </summary>
			/// <value>The route length data.</value>
			public int[,] routeLengthData{get; private set;}
			/// <summary>
			/// 正解ルートの長さ. 失敗時-1
			/// </summary>
			/// <value>The length of the route.</value>
			public int routeLenght{get{return goalNode==null ? -1 : goalNode.gs;}}
			/// <summary>
			/// 探索が成功したか
			/// </summary>
			/// <value><c>true</c> if is solved; otherwise, <c>false</c>.</value>
			public bool isSolved{get{return goalNode!=null;}}


			public ASter(Point start, Point goal, bool[,] mazeData) {

				if(mazeData[start.x,start.y] || mazeData[goal.x,goal.y]){
					Debug.LogError("Start or Goal is a wall!!");
					return;
				}
					
				this.H = mazeData.GetLength(0);
				this.W = mazeData.GetLength(1);
				mazeNodes = new Node[H,W];
				for(int i=0; i<H; i++){
					for (int j=0; j<W; j++) {
						mazeNodes[i,j] = new Node(new Point(i,j),mazeData[i,j]);
					}
				}
				this.goalPos=goal;

				// スタート地点のみ gs を 0 とし、openNodes に加える
				Node startNode = mazeNodes[start.x,start.y];
				startNode.gs = 0;
				startNode.fs = startNode.gs + hs(startNode);
				openNodes.Add(startNode);

				// nextStep の呼び出しを開始する
				#if MY_DEBUG				
				uint count=0;
				#endif
				while(!nextStep()){
					#if MY_DEBUG				
					count++;
					#endif
				}
				#if MY_DEBUG
				Debug.Log("Step count:"+count);
				#endif
				if(goalNode==null){Debug.LogError("Solving maze failed");	return;}

				//結果処理
				this.routeData = new bool[H,W];
				this.routeLengthData = new int[H,W];
				Node tmp = this.goalNode;
				while(tmp!=null){
					tmp.isRoute=true;
					routeData[tmp.pos.x,tmp.pos.y]=true;
					routeLengthData[tmp.pos.x,tmp.pos.y]=tmp.gs;
					tmp=tmp.prev;
				}
//				foreach (var node in mazeNodes) {
//					Point pos = node.pos;
//					this.routeData[pos.x,pos.y]=node.isRoute;
//					this.routeLengthData[pos.x,pos.y]=node.gs;
//				}

			}

			// ダイクストラ法の１ステップを実行する
			bool nextStep(){
				// 未確定ノードの中から、スコアが最小となるノード u を決定する
				int minScore = int.MaxValue;
				int minIndex = -1;
				Node u = null;
				for (int i = 0; i < openNodes.Count; i++) {
					Node block = openNodes[i];
					//未確定ノードの中に確定ノードがある？
					if (block.done){Debug.LogWarning("確定ノード再チェック"); continue;}
					if (block.fs < minScore) {
						minScore = block.fs;
						minIndex = i;
						u = block;
					}
				}

				// 未確定ノードがなかった場合は終了
				if (u == null) {
					return true;
				}

				// ノード u を確定ノードとする
				openNodes.RemoveAt(minIndex);
				u.done = true;

				// ゴールだった場合は終了
				if (u.pos.Equals(goalPos)) {
					goalNode = u;
					return true;
				}

				// ノード u の周りのノードのスコアを更新する
				for(int i=0; i<nearPoint.Length; i++){
					Point next = u.pos + nearPoint[i];
					// 境界チェック
					if (next.x<0 || next.x>=H || next.y<0 || next.y>=W) continue;
					// ノード v を取得する
					Node v = mazeNodes[next.x,next.y];

					// 確定ノードや壁だったときにはパスする
					//確定ノードのスコア更新はないか？
					if (v.done || v.isWall) {
						if(v.done && u.gs + 1 + hs(v) < v.fs)	Debug.LogWarning("確定ノードのスコア更新");
						continue;
					}

					// 既存のスコアより小さいときのみ更新する
					if (u.gs + 1 + hs(v) < v.fs) {
						v.gs = u.gs + 1;
						v.fs = v.gs + hs(v);
						v.prev = u;
						// open リストに追加
						if (!openNodes.Contains(v)) openNodes.Add(v);
					}
				}

				return false;
			}

	
			// h* を計算する ここではGoalとのマンハッタン距離
			protected virtual int hs(Node node){
				return Math.Abs(node.pos.x-goalPos.x)+Math.Abs(node.pos.y-goalPos.y);
			}

		}
			

		//ダイクストラ法を計算するクラス
		class Dijkstra :ASter{
			public Dijkstra(Point start, Point goal, bool[,] mazeData) : base(start, goal, mazeData){}
			//ダイクストラ法ではh*は常に0
			protected override int hs (Node node)
			{
				return 0;
			}
		}
			


		//迷路を全探索するクラス
		class SearchWholeMaze{

			protected class Node{
				//public int fs;           // ノードの f* の値
				public int gs;           // ノードの g* の値
				public bool done;        // ダイクストラ法の確定ノード一覧
				public Node prev;        // ダイクストラ法の直前の頂点を記録
				public bool isWall;      // 壁かどうか
				public bool isRoute;     // スタートからゴールへのルート上の点かどうか
				public Point pos;        // マスの 位置

				public Node(Point pos,bool isWall){
					this.pos=pos;
					this.isWall=isWall;
					//fs=int.MaxValue;
					gs=int.MaxValue;
					prev=null;
					isRoute=false;
				}
			}
			Node[,] mazeNodes;							// 迷路を格納した配列
			List<Node> openNodes = new List<Node>();	// 探索中のノード一覧
			Node goalNode;             				    // ゴールの場所
			Point? goalPos;								//ゴールの位置
			readonly int H;                      		// 迷路の縦幅
			readonly int W;                      		// 迷路の横幅
			Point[] nearPoint = {Point.up,Point.down,Point.right,Point.left};

			/// <summary>
			/// 正解ルートを格納する配列. ルートはtrue、他はfalse
			/// </summary>
			/// <value>The route data.</value>
			public bool[,] routeData{get; private set;}
			/// <summary>
			/// 正解ルートのスタートからの長さを格納する配列
			/// </summary>
			/// <value>The route length data.</value>
			public int[,] routeLengthData{get; private set;}
			/// <summary>
			/// スタートからの最長距離. 
			/// </summary>
			/// <value>The length of the route.</value>
			public int maxLenght{get; private set;}
			/// <summary>
			/// ゴールに到達したか
			/// </summary>
			/// <value><c>true</c> if is solved; otherwise, <c>false</c>.</value>
			public bool isSolved{get{return goalNode!=null;}}


			public SearchWholeMaze(Point start, Point? goal, bool[,] mazeData) {

				if(mazeData[start.x,start.y]){
					Debug.LogError("Start is a wall!!");
					return;
				}

				this.H = mazeData.GetLength(0);
				this.W = mazeData.GetLength(1);
				mazeNodes = new Node[H,W];
				for(int i=0; i<H; i++){
					for (int j=0; j<W; j++) {
						mazeNodes[i,j] = new Node(new Point(i,j),mazeData[i,j]);
					}
				}
				this.goalPos=goal;

				// スタート地点のみ gs を 0 とし、openNodes に加える
				Node startNode = mazeNodes[start.x,start.y];
				startNode.gs = 0;
				//startNode.fs = startNode.gs + hs(startNode);
				openNodes.Add(startNode);

				// nextStep の呼び出しを開始する
				#if MY_DEBUG				
				uint count=0;
				#endif
				while(!nextStep()){
					#if MY_DEBUG				
					count++;
					#endif
				}
				#if MY_DEBUG
				Debug.Log("Step count:"+count);
				#endif
				//if(goalNode==null){Debug.LogError("Solving maze failed");	return;}

				//結果処理
				this.routeData = new bool[H,W];
				this.routeLengthData = new int[H,W];
				Node tmp = this.goalNode;
				while(tmp!=null){
					tmp.isRoute=true;
					//routeData[tmp.pos.x,tmp.pos.y]=true;
					//routeLengthData[tmp.pos.x,tmp.pos.y]=tmp.gs;
					tmp=tmp.prev;
				}
				foreach (var node in mazeNodes) {
					Point pos = node.pos;
					this.routeData[pos.x,pos.y]=node.isRoute;
					this.routeLengthData[pos.x,pos.y]=node.gs;
					if(node.gs!=int.MaxValue && this.maxLenght<node.gs)	this.maxLenght=node.gs;
				}

			}

			// ダイクストラ法の１ステップを実行する
			bool nextStep(){
				// 未確定ノードの中から、スコアが最小となるノード u を決定する
				int minScore = int.MaxValue;
				int minIndex = -1;
				Node u = null;
				for (int i = 0; i < openNodes.Count; i++) {
					Node block = openNodes[i];
					//未確定ノードの中に確定ノードがある？
					if (block.done){Debug.LogWarning("確定ノード再チェック"); continue;}
					if (block.gs < minScore) {
						minScore = block.gs;
						minIndex = i;
						u = block;
					}
				}

				// 未確定ノードがなかった場合は終了
				if (u == null) {
					return true;
				}

				// ノード u を確定ノードとする
				openNodes.RemoveAt(minIndex);
				u.done = true;

				// ゴールだった場合
				if (goalPos!=null && u.pos.Equals(goalPos)) 
					goalNode = u;

				// ノード u の周りのノードのスコアを更新する
				for(int i=0; i<nearPoint.Length; i++){
					Point next = u.pos + nearPoint[i];
					// 境界チェック
					if (next.x<0 || next.x>=H || next.y<0 || next.y>=W) continue;
					// ノード v を取得する
					Node v = mazeNodes[next.x,next.y];

					// 確定ノードや壁だったときにはパスする
					//確定ノードのスコア更新はないか？
					if (v.done || v.isWall) {
						if(v.done && u.gs + 1 < v.gs)	Debug.LogWarning("確定ノードのスコア更新");
						continue;
					}

					// 既存のスコアより小さいときのみ更新する
					if (u.gs + 1 < v.gs) {
						v.gs = u.gs + 1;
						v.prev = u;
						// open リストに追加
						if (!openNodes.Contains(v)) openNodes.Add(v);
					}
				}

				return false;
			}

		}




//		class RouteNode{
//			public int lengthFromStart;
//			public Vector2 nextDir;
//			public bool isCorrentRoute;
//			public RouteNode(){lengthFromStart=-1;nextDir=Vector2.zero;isCorrentRoute=false;}
//			public RouteNode(int lengthFromStart, Vector2 nextdir, bool isCorrectRoute=false){
//				this.lengthFromStart=lengthFromStart;
//				this.nextDir=nextDir;
//				this.isCorrentRoute=isCorrectRoute;
//			}
//		}

	}



	/// <summary>
	/// 生成された迷路(外周付き). trueは壁、falseは通路
	/// </summary>
	/// <value>The maze data.</value>
	public bool[,] wallData{get{return isGenerated ? (bool[,])_wallData.Clone() : null;}}
	private bool[,] _wallData;
	/// <summary>
	/// 正解ルートを格納する配列. ルートはtrue、他はfalse
	/// </summary>
	/// <value>The route data.</value>
	public bool[,] routeData{get{return isSolved ? (bool[,])_routeData.Clone() : null;}}
	private bool[,] _routeData;
	/// <summary>
	/// 正解ルートのスタートからの長さを格納する配列
	/// </summary>
	/// <value>The route length data.</value>
	public int[,] routeLengthData{get{return isSolved ? (int[,])_routeLengthData.Clone() : null;}}
	private int[,] _routeLengthData;
	/// <summary>
	/// 正解ルートの長さ. 失敗時-1
	/// </summary>
	/// <value>The length of the route.</value>
	public int routeLength{get{return _routeLength;}}
	private int _routeLength;
	/// <summary>
	/// 迷路の縦の高さ（外周を除く）
	/// </summary>
	public readonly int height;
	/// <summary>
	/// 迷路の横幅（外周を除く）
	/// </summary>
	public readonly int width;
	/// <summary>
	/// 迷路の生成が完了したか
	/// </summary>
	/// <value><c>true</c> if is generated; otherwise, <c>false</c>.</value>
	public bool isGenerated{get{return _wallData!=null;}}
	/// <summary>
	/// 経路探索が成功したか
	/// </summary>
	/// <value><c>true</c> if is solved; otherwise, <c>false</c>.</value>
	public bool isSolved{get{return _routeData!=null;}}
//	public bool isSolved{get{return _routeData!=null && !isSolving;}}
//	bool isSolving;


	/// <summary>
	/// Mazeのコンストラクタ. 外周を除いた迷路の縦横サイズ（ともに奇数に限定）を引数に取る
	/// </summary>
	/// <param name="height">Height.</param>
	/// <param name="width">Width.</param>
	public Maze(int height, int width){
		this.height=height;
		this.width=width;
		GenerateMaze(height,width);
		SolveMinimum(new Point(1,1), new Point(height,width));
		//SearchAll(new Point(1,1), new Point(height,width));
	}

	/// <summary>
	/// 出力したテキストデータから迷路を再構成する
	/// </summary>
	/// <param name="filePath">File path.</param>
//	public Maze(string filePath){
//		isSolving=false;
//
//		if(!File.Exists(filePath)){
//			Debug.LogError("File not exist!");
//			return;
//		}
//		StreamReader fs = File.OpenText(filePath);
//		if(fs.ReadLine()!="#Maze"){
//			Debug.LogError("File not supported!");
//			return;
//		}
//
//		string s = fs.ReadLine();
//		string[] ary = s.Split(new char[]{',',' ','\t'});
//		this.height = int.Parse(ary[0]);
//		this.width = int.Parse(ary[1]);
//
//
//	}
//		
//	public void Write(string filePath){
//
//	}

	/// <summary>
	/// 迷路を解く. isGenerated=trueでないと動かない
	/// </summary>
	public bool Solve(Point start, Point goal){
		return SolveMinimum(start, goal);
	}
		
	/// <summary>
	/// 迷路のスタートからの距離を全探索する. isGenerated=trueでないと動かない
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	public bool Search(Point start, Point? goal=null){
		return SearchAll(start, goal);
	}


	//迷路生成
	void GenerateMaze(int _height, int _width){
		if(_width%2!=1 || _height%2!=1){
			Debug.LogError("Height and width must be odd number!");
			return;
		}
		#if MY_DEBUG
		Timer timer = new Timer(); timer.Start();
		#endif
		MazeGenerator MG = new MazeGenerator(_height,_width);
		bool[,] tmp =  MG.generatedDataWithWall;
		if(tmp==null || tmp.GetLength(0)!=_height+2 || tmp.GetLength(1)!=_width+2){
			Debug.LogError("Generate Maze Error!!");
			return;
		}
		#if MY_DEBUG
		timer.Stop();
		Debug.Log("Generate Maze:"+timer.sec+"[s]");
		#endif
		_wallData = tmp;
	}

	//迷路生成（マルチスレッド）
//	void GenerateMazeAsync(int _height, int _width){
//		if(_width%2!=1 || _height%2!=1){
//			Debug.LogError("Height and width must be odd number!");
//			return;
//		}
//
//		Thread thread = new Thread(()=>{
//			Timer timer = new Timer(); timer.Start();
//			MazeGenerator MG = new MazeGenerator(_height,_width);
//			bool[,] tmp = MG.generatedDataWithWall;
//			if(tmp==null || tmp.GetLength(0)!=_height+2 || tmp.GetLength(1)!=_width+2){
//				Debug.LogError("Generate Maze Error!!");
//				return;
//			}
//			timer.Stop();
//			Debug.Log("Generate Maze:"+timer.sec+"[s]");
//			_mazeData = tmp;	
//		});
//		thread.IsBackground = true;
//		thread.Start();
//	}


	/// <summary>
	/// 経路探索. true=成功、false=失敗
	/// </summary>
	/// <returns><c>true</c>, if maze was solved, <c>false</c> otherwise.</returns>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	bool SolveMinimum(Point start, Point goal){
		if(!isGenerated)	return false;
		#if MY_DEBUG
		Timer timer = new Timer(); timer.Start();
		#endif
		if(!MazeSolver.SolveMinimum(start,goal,this._wallData,out this._routeData,out this._routeLengthData,out this._routeLength)){
			Debug.LogError("Solve Maze Error!!");
			return false;
		}
		#if MY_DEBUG
		Debug.Log("Route lenght: "+this.routeLength);
		timer.Stop();Debug.Log("Solve Maze:"+timer.msec+"[msec]");
		#endif

		return true;
	}


	/// <summary>
	/// スタートからの距離を全探索. true=成功、false=失敗
	/// </summary>
	/// <returns><c>true</c>, if all was searched, <c>false</c> otherwise.</returns>
	/// <param name="start">Start.</param>
	/// <param name="goal">Goal.</param>
	bool SearchAll(Point start, Point? goal){
		if(!isGenerated)	return false;
		#if MY_DEBUG
		Timer timer = new Timer(); timer.Start();
		#endif
		if(!MazeSolver.SearchAll(start,goal,this._wallData,out this._routeData,out this._routeLengthData,out this._routeLength)){
			Debug.LogError("Search whole maze Error!!");
			return false;
		}
		#if MY_DEBUG
		Debug.Log("Route lenght: "+this.routeLength);
		timer.Stop();Debug.Log("Search whole maze:"+timer.msec+"[msec]");
		#endif

		return true;
	}
		

	bool SolveMinimum2(Point start, Point goal){
		if(!isGenerated)	return false;
		#if MY_DEBUG
		Timer timer = new Timer(); timer.Start();
		#endif
		MazeSolver MS = new MazeSolver(start,goal,this._wallData);
		this._routeData=MS.routeData;
		this._routeLengthData=MS.routeLengthData;
		this._routeLength=MS.routeLength;
		if(this._routeData==null){
			Debug.LogError("Solve Maze Error!!");
			return false;
		}
		#if MY_DEBUG
		Debug.Log("Route lenght: "+this.routeLength);
		timer.Stop();Debug.Log("Solve Maze:"+timer.msec+"[msec]");
		#endif

		return true;
	}


	//経路探索（マルチスレッド）
//	void SolveMazeAsync(Point start, Point goal, bool useDijkstra=false){
//		if(!isGenerated || isSolving)	return;
//		isSolving=true;
//
//		Thread thread = new Thread(()=>{
//			Timer timer = new Timer(); timer.Start();
//			MazeSolver MS = new MazeSolver(start,goal,this._mazeData,useDijkstra);
//			bool[,] tmp = MS.routeData;
//			if(tmp==null || tmp.GetLength(0)!=height+2 || tmp.GetLength(1)!=width+2){
//				Debug.LogError("Solve Maze Error!!");
//				isSolving=false;
//				return;
//			}
//			timer.Stop();
//			Debug.Log("Solve Maze:"+timer.sec+"[s]");
//			Debug.Log("Route lenght: "+MS.routeLength);
//			_routeData = tmp;
//			isSolving = false;
//		});
//		thread.IsBackground = true;
//		thread.Start();
//	}


	//計算時間測定用
	class Timer{
		System.Diagnostics.Stopwatch stopwatch;
		public float sec{get{return stopwatch.ElapsedMilliseconds/1000f;}}
		public long msec{get{return stopwatch.ElapsedMilliseconds;}}

		public Timer(){stopwatch = new System.Diagnostics.Stopwatch();}
		public void Start(){stopwatch.Start();}
		public void Stop(){stopwatch.Stop();}
	}


	//二次元座標を扱う構造体。vector2にdownとleftがなかったから作った
	public struct Point{
		public int x{get; set;}
		public int y{get; set;}
		public Point(int x, int y){this.x=x;this.y=y;}
		public static Point operator+(Point p1, Point p2)	{return new Point(p1.x+p2.x,p1.y+p2.y);}
		public static Point operator-(Point p1, Point p2)	{return new Point(p1.x-p2.x,p1.y-p2.y);}
		public static Point up{get{return new Point(0,1);}}
		public static Point down{get{return new Point(0,-1);}}
		public static Point right{get{return new Point(1,0);}}
		public static Point left{get{return new Point(-1,0);}}
	}




}
