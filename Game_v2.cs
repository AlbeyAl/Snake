using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_v2 : MonoBehaviour {

	int          xv = 0,  yv = 0;
	int			oXV = 0, oYV = 0;
	public int             tileX;
	int	               	   tileY;

	float 		 	    tileSize;

	public float  spacing = 1.0f;

	float  			    camWidth;
	float			   camHeight;

	public 	 Sprite  snakeSprite;
	public	 Sprite   foodSprite;

	int[,]			    snakeMap;

	int				spawnQue = 5600;

	List<Snake>        snakeList;
	List<GameObject> snakeGOList;

	GameObject food;

	void Start() {
		Cursor.visible = false;

		snakeList = new List<Snake> ();
		snakeGOList = new List<GameObject> ();
		food = new GameObject ("food");

		SetupScene ();
	}

	void SetupScene() {
		// 1.) setup camera width & height:
		camHeight =  	        2.0f * (float)(Camera.main.orthographicSize);
		camWidth  = camHeight * ((float)Screen.width / (float)Screen.height);

		Debug.Log 			    ("Screen aspect: " + (Screen.width / Screen.height));
		Debug.Log     ("Screen information: " + Screen.width + ", " + Screen.height);
		Debug.Log ("Camera width & height in units: " + camWidth + ", " + camHeight);

		// Target tile size:
		tileSize = 		   camWidth / tileX;
		// Set tileY based off of tileSize (camHeight / tileSize)
		tileY = (int)(camHeight / tileSize);

		Debug.Log ("Tile X & Y: " + tileX + ", " + tileY);

		// Setup map with preplanned positions:
		float 			halfSize = tileSize / 2;

		snakeMap = new int[tileX, tileY];

		for (int x = 0; x < tileX; x++) {
			for (int y = 0; y < tileY; y++) {
				snakeMap [x, y] = 0;
			}
		}

		CreateSnakeBody ();

		Snake newSnake = new Snake();
		newSnake.x = tileX / 2;
		newSnake.y = tileY / 2;

		snakeList.Add (newSnake);

		yv = 1;
		xv = 0;

		food.AddComponent<SpriteRenderer> ();
		food.GetComponent<SpriteRenderer> ().sprite = foodSprite;
		food.GetComponent<SpriteRenderer> ().color = Color.yellow;
		float unitSize = foodSprite.texture.width / foodSprite.pixelsPerUnit;
		float    scale =		                           tileSize / unitSize;

		food.transform.localScale = new Vector3 (scale, scale, 1.0f);

		MoveFood ();
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Space)) {
			Reset ();
		}

		if (Input.GetKeyUp (KeyCode.Keypad0)) {
			lagTimerConst = 5;
			Reset ();
		}

		if (Input.GetKeyUp (KeyCode.Keypad1)) {
			lagTimerConst = 4;
			Reset ();
		}		

		if (Input.GetKeyUp (KeyCode.Keypad2)) {
			lagTimerConst = 3;
			Reset ();
		}

		if (Input.GetKeyUp (KeyCode.Keypad3)) {
			lagTimerConst = 1;
			Reset ();
		}

		if (Input.GetKeyUp (KeyCode.Keypad4)) {
			lagTimerConst = 0;
			Reset ();
		}

		GetInput  ();
	}

	int lagTimerConst = 5;
	int lagTimer = 0;
	void FixedUpdate() {
		if (lagTimer > 0) {
			lagTimer--;
		} else {
			lagTimer = lagTimerConst;

			MoveSnake ();
			Draw ();
		}
	}

	void Reset() {
		snakeMap = new int[tileX, tileY];
		snakeList.Clear ();

		for (int i = 0; i < snakeGOList.ToArray ().Length; i++) {
			GameObject.Destroy (snakeGOList [i]);
		}

		snakeGOList.Clear ();

		GameObject.Destroy (food);

		Time.timeScale = 1;

		Start ();
	}

	void SpawnSnakeBody() {
		// Check to see if it can spawn new snake body behind last snake body:
		Snake lastSnake =        snakeList [snakeList.ToArray ().Length - 1];
		int _xv = xv;
		int _yv = yv;

		if (snakeList.ToArray ().Length > 1) {
			Snake secondToLast = snakeList [snakeList.ToArray ().Length - 2];

			_xv = secondToLast.x - lastSnake.x;
			_yv = secondToLast.y - lastSnake.y;
		} 

		int x = lastSnake.x + (_xv * -1);
		int y = lastSnake.y + (_yv * -1);

		if (GetMap(x, y) == 0) {
			CreateSnakeBody ();
			Snake 	  newSnake = new Snake();

			newSnake.x = x;
			newSnake.y = y;

			snakeList.Add (newSnake);

			SetSnake (newSnake.x, newSnake.y, snakeList.ToArray ().Length - 1);

			spawnQue--;
		}
	}

	void MoveSnake() {

		// Update map:
		UpdateMap ();

		int snakeCount = snakeList.ToArray ().Length;

		if (snakeCount > 1) {
			for (int i = snakeCount - 1; i > 0; i--) {
				if (i > 0) {
					SetSnake (snakeList [i - 1].x, snakeList [i - 1].y, i);	
				}
			}
		}
				
		// Check to see if it's going to crash into itself:
		SetSnake(snakeList [0].x + xv, snakeList [0].y + yv, 0);

		if (GetMap(snakeList [0].x, snakeList [0].y) == 1) {
			// End game;
			Time.timeScale = 0;

			// Find other snake part that matches snake [0] x & y:
			for (int i = 1; i < snakeList.ToArray ().Length; i++) {
				if (snakeList [0].x == snakeList [i].x && snakeList [0].y == snakeList [i].y) {
					snakeGOList [0].GetComponent<SpriteRenderer> ().color = Color.red;
					snakeGOList [i].GetComponent<SpriteRenderer> ().color = Color.red;
				}
			}
		} else if (GetMap(snakeList [0].x, snakeList [0].y) == 2) {
			// Collect food;
			MoveFood();
			spawnQue++;
		}

		UpdateMap ();

		if (spawnQue > 0) {
			SpawnSnakeBody ();
		}
	}

	void UpdateMap() {						
		for (int x = 0; x < tileX; x++) {
			for (int y = 0; y < tileY; y++) {
				if (GetMap (x, y) != 2) {
					SetMap (x, y, 0);
				}
			}
		}

		for (int i = 0; i < snakeList.ToArray ().Length; i++) {
			SetMap(snakeList [i].x, snakeList [i].y, 1);
		}
	}

	void MoveFood() {
		bool emptySpot = false;

		int x = 0;
		int y = 0;

		while (!emptySpot) {
			x = Random.Range (0, tileX);
			y = Random.Range (0, tileY);

			if (GetMap (x, y) == 0) {
				emptySpot = true;
			}
		}

		SetMap(x, y, 2);
		Debug.Log ("Food: " + GetMap (x, y) + ", x: " + x + ", y: " + y);

		float bX = (Camera.main.transform.position.x - (camWidth / 2)) + (tileSize / 2);
		float bY = (Camera.main.transform.position.y - (camHeight / 2)) + (tileSize / 2);

		food.transform.position = new Vector2 (bX + (x * tileSize), bY + (y * tileSize));
	}

	void CreateSnakeBody() {
		Debug.Log ("Good 3");

		GameObject tempGO = new GameObject ("snakepart_");
		tempGO.AddComponent<SpriteRenderer> ();
		tempGO.GetComponent<SpriteRenderer> ().sprite = snakeSprite;

		// Figure out how much to scale the transform:
		float unitSize = snakeSprite.texture.width / snakeSprite.pixelsPerUnit;

		Debug.Log ("Unit size: " + unitSize);

		float scale = tileSize / unitSize;

		Debug.Log ("Scale size: " + scale);

		tempGO.transform.localScale = new Vector3 (scale, scale, 1.0f);

		snakeGOList.Add (tempGO);
	}

	void GetInput() {

		bool veloChange = false;

		int oXV = xv;
		int oYV = yv;

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			xv = -1;
			yv = 0;

			veloChange = true;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			xv = 1;
			yv = 0;

			veloChange = true;
		}

		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			xv = 0;
			yv = 1;

			veloChange = true;
		}

		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			xv = 0;
			yv = -1;

			veloChange = true;
		}

		if (veloChange) {
			if (snakeList.ToArray ().Length > 1) {
				int cX = snakeList [0].x - snakeList [1].x;
				int cY = snakeList [0].y - snakeList [1].y;

				if (xv + cX == 0) {
					xv = cX;
				}

				if (yv + cY == 0) {
					yv = cY;
				}
			}

			if (xv == oXV || yv == oYV) {
				MoveSnake ();
				Draw ();
			}
		}
	}

	int GetMap(int x, int y) {
		if (x > tileX - 1)
			x = 0;
		else if (x < 0)
			x = tileX - 1;

		if (y > tileY - 1)
			y = 0;
		else if (y < 0)
			y = tileY - 1;

		return snakeMap [x, y];
	}

	void SetMap(int x, int y, int n) {
		if (x > tileX - 1)
			x = 0;
		else if (x < 0)
			x = tileX - 1;

		if (y > tileY - 1)
			y = 0;
		else if (y < 0)
			y = tileY - 1;

		snakeMap [x, y] = n;
	}

	void SetSnake(int x, int y, int i) {
		if (x > tileX - 1)
			x = 0;
		else if (x < 0)
			x = tileX - 1;

		if (y > tileY - 1)
			y = 0;
		else if (y < 0)
			y = tileY - 1;

		snakeList [i].x = x;
		snakeList [i].y = y;
	}

	void Draw() {
		float bX = (Camera.main.transform.position.x - (camWidth / 2)) + (tileSize / 2);
		float bY = (Camera.main.transform.position.y - (camHeight / 2)) + (tileSize / 2);

		if (snakeGOList.ToArray ().Length > 0) {
			for (int i = 0; i < snakeGOList.ToArray ().Length; i++) {
				snakeGOList [i].transform.position = new Vector2 (bX + (snakeList [i].x * tileSize), bY + (snakeList [i].y * tileSize));
			}
		}
	}
}
