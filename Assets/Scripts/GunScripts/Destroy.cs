using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
       private int maxHealth = 1;
    public int currentHealth;
    [SerializeField] private GameObject hitFx;
    [SerializeField] private GameObject dedFx;
    [SerializeField] private AudioClip clipToPlay;


    public int GetHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;
            if(currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }


    }

    public int GetMaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.gameObject.GetComponent<Bullet>();

        if (bullet)
        {
            if(bullet.owner != gameObject)
            {
                currentHealth -= 25;

                AudioSource.PlayClipAtPoint(clipToPlay, transform.position);
                if (hitFx != null)
                {
                    Instantiate(hitFx, transform.position, Quaternion.identity);
                }

                if (currentHealth <= 0)
                {
                    Instantiate(dedFx, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }

                
                Destroy(other.gameObject);

            }
        }
        
    }
}
