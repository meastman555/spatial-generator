using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{

    [Header("Generation Parameters")]
    //X max
    [SerializeField] private int width;
    //Y max
    [SerializeField] private int height;
    [SerializeField] private int recursionDepth;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    [Header("Starting Parameters")]
    [SerializeField] private GameObject startingRoom;
    //if you want to do the middle, it's not (0,0) it's (width/2, height/2)
    [SerializeField] private int startingXCoord;
    [SerializeField] private int startingYCoord;

    [Header("Available Rooms")]
    [SerializeField] private GameObject[] upOpeningRooms;
    [SerializeField] private GameObject[] leftOpeningRooms;
    [SerializeField] private GameObject[] rightOpeningRooms;
    [SerializeField] private GameObject[] downOpeningRooms;
    [SerializeField] private GameObject upBranchEndRoom;
    [SerializeField] private GameObject leftBranchEndRoom;
    [SerializeField] private GameObject rightBranchEndRoom;
    [SerializeField] private GameObject downBranchEndRoom;

    private bool[,] rooms;

    void Start()
	{
        //some offsets here to account for the buffer around the actual grid (determines when a barrier room should be placed to stop branch)
        rooms = new bool[width + 2, height + 2];
        rooms[startingXCoord + 1, startingYCoord + 1] = true;
        GameObject instantiatedStartingRoom = Instantiate(startingRoom, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        recursivelyGenerateNextRoom(1, instantiatedStartingRoom, startingXCoord + 1, startingYCoord + 1);
    }

    //recursively picks and places rooms in a depth-first manner
    //general rules:
    //-- if the next placed room would be outside the defined layout size, that is ok, just place a room with ONLY the complementary opening (no additional branches) and thus end recursion on that path
    //-- if the next placed room would be in a spot that already has a room (multiple shared openings that create a loop), don't place anything and move on (end recursion)
    //-- if this iteration would exceed the specified recursion depth, don't do anything
    //the way the indecies and bounds are set up, there should never be an out of bound error on the array, but I'm not using assert statements nor proofs so this is more anecdoatal. find a way to guarantee?

    //TODO: few things
    //--> sometimes the first room (specified as just U) will regenerate on top of itself at the very end. not sure why, doens't seem to be negatively affecting things, but look in to
    //--> rooms are still generating on top of each other in some cases. not sure why, but need to fix
    //--> a few blocks, in that rooms with openings don't lead to another room correctly, hits the wall. i think this is caused by the branch generation of another set of rooms not communicating with others, leading to linear paths that block each other off
    //-----> maybe each room checks more of its surroundings before deciding which to place?
    //--> ^ before a room is placed, need to check the cell's neighbors to see if there are any openings that HAVE to be matches (prevents a wall being thrown where there shouldn't be)
    void recursivelyGenerateNextRoom(int currentDepth, GameObject currentRoom, int currentXPos, int currentYPos) {
        RoomData roomData = currentRoom.GetComponent<RoomData>();

        Debug.Log("room type: " + currentRoom.name);
        Debug.Log("number of openings on the room: " + roomData.numOpenings);

        //depth check, make sure all the openings of the current room are closed and end branching
        if(currentDepth > recursionDepth) {
            placeEndingRoom(roomData, currentRoom, currentXPos, currentYPos);
            //breaks the recursion on this branch!
            return;
        }

        //checkes each opening of the current room independently and kicks off recursion if the next room is in bounds and there is an available spot
        //if the next room would go out of bounds, but there isn't one there, end the branch by placing a dead-end ending room

        if(roomData.upOpening) {
            //in bounds and there isn't already a room
            if(currentYPos - 1 > 0 && !rooms[currentXPos, currentYPos - 1]) {
                GameObject instantiatedRoom = pickAndPlaceRoom(downOpeningRooms, currentRoom, currentXPos, currentYPos, 0, -1);
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos, currentYPos - 1);
            }
            //if the out of bounds check above breaks, still only want to place a room if there isn't already something there
            else if(!rooms[currentXPos, currentYPos - 1]) {
                //don't need to keep track of this room because it's not used for any more generation
                placeEndingRoom(roomData, currentRoom, currentXPos, currentYPos);
            }
        }

        if(roomData.leftOpening) {
            //in bounds and there isn't already a room
            if(currentXPos - 1 > 0 && !rooms[currentXPos - 1, currentYPos]) {
                GameObject instantiatedRoom = pickAndPlaceRoom(rightOpeningRooms, currentRoom, currentXPos, currentYPos, -1, 0);
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos - 1, currentYPos);
            }
            //if the out of bounds check above breaks, still only want to place a room if there isn't already something there
            else if(!rooms[currentXPos - 1, currentYPos])  {
                //don't need to keep track of this room because it's not used for any more generation
                placeEndingRoom(roomData, currentRoom, currentXPos, currentYPos);
            }
        }

        if(roomData.rightOpening) {
            //in bounds and there isn't already a room
            if(currentXPos + 1 < width - 1 && !rooms[currentXPos + 1, currentYPos]) {
                GameObject instantiatedRoom = pickAndPlaceRoom(leftOpeningRooms, currentRoom, currentXPos, currentYPos, 1, 0);
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos + 1, currentYPos);
            }
            //if the out of bounds check above breaks, still only want to place a room if there isn't already something there
            else if(!rooms[currentXPos + 1, currentYPos]) {
                //don't need to keep track of this room because it's not used for any more generation
                placeEndingRoom(roomData, currentRoom, currentXPos, currentYPos);
            }
        }

        if(roomData.downOpening) {
            //in bounds and there isn't already a room
            if(currentYPos + 1 < height - 1 && !rooms[currentXPos, currentYPos + 1]) {
                GameObject instantiatedRoom = pickAndPlaceRoom(upOpeningRooms, currentRoom, currentXPos, currentYPos, 0, 1);
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos, currentYPos + 1);
            }
            //if the out of bounds check above breaks, still only want to place a room if there isn't already something there
            else if(!rooms[currentXPos, currentYPos + 1]) {
                //don't need to keep track of this room because it's not used for any more generation
                placeEndingRoom(roomData, currentRoom, currentXPos, currentYPos);
            }
        }
    }

    //two step process to handle the next room based on the current, and some additional info
    //calls two separate methods to further clarify their separation (grammars are handled in placeRoom since that's where they need to be applied)
    private GameObject pickAndPlaceRoom(GameObject[] listOfPossibleRooms, GameObject currentRoom, int currentXPos, int currentYPos, int dx, int dy) {
        GameObject nextRoomPrefab = pickRoom(listOfPossibleRooms);
        GameObject instantiatedRoom = placeRoom(nextRoomPrefab, currentRoom, currentXPos, currentYPos, dx, dy);

        return instantiatedRoom;
    }

    //picks a random room prefab based from the ones specified
    private GameObject pickRoom(GameObject[] listOfPossibleRooms) {
        //picks a room from possibilities!
        int roomIndex = Random.Range(0, listOfPossibleRooms.Length);
        GameObject nextRoomPrefab = listOfPossibleRooms[roomIndex];
        //ensures that there will not be an immediate dead end on a room that should branch
        //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
        while(nextRoomPrefab.GetComponent<RoomData>().numOpenings == 1) { 
            roomIndex = Random.Range(0, downOpeningRooms.Length);
            nextRoomPrefab = downOpeningRooms[roomIndex];
        }

        return nextRoomPrefab;
    }

    //places a new room in relation to the current room given a prefab, coordinates, and direction (dx, dy)
    //can be called without pickRoom first (in the example of placing ending rooms) 
    private GameObject placeRoom(GameObject nextRoomPrefab, GameObject currentRoom, int currentXPos, int currentYPos, int dx, int dy) {
        Debug.Log("Next room coordinate: (" + (currentXPos + dx) + ", " + (currentYPos + dy) + ")");

        rooms[currentXPos + dx, currentYPos + dy] = true;
        //set up all needed data then instnatiate the room
        //TODO: more initializations need for instantiation?
        Vector3 nextRoomPos = currentRoom.transform.position;
        nextRoomPos.x += (xOffset * dx);
        //dy is negated because moving up in the rooms array (dy = -1) actually moves positively up in world space -- and vice-versa -- so flip
        nextRoomPos.y += (yOffset * -dy);

        Debug.Log("Current room at:" + currentRoom.transform.position);
        Debug.Log("Generating room at: " + nextRoomPos);

        GameObject instantiatedRoom = Instantiate(nextRoomPrefab, nextRoomPos, Quaternion.identity);
        //does the grammars! applies to the instantiated room, so we don't edit the prefab
        instantiatedRoom = applyGrammars(instantiatedRoom);

        return instantiatedRoom;
    }

    //applies the various grammars to this room and edits it as needed
    //TODO: more grammar stuff! For now just assigns the room a color based on pulled type, but do more
    private GameObject applyGrammars(GameObject instantiatedRoom) {
        int typeIndex = Random.Range(0, RoomGrammar.roomTypes.Length);
        string roomTypeName = RoomGrammar.roomTypes[typeIndex];
        Color roomTypeColor = RoomGrammar.roomTypeDict[roomTypeName];
        instantiatedRoom.GetComponent<SpriteRenderer>().color = roomTypeColor;

        return instantiatedRoom;
    }

    //places an ending room (only one opening that complements current room exit) to stop this recursive branch of generation
    private void placeEndingRoom(RoomData roomData, GameObject currentRoom, int currentXPos, int currentYPos) {
        //for now, manually check each opening like recursive cases
        //placeRoom returns a room, but since this is the ending room we don't need to keep track of it for anything else in generation and can discard
        if(roomData.upOpening && !rooms[currentXPos, currentYPos - 1]) {
            placeRoom(upBranchEndRoom, currentRoom, currentXPos, currentYPos, 0, -1);
        }
        if(roomData.leftOpening && !rooms[currentXPos - 1, currentYPos]) {
            placeRoom(leftBranchEndRoom, currentRoom, currentXPos, currentYPos, -1, 0);
        }
        if(roomData.rightOpening && !rooms[currentXPos + 1, currentYPos]) {
            placeRoom(rightBranchEndRoom, currentRoom, currentXPos, currentYPos, 1, 0);
        }
        if(roomData.downOpening && !rooms[currentXPos, currentYPos + 1]) {
            placeRoom(downBranchEndRoom, currentRoom, currentXPos, currentYPos, 0, 1);
        }
    }
}
