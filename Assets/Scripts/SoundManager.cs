using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource ShootingChannel;

    public AudioClip M4_8Shot;
    public AudioClip M16Shot;
    public AudioClip PistolShot;

    public AudioSource reloadingSoundM16a4;
    public AudioSource reloadingSoundM1911;
    public AudioSource reloadingSoundM4_8;
    public AudioSource emptyMagazineSoundM1911;

    public AudioSource throwablesChannel;
    public AudioClip grenadeSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                ShootingChannel.PlayOneShot(PistolShot);
                break;
            case WeaponModel.M4_8:
                ShootingChannel.PlayOneShot(M4_8Shot);
                break;
            case WeaponModel.M16a4:
                ShootingChannel.PlayOneShot(M16Shot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                reloadingSoundM1911.Play();
                break;
            case WeaponModel.M4_8:
                reloadingSoundM4_8.Play();
                break;
            case WeaponModel.M16a4:
                reloadingSoundM16a4.Play();
                break;
        }
    }
}
