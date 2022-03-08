using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO for use in final project
//--add more grammars! I think it would be interesting to add some grammars that change the size of rooms (make them long an thin, really large, etc)
//--find a more efficient way to do things (specifically looping each time the current roomtype needs to be accessed)? 
//--abstract this out of the wang tile room generation compeletely so the two don't overlap, instead just work on the same instanced room separately? (mentioned in comment in RoomGenerator.cs)
public class RoomGrammar : MonoBehaviour
{
    //dictionaries or structs are not editor serializable so use a small custom nested class to store relevant data
    [System.Serializable]
    public class RoomType {
        public string name;
        public Color color;
        //need to make more general -- this *has* to be a multiple of 10 integer percentage out of 100 for now
        public int chance;
        public GameObject[] possiblePlacedObjects;
    }
    public RoomType[] roomTypes;

    //will end up storing 10 strings, as a way to weight which one could get picked through randomly indexing in
    private ArrayList roomTypeNamesGrammar;

    void Start() {
        //creates the actual grammar
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

    //for now, just assumes a normal distribution of each object's chance of being picked
    public GameObject pickObjectToPlaceInRoom(string roomTypeName) {
        RoomType currentRT = getRoomType(roomTypeName);
        int randIndex = Random.Range(0, currentRT.possiblePlacedObjects.Length);
        GameObject chosenObject = currentRT.possiblePlacedObjects[randIndex];
        return chosenObject;
    }
}
