using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Gun : ScriptableObject
{
    public GameObject prefab;
    public float aimSpeed;
    public float firerate;
    public float kickback;
    public float recoil;
    public string name;
    public float bloom;
    public int damage;
}
