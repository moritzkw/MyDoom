using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Weapon : MonoBehaviour
{
    public GameObject impactEffect;
    public GameObject flareEffect;
    public Transform spawnPoint;
    public Transform orientation;

    public Camera cam;


    private void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10);
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
    }


    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30f))
        {

            Bot hitBot = hit.collider.GetComponent<Bot>();
            if (hitBot != null)
            {
                hitBot.Hit(35);
            }

            GameObject flash = Instantiate(flareEffect, spawnPoint.transform.position, Quaternion.LookRotation(-hit.normal));
            flash.transform.parent = spawnPoint;
            Destroy(flash, 0.2f);
        }
    }
}
