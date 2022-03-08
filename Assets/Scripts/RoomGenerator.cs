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

    private GameObject[,] rooms;
    private RoomGrammar roomGrammar;

    void Start()
	{
        roomGrammar = GetComponent<RoomGrammar>();
        Debug.Log("starting generation");
        //some offsets here to account for the buffer around the actual grid (determines when a barrier room should be placed to stop branch)
        rooms = new GameObject[width + 2, height + 2];
        GameObject instantiatedStartingRoom = Instantiate(startingRoom, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        rooms[startingXCoord + 1, startingYCoord + 1] = instantiatedStartingRoom;
        recursivelyGenerateNextRoom(1, instantiatedStartingRoom, startingXCoord + 1, startingYCoord + 1);
    }

    //recursively picks and places rooms in a depth-first manner
    //general rules:
    //-- if the next placed room would be outside the defined layout size, that is ok, just place a room with ONLY the complementary opening (no additional branches) and thus end recursion on that path
    //-- if the next placed room would be in a spot that already has a room (multiple shared openings that create a loop), don't place anything and move on (end recursion)
    //-- if this iteration would exceed the specified recursion depth, don't do anything
    //the way the indecies and bounds are set up, there should never be an out of bound error on the array, but I'm not using assert statements nor proofs so this is more anecdoatal. find a way to guarantee?
    void recursivelyGenerateNextRoom(int currentDepth, GameObject currentRoom, int currentXPos, int currentYPos) {
        RoomData roomData = currentRoom.GetComponent<RoomData>();

        Debug.Log("room structure: " + currentRoom.name);
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
        GameObject nextRoomPrefab = pickRoom(listOfPossibleRooms, currentXPos, currentYPos, dx, dy);
        GameObject instantiatedRoom = placeRoom(nextRoomPrefab, currentRoom, currentXPos, currentYPos, dx, dy);

        return instantiatedRoom;
    }

    //picks a random room prefab based from the ones specified
    //TODO: I think something is still hanging up here very rarely? Not sure what, but this is the only place in generation I use a while loop
    private GameObject pickRoom(GameObject[] listOfPossibleRooms, int currentXPos, int currentYPos, int dx, int dy) {
        //picks a room from possibilities!
        int roomIndex = Random.Range(0, listOfPossibleRooms.Length);
        GameObject nextRoomPrefab = listOfPossibleRooms[roomIndex];
        //ensures that there will not be an immediate dead end on a room that should branch
        //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
        ArrayList failedIndices = new ArrayList();
        while(nextRoomPrefab.GetComponent<RoomData>().numOpenings == 1 || !roomWouldConnect(nextRoomPrefab, currentXPos + dx, currentYPos + dy)) { 
            failedIndices.Add(roomIndex);
            roomIndex = Random.Range(0, listOfPossibleRooms.Length);
            //TODO: is this needed? less expensive to calculate a new index than it is to look through all the rooms and neighbors (I think... Contains is O(N) though) which is unnecessary for already tried rooms 
            while(failedIndices.Contains(roomIndex)) {
                roomIndex = Random.Range(0, listOfPossibleRooms.Length);
            }
            nextRoomPrefab = listOfPossibleRooms[roomIndex];
        }

        return nextRoomPrefab;
    }

    //given a potential room, make sure it has complementary openings to any rooms that it would connect with (not just currentRoom)
    //nextRoomX and nextRoomY are guaranteed to be in bounds of rooms array, since that gets checked before this method is called
    //returns false if any misalignments are found, true if it passes all of them -- have to check both ways for any neighbor
    //TODO: this does not get called when ending rooms are placed (in placeEndRoom), meaning that there is a small chance those rooms will not connect to other correctly (but overall this seems to work)
    private bool roomWouldConnect(GameObject potentialRoomPrefab, int nextRoomX, int nextRoomY) {
        RoomData rd = potentialRoomPrefab.GetComponent<RoomData>();
        //all of these will short circuit if the neighbor is out of bounds of the array
        //check up neighbor
        if(rooms[nextRoomX, nextRoomY - 1]) {
            Debug.Log("potential room has up neighbor");
            RoomData upNeighborRD = rooms[nextRoomX, nextRoomY - 1].GetComponent<RoomData>();
            if((rd.upOpening && !upNeighborRD.downOpening) || (upNeighborRD.downOpening && !rd.upOpening)) {
                Debug.Log("potential room does not connect with up neighbor");
                return false;
            }
        }
        //check left neighbor
        if(rooms[nextRoomX - 1, nextRoomY]) {
            Debug.Log("potential room has left neighbor");
            RoomData leftNeighborRD = rooms[nextRoomX - 1, nextRoomY].GetComponent<RoomData>();
            if((rd.leftOpening && !leftNeighborRD.rightOpening) || (leftNeighborRD.rightOpening && !rd.leftOpening)) {
                Debug.Log("potential room does not connect with left neighbor");
                return false;
            }
        }
        //check right neighbor
        if(rooms[nextRoomX + 1, nextRoomY]) {
            Debug.Log("potential room right neighbor");
            RoomData rightNeighborRD = rooms[nextRoomX + 1, nextRoomY].GetComponent<RoomData>();
            if((rd.rightOpening && !rightNeighborRD.leftOpening) || (rightNeighborRD.leftOpening && !rd.rightOpening)) {
                Debug.Log("potential room does not connect with right neighbor");
                return false;
            }
        }
        //check down neighbor
        if(rooms[nextRoomX, nextRoomY + 1]) {
            Debug.Log("potential room has down neighbor");
            RoomData downNeighborRD = rooms[nextRoomX, nextRoomY + 1].GetComponent<RoomData>();
            if((rd.downOpening && !downNeighborRD.upOpening) || (downNeighborRD.upOpening && !rd.downOpening)) {
                Debug.Log("potential room does not connect with down neighbor");
                return false;
            }
        }
        Debug.Log("potential room connects with all neighbors! pick it");
        return true;
    }

    //places a new room in relation to the current room given a prefab, coordinates, and direction (dx, dy)
    //can be called without pickRoom first (in the example of placing ending rooms) 
    private GameObject placeRoom(GameObject nextRoomPrefab, GameObject currentRoom, int currentXPos, int currentYPos, int dx, int dy) {
        Debug.Log("Next room coordinate: (" + (currentXPos + dx) + ", " + (currentYPos + dy) + ")");

        //calculate position then instantiate!
        Vector3 nextRoomPos = currentRoom.transform.position;
        nextRoomPos.x += (xOffset * dx);
        //dy is negated because moving up in the rooms array (dy = -1) actually moves positively up in world space -- and vice-versa -- so flip
        nextRoomPos.y += (yOffset * -dy);

        Debug.Log("Current room at:" + currentRoom.transform.position);
        Debug.Log("Generating room at: " + nextRoomPos);

        GameObject instantiatedRoom = Instantiate(nextRoomPrefab, nextRoomPos, Quaternion.identity);
        //does the grammars! applies to the instantiated room, so we don't edit the prefab
        instantiatedRoom = applyGrammars(instantiatedRoom);
        rooms[currentXPos + dx, currentYPos + dy] = instantiatedRoom;

        return instantiatedRoom;
    }

    //uses the grammar class to access the grammar and various properties, that get applied to the room here
    //currently picks a type, gets the associated color, and picks one of the possible objects to place down
    private GameObject applyGrammars(GameObject instantiatedRoom) {
        string roomTypeName = roomGrammar.getNextRoomFromGrammar();
        Debug.Log("room type: " + roomTypeName);

        Color roomTypeColor = roomGrammar.getRoomTypeColor(roomTypeName);
        Debug.Log("room color: " + roomTypeColor.ToString());
        instantiatedRoom.GetComponent<SpriteRenderer>().color = roomTypeColor;

        //make sure the room type has placeable objects before moving on
        if(roomGrammar.canPlaceObject(roomTypeName)) {
            GameObject objectToPlace = roomGrammar.pickObjectToPlaceInRoom(roomTypeName);
            //simply places the object in the center of the room for now
            Vector3 objectToPlacePos = instantiatedRoom.transform.position;
            //make sure to set z correctly so they'll show up on top of rooms
            objectToPlacePos.z = -1.0f;
            GameObject placedObject = Instantiate(objectToPlace, objectToPlacePos, Quaternion.identity);
            //childs the object to the room it's in
            placedObject.transform.SetParent(instantiatedRoom.transform);
        }
       
        return instantiatedRoom;
    }

    //places an ending room (only one opening that complements current room exit) to stop this recursive branch of generation
    //TODO: the openings do not always line up with surrounding neighbors, because we don't check for fit (like in roomWouldConnect) -- refactor/use that method so that these can match all openings too
    //also, remove the green color when done, that's just debug and overwrites the room type color
    private void placeEndingRoom(RoomData roomData, GameObject currentRoom, int currentXPos, int currentYPos) {
        //for now, manually check each opening like recursive cases
        //placeRoom returns a room, but since this is the ending room we don't need to keep track of it for anything else in generation and can discard
        if(roomData.upOpening && !rooms[currentXPos, currentYPos - 1]) {
            GameObject finalRoom = placeRoom(upBranchEndRoom, currentRoom, currentXPos, currentYPos, 0, -1);
            //finalRoom.GetComponent<SpriteRenderer>().color = Color.green;
        }
        if(roomData.leftOpening && !rooms[currentXPos - 1, currentYPos]) {
            GameObject finalRoom = placeRoom(leftBranchEndRoom, currentRoom, currentXPos, currentYPos, -1, 0);
            //finalRoom.GetComponent<SpriteRenderer>().color = Color.green;
        }
        if(roomData.rightOpening && !rooms[currentXPos + 1, currentYPos]) {
            GameObject finalRoom = placeRoom(rightBranchEndRoom, currentRoom, currentXPos, currentYPos, 1, 0);
            //finalRoom.GetComponent<SpriteRenderer>().color = Color.green;
        }
        if(roomData.downOpening && !rooms[currentXPos, currentYPos + 1]) {
            GameObject finalRoom = placeRoom(downBranchEndRoom, currentRoom, currentXPos, currentYPos, 0, 1);
            //finalRoom.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}
