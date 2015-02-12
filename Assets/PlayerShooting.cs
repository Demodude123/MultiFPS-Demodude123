using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {


	float cooldown = 0;
	FXManager fxManager;
	WeaponData weaponData;

	void Start(){
		fxManager = GameObject.FindObjectOfType <FXManager>();
		if(fxManager==null){
			Debug.LogError("Couldn't find FXManager");
		}

		weaponData = gameObject.GetComponentInChildren<WeaponData> ();
		if(weaponData==null){
			Debug.Log ("Did not find any WeaponData in our children!!!");
			return;
		}
	}

	// Update is called once per frame
	void Update () {
		cooldown -= Time.deltaTime;
		if(Input.GetButton("Fire1")){
			//Player wants to shoot, so shoot
			Fire ();
		}
	}

	void Fire(){
		if(weaponData==null){
			return;
		}

		if(cooldown > 0){
			return;
		}
		Debug.Log ("Firing our gun!!!");

		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		Transform hitTransform;
		Vector3 hitPoint;

		hitTransform = FindClosestHitObject (ray, out hitPoint);

		if(hitTransform != null){
			Debug.Log ("We hit: " + hitTransform.transform.name);
			//We could do a special effect at the hit location
			//DoRicochetEffectAt(hitinfo.point);

			Health h = hitTransform.transform.GetComponent<Health>();

			while(h == null && hitTransform.parent){
				hitTransform = hitTransform.parent;
				h = hitTransform.transform.GetComponent<Health>();
			}

			//Once we hit here, hitTransofmr may not be the hitTransform we started with
		
			if(h != null){
				PhotonView pv = h.GetComponent<PhotonView>();
				if(pv == null){
					Debug.LogError ("FREAK OUT!!!");
				}else{
				h.GetComponent<PhotonView>().RPC ("TakeDamage", PhotonTargets.AllBuffered,weaponData.damage);
				//h.TakeDamage(damage);
				}
			}
		
			if(fxManager != null){

				DoGunFX(hitPoint);
			}else{
				//.We didnt hit anything (except empty space) but lets do a visual fx anyway
					hitPoint = Camera.main.transform.position + (Camera.main.transform.position*100f);
					DoGunFX(hitPoint);
			}
		
		
		
		
		}
		cooldown = weaponData.fireRate;
	}

	void DoGunFX(Vector3 hitPoint){
		fxManager.GetComponent<PhotonView>().RPC ("SniperBulletFX", PhotonTargets.All,weaponData.transform.position,hitPoint);
	}

	Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint){

		RaycastHit[] hits = Physics.RaycastAll (ray);

		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;

		foreach(RaycastHit hit in hits){
			if(hit.transform != this.transform && (closestHit == null || hit.distance < distance)){
				//We have hit something that is:
				//a) not us
				//b)the first thing we hit(that is not us)
				//c) or, if not b, is at least closer than the previous closest thing

				closestHit=hit.transform;
				distance=hit.distance;
				hitPoint = hit.point;
			}
		}

		//ClosestHit is now either still null (i.e we hit nothing) or it cointains the closest thing that is a valid thing to hit

		return closestHit;
	}
}
