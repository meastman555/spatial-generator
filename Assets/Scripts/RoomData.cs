using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple data storage class that is attached to each room prefab
//public bc it's just a couple of ints and I *promise* not to change them in the code
public class RoomData : MonoBehaviour
{
    public int numOpenings;
    public bool upOpening;
    public bool leftOpening;
    public bool rightOpening;
    public bool downOpening;

}
