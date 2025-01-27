﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharShootingScript : MonoBehaviour {
    //public Rigidbody projectile;
    public GameObject ragdoll;
    public Transform releasePoint;
    public AudioSource shootingAudio;
    public AudioClip fireClip;
    public AudioClip chargingClip;
    public CharControlScript ccScript;
    public LaunchMeshScript launchMeshScript;
    public GameObject launchMesh;

    // Set Values
    public float minLaunchForce = 15f;
    public float maxLaunchForce = 30f;
    public float maxChargeTime = .75f;
    public float ammo;

    private float mCurrentLaunchForce;
    private float mCurrentAngle;
    public bool justFired;
    private float mChargeSpeed;
    private bool mFired;

    private void OnEnable()
    {
        mCurrentLaunchForce = minLaunchForce;
    }

	// Use this for initialization
	private void Start () {
        mChargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
	}

	public int getAmmo() {
		return (int) ammo;
	}
	
	private void Update () {
        // At max charge but not yet fired
        if (ammo > 0 && ccScript.isAiming)
        {
            if (ccScript.explode)
            {
                mCurrentLaunchForce = minLaunchForce;
                Fire();
                ccScript.isAiming = false;
            } else if (mCurrentLaunchForce >= maxLaunchForce && !mFired)
            {
                mCurrentLaunchForce = maxLaunchForce;
                Fire();
            }
            else if (Input.GetButtonDown("Fire1"))
            {
                // Press fire button for first time
                mFired = false;
                mCurrentLaunchForce = minLaunchForce;

                // Play charging sounds
                shootingAudio.clip = chargingClip;
                shootingAudio.Play();

                // Render Launch Arc
                mCurrentAngle = Vector3.Angle(Vector3.forward, new Vector3(0, releasePoint.forward.y + .1f, 1));
                // HACK: Launch arc calcs don't really work for negative angles
                mCurrentAngle = releasePoint.forward.y > 0 ? mCurrentAngle : -mCurrentAngle;
                launchMeshScript.RedrawArc(mCurrentLaunchForce / 6, mCurrentAngle);
            }
            else if (Input.GetButton("Fire1") && !mFired)
            {
                // Holding the button, but not yet fired
                mCurrentLaunchForce += mChargeSpeed * Time.deltaTime;

                // Set launch arc to new direction and power
                mCurrentAngle = Vector3.Angle(Vector3.forward, new Vector3(0, releasePoint.forward.y + .1f, 1));
                //angle = releasePoint.forward.y > 0 ? angle : 0.1f;
                mCurrentAngle = releasePoint.forward.y > 0 ? mCurrentAngle : -mCurrentAngle;
                launchMeshScript.RedrawArc(mCurrentLaunchForce / 6, mCurrentAngle);
                Debug.DrawRay(releasePoint.position + 2 * releasePoint.forward, releasePoint.forward * mCurrentLaunchForce);
            }
            else if (Input.GetButtonUp("Fire1") && !mFired)
            {
                // Release and haven't yet fired
                Fire();
            }
        } else
        {
            mFired = false;
            mCurrentLaunchForce = minLaunchForce;

            // Remove any current launching indicators
            launchMeshScript.ClearMesh();
            shootingAudio.Stop();
        }
	}

    private void Fire()
    {
        mFired = true;
        justFired = true;

        // Play firing sound
        shootingAudio.clip = fireClip;
        shootingAudio.Play();

        // Instantiate Projectile with velocity
        //Rigidbody projectileInstance = Instantiate(projectile, releasePoint.position, releasePoint.rotation) as Rigidbody;
        //projectileInstance.velocity = mCurrentLaunchForce * releasePoint.forward;

        // Instantiate Ragdoll instead of Projectile
        GameObject ragInstance = Instantiate(ragdoll, releasePoint.position + 2 * releasePoint.forward, releasePoint.rotation) as GameObject;
        GameObject ragMain = ragInstance.transform.Find("mixamorig:Hips").gameObject;
        Rigidbody ragMainRbody = ragMain.GetComponent<Rigidbody>();
        ragMainRbody.velocity = mCurrentLaunchForce * releasePoint.forward;
		ragInstance.tag = "Projectile";

        // Reset Launch force
        mCurrentLaunchForce = minLaunchForce;

        // Clear the launch arc mesh after firing
        launchMeshScript.ClearMesh();

        // Take down ammo count
        ammo--;
    }
}
