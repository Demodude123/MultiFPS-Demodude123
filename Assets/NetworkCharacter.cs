using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;


	Animator anim;
	bool gotFirstUpdate = false;
	//float lastUpdateTime;
	float realSpeed = 0.1f;
	float realAimAngle = 0f;
	float realRecievedAimAngle = 0f;
	bool realJumping = true;
	float realReceivedSpeed = 0.1f;
	bool realRecievedJumping = true;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		if(anim==null){
			Debug.Log("No Animator!!!");
		}
	}
	
	// Update is called once per frame
	void Update () {
		realSpeed = anim.GetFloat("Speed");
		realJumping = anim.GetBool("Jumping");
		realAimAngle = anim.GetFloat ("AimAngle");
		//Debug.Log (realAimAngle);
		if (photonView.isMine) {
			//Do nothing -- the character motor/input/etc... is moving us
		}
		else{
			transform.position = Vector3.Lerp (transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp (transform.rotation, realRotation, 0.1f);

		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){

		//Debug.Log ("OnPhotonSerializeView");

		if (stream.isWriting) {
			// This is OUR player. We need to send our actual positon to the network.
			stream.SendNext (transform.position);
			stream.SendNext (transform.rotation);
			stream.SendNext(realSpeed);
			stream.SendNext(realJumping);
			stream.SendNext(realAimAngle);
			
		} else {
			//This is someone else's player. We need to recieve their postion (as of a few millliseconds ago, and update our version of that player.
			//transform.position = (Vector3)stream.ReceiveNext ();
			//transform.rotation = (Quaternion)stream.ReceiveNext ();

			//Right now, "realPosition" holds the other person's positon at the last frame.
			// Instead of simply updating "realPosition" and continuing ot lerp,
			// we May want to set our transform.position to immediately to this old "realPosition"
			// and then update realPositio


			realPosition = (Vector3)stream.ReceiveNext ();
			realRotation = (Quaternion)stream.ReceiveNext ();
 			anim.SetFloat ("Speed",realReceivedSpeed);
			anim.SetBool ("Jumping",realRecievedJumping);
			anim.SetFloat ("AimAngle",realRecievedAimAngle);
			realReceivedSpeed =(float)stream.ReceiveNext();
			realRecievedJumping = (bool)stream.ReceiveNext();
			realRecievedAimAngle = (float)stream.ReceiveNext();
			//Debug.Log ((Vector3)stream.ReceiveNext());

			if(gotFirstUpdate==false){
				transform.position = realPosition;
				transform.rotation = realRotation;
				//anim.SetFloat ("AimAngle",realAimAngle);
				gotFirstUpdate=true;
			}
		}
	}
}