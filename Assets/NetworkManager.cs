using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public GameObject standbyCamera;
	SpawnSpot[] spawnSpots;
	int count = 0;
	public bool offlineMode = false;
	bool connecting = false;

	List<string> chatMessages;
	int maxChatMessages = 5;
	// Use this for initialization
	void Start () {
		spawnSpots = GameObject.FindObjectsOfType <SpawnSpot> ();
		PhotonNetwork.player.name = PlayerPrefs.GetString ("Username", "Default");
		chatMessages = new List <string>();
	}

	void OnDestroy(){
		PlayerPrefs.SetString ("Username", PhotonNetwork.player.name);
	}
	[RPC]
	void AddChatMessage_RPC(string m){
		if (chatMessages.Count >= maxChatMessages){
			chatMessages.RemoveAt (0);
		}
		chatMessages.Add (m);
	}

	public void AddChatMessage(string m){
		GetComponent<PhotonView>().RPC("AddChatMessage_RPC",PhotonTargets.All,m);
	}
	void Connect() {
		PhotonNetwork.ConnectUsingSettings ("MultiFPS v003");
	}

	void OnGUI(){
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString() );

		if(PhotonNetwork.connected == false && connecting == false){
			GUILayout.BeginArea (new Rect(0,0,Screen.width,Screen.height));
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Username: ");
			PhotonNetwork.player.name = GUILayout.TextField (PhotonNetwork.player.name);
			GUILayout.EndHorizontal ();


			if(GUILayout.Button ("Single Player")){
				connecting = true;
				PhotonNetwork.offlineMode = true;
				OnJoinedLobby ();
			}
			if(GUILayout.Button ("Multi Player")){
				connecting = true;
				Connect ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		if(PhotonNetwork.connected == true && connecting==false){
			GUILayout.BeginArea (new Rect(0,0,Screen.width,Screen.height));
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace();

			foreach(string msg in chatMessages){
				GUILayout.Label(msg);
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}

	void OnJoinedLobby() {
		PhotonNetwork.JoinRandomRoom ();
	}

	void OnPhotonRandomJoinFailed() {
		PhotonNetwork.CreateRoom (null);
	}

	void OnJoinedRoom(){
		connecting = false;
		SpawnMyPlayer ();
	}

	void SpawnMyPlayer() {
		AddChatMessage ("Spawning player: " + PhotonNetwork.player.name);

		//Start background music
		GameObject music = GameObject.FindWithTag ("Music");
		music.audio.Play ();
		count += 1;
		if (spawnSpots == null) {
			Debug.LogError ("Wierdnes");
			return;
		}
		SpawnSpot mySpawnSpot = spawnSpots [Random.Range (0,spawnSpots.Length)];
		GameObject myPlayerGO = PhotonNetwork.Instantiate ("PlayerController",mySpawnSpot.transform.position,mySpawnSpot.transform.rotation,0);
		myPlayerGO.name = "Player_" + count;
		standbyCamera.SetActive (false);

		//((MonoBehaviour)myPlayerGO.GetComponent ("FPSInputController")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent ("MouseLook")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent ("PlayerMovement")).enabled = true;
		((MonoBehaviour)myPlayerGO.GetComponent ("PlayerShooting")).enabled = true;
		//((MonoBehaviour)myPlayerGO.GetComponent ("CharacterMotor")).enabled = true;
		myPlayerGO.transform.FindChild ("Main Camera").gameObject.SetActive (true);
	}
}
