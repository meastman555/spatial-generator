using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//There is almost certainly a more efficient way to do some of this, which I'll look into for my final!
public class RoomGrammar : MonoBehaviour
{
    //dictionaries or structs are not editor serializable so use a small custom nested class to store relevant data
    [System.Serializable]
    public class RoomType {
        public string name;
        public Color color;
        //TODO: make more general -- this *has* to be a multiple of 10 integer percentage out of 100 for now
        public int chance;
        public GameObject[] possiblePlacedObjects;
    }
    public RoomType[] roomTypes;

    //TODO: other grammars?

    private ArrayList roomTypeNamesGrammar;

    void Start() {
        //creates the actual grammar by keeping a list of weighted strings corresponding to room types
        roomTypeNamesGrammar = new ArrayList();
        foreach(RoomType rt in roomTypes) {
            //this only works because the chances are assumed to be integer numbers in multiples of 10 from 0 to 100
            int chanceToInt = rt.chance / 10;
            ArrayList tempList = ArrayList.Repeat(rt.name, chanceToInt);
            roomTypeNamesGrammar.AddRange(tempList);
        }
    }

    //does a weighted pick based on the chance percentage of the room type 
    public string getNextRoomFromGrammar() {
        int randIndex = Random.Range(0, roomTypeNamesGrammar.Count);
        string roomTypeString = roomTypeNamesGrammar[randIndex].ToString();
        return roomTypeString;
    }

    private RoomType getRoomType(string roomTypeName) {
        foreach(RoomType rt in roomTypes) {
            if(rt.name == roomTypeName) {
                return rt;
            }
        }
        //should never reach this, but makes compiler happy
        return null;
    }

    public Color getRoomTypeColor(string roomTypeName) {
        RoomType currentRT = getRoomType(roomTypeName);
        Color currentRTColor = currentRT.color;
        return currentRTColor;
    } 

    public bool canPlaceObject(string roomTypeName) {
        RoomType currentRT = getRoomType(roomTypeName);
        bool hasObjects = currentRT.possiblePlacedObjects.Length > 0;
        return hasObjects;
    }

    //for now, just assumes a normal distribution of each object's chance
    public GameObject pickObjectToPlaceInRoom(string roomTypeName) {
        RoomType currentRT = getRoomType(roomTypeName);
        int randIndex = Random.Range(0, currentRT.possiblePlacedObjects.Length);
        GameObject chosenObject = currentRT.possiblePlacedObjects[randIndex];
        return chosenObject;
    }
}
