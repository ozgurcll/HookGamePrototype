using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    [SerializeField] private GunAttack attack;
    [SerializeField] private Transform fireTransform;
    [SerializeField] private float fireRate;
    [SerializeField] private int clipSize;
    public AudioClip clip;
    private int currentAmmoCount;

    public int GetCurrentAmmoCount
    {
        get
        {
            return currentAmmoCount;
        }
        set
        {
            currentAmmoCount = value;
        }
    }
    private void Awake()
    {
        currentAmmoCount = clipSize;

    }
   
    private void OnEnable()
    {
        if(attack != null)
        {
            attack.GetFireTransform = fireTransform;
            attack.GetFireRate = fireRate;
            attack.GetClipSize = clipSize;
            attack.GetAmmo = currentAmmoCount;
            attack.GetAudioClip = clip;
        }
    }
}
