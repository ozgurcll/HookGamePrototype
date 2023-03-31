using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 200;
public ParticleSystem effect;
    public GameObject owner;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        transform.rotation = Quaternion.LookRotation(player.transform.forward);
    }
    void Update()
    {
        
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        Instantiate(effect , transform.position , Quaternion.identity);


    }
    private void OnTriggerEnter(Collider other)
    {


        if (other.gameObject.GetComponent<Destroy>() == false)
        {
            Destroy(gameObject);
        }
    }
}
