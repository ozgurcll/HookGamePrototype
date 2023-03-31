using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAttack : MonoBehaviour
{
    public GameObject[] weapons;
    public GameObject ammo;
    
    float currentFireRate = 0f;
    private int ammoCount = 25;
    int currentAmmo;
    [SerializeField] private bool isPlayer = false;

    private AudioClip clipToPlay;
    private AudioSource audioSource;

    private Transform fireTransform;
    float fireRate = 0.5f;


    public float GetCurrentFireRate
    {
        get
        {
            return currentFireRate;
        }
        set
        {
            currentFireRate = value;
        }
    }

    public int GetAmmo
    {
        get
        {
            return currentAmmo;
        }
        set
        {
            currentAmmo = value;
            if (currentAmmo > ammoCount)
            {
                currentAmmo = ammoCount;
            }
        }

    }

    public int GetClipSize
    {
        get
        {
            return ammoCount;
        }
        set
        {
            ammoCount = value;
        }
    }

    public float GetFireRate
    {
        get
        {
            return fireRate;
        }
        set
        {
            fireRate = value;
        }
    }

    public Transform GetFireTransform
    {
        get
        {
            return fireTransform;
        }
        set
        {
            fireTransform = value;
        }
    }

    public AudioClip GetAudioClip
    {
        get
        {
            return clipToPlay;
        }
        set
        {
            clipToPlay = value;
        }
    }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    

    void Update()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
        PlayerInput();

    }

    private void PlayerInput()
    {
        
            if (Input.GetMouseButtonDown(0))
            {
                if (currentFireRate <= 0 && currentAmmo > 0)
                {
                    Fire();
                }
            }

            switch (Input.inputString)
            {
                case "1":
                    weapons[1].gameObject.GetComponent<Weapons>().GetCurrentAmmoCount = currentAmmo;
                    weapons[0].gameObject.SetActive(true);
                    weapons[1].gameObject.SetActive(false);
                    break;
                case "2":
                    weapons[0].gameObject.GetComponent<Weapons>().GetCurrentAmmoCount = currentAmmo;
                    weapons[1].gameObject.SetActive(true);
                    weapons[0].gameObject.SetActive(false);
                    break;
            }

        
    }

    public void Fire()
    {
        float difference = 180f - transform.eulerAngles.y;
        float targetDifference = 90f;
        if (difference >= 90)
        {
            targetDifference = 90f;
        }
        else if (difference < 90) 
        {
            targetDifference = -90;
        }
        currentAmmo--;
        currentFireRate = fireRate;
        audioSource.PlayOneShot(clipToPlay);
        GameObject bulletClone = Instantiate(ammo, fireTransform.position, Quaternion.Euler(0f , 0f ,targetDifference));
        bulletClone.GetComponent<Bullet>().owner = gameObject; 
    }
}
