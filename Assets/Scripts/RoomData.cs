using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple data storage class that is attached to each room prefab
//needs to be public so room generator class can easily access them (and I promise not to change values needlessly in the code :smile:)
public class RoomData : MonoBehaviour
{
    public int numOpenings;
    public bool upOpening;
    public bool leftOpening;
    public bool rightOpening;
    public bool downOpening;
    public string roomTypeName;

}
