using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string name;
    public GameObject weaponObject;
    public float swingAngleX = 90f; // g³ówny zamach
    public float swingAngleZ = 20f; // lekki obrót w bok
    public float swingSpeed = 5f;   // szybkoœæ zamachu
}
