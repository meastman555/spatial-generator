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
    void recursivelyGenerateNextRoom(int currentDepth, GameObject currentRoom, int currentXPos, int currentYPos) {
        RoomData roomData = currentRoom.GetComponent<RoomData>();

        Debug.Log("room type: " + currentRoom.name);
        Debug.Log("number of openings on the room: " + roomData.numOpenings);

        //depth check, make sure all the openings of the current room are closed and end branching
        if(currentDepth > recursionDepth) {
            //for now, manually check each opening like recursive cases
            if(roomData.upOpening && !rooms[currentXPos, currentYPos - 1]) {
                rooms[currentXPos, currentYPos - 1] = true;
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.y += yOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(upBranchEndRoom, endRoomPos, Quaternion.identity);
            }
            if(roomData.leftOpening && !rooms[currentXPos - 1, currentYPos]) {
                rooms[currentXPos - 1, currentYPos] = true;
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x -= xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(leftBranchEndRoom, endRoomPos, Quaternion.identity);
            }
            if(roomData.rightOpening && !rooms[currentXPos + 1, currentYPos]) {
                rooms[currentXPos + 1, currentYPos] = true;
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x += xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(rightBranchEndRoom, endRoomPos, Quaternion.identity);
            }
            if(roomData.downOpening && !rooms[currentXPos, currentYPos + 1]) {
                rooms[currentXPos, currentYPos + 1] = true;
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.y -= yOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(downBranchEndRoom, endRoomPos, Quaternion.identity);
            }
            //breaks the recursion on this branch!
            return;
        }

        //check each opening *independently*
        //order doesn't really matter
        //find a way to loop these? each room keeps track of the number of openings it has

        if(roomData.upOpening) {
            //in bounds and there isn't a room, place one do all that
            if(currentYPos - 1 > 0 && !rooms[currentXPos, currentYPos - 1]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos, currentYPos - 1] = true;

                Debug.Log("Next room coordinate: (" + currentXPos + ", " + (currentYPos + 1) + ")");

                //picks a room from possibilities!
                int randIndex = Random.Range(0, downOpeningRooms.Length);
                GameObject nextRoom = downOpeningRooms[randIndex];
                //ensures that there will not be an immediate dead end on a room that should branch
                //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
                while(nextRoom.GetComponent<RoomData>().numOpenings == 1) { 
                    randIndex = Random.Range(0, downOpeningRooms.Length);
                    nextRoom = downOpeningRooms[randIndex];
                }

                //TODO: this is where a grammer will be placed to change the base room's attributes before instantiating!
                //set up all the needed data then create the room
                Vector3 nextRoomPos = currentRoom.transform.position;
                nextRoomPos.y += yOffset;

                Debug.Log("Current room at:" + currentRoom.transform.position);
                Debug.Log("Generating room at: " + nextRoomPos);
                
                GameObject instantiatedRoom = Instantiate(nextRoom, nextRoomPos, Quaternion.identity);

                //recursively branch!
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos, currentYPos - 1);
            }
            //just because we go out of bounds doesn't mean there isn't already a room there! have to check??
            else if(!rooms[currentXPos, currentYPos - 1]) {
                rooms[currentXPos, currentYPos - 1] = true;
                //generate ending room, does't call recursion function
                Debug.Log("hit the top of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.y += yOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(upBranchEndRoom, endRoomPos, Quaternion.identity);
            }

        }

        if(roomData.leftOpening) {
            if(currentXPos - 1 > 0 && !rooms[currentXPos - 1, currentYPos]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos - 1, currentYPos] = true;

                Debug.Log("Next room coordinate: (" + (currentXPos - 1) + ", " + currentYPos + ")");

                //picks a room from possibilities!
                int randIndex = Random.Range(0, rightOpeningRooms.Length);
                GameObject nextRoom = rightOpeningRooms[randIndex];
                //ensures that there will not be an immediate dead end on a room that should branch
                //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
                while(nextRoom.GetComponent<RoomData>().numOpenings == 1) { 
                    randIndex = Random.Range(0, rightOpeningRooms.Length);
                    nextRoom = rightOpeningRooms[randIndex];
                }

                //TODO: this is where a grammer will be placed to change the base room's attributes before instantiating!
                //set up all the needed data then create the room
                Vector3 nextRoomPos = currentRoom.transform.position;
                nextRoomPos.x -= xOffset;

                Debug.Log("Current room at:" + currentRoom.transform.position);
                Debug.Log("Generating room at: " + nextRoomPos);

                GameObject instantiatedRoom = Instantiate(nextRoom, nextRoomPos, Quaternion.identity);

                //recursively branch!
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos - 1, currentYPos);
            }
            else if(!rooms[currentXPos - 1, currentYPos])  {
                rooms[currentXPos - 1, currentYPos] = true;
                //generate ending room, does't call recursion function
                Debug.Log("hit the left of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x -= xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(leftBranchEndRoom, endRoomPos, Quaternion.identity);
            }
        }

        if(roomData.rightOpening) {
            if(currentXPos + 1 < width - 1 && !rooms[currentXPos + 1, currentYPos]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos + 1, currentYPos] = true;

                Debug.Log("Next room coordinate: (" + (currentXPos + 1) + ", " + currentYPos + ")");

                //picks a room from possibilities!
                int randIndex = Random.Range(0, leftOpeningRooms.Length);
                GameObject nextRoom = leftOpeningRooms[randIndex];
                //ensures that there will not be an immediate dead end on a room that should branch
                //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
                while(nextRoom.GetComponent<RoomData>().numOpenings == 1) { 
                    randIndex = Random.Range(0, leftOpeningRooms.Length);
                    nextRoom = leftOpeningRooms[randIndex];
                }

                //TODO: this is where a grammer will be placed to change the base room's attributes before instantiating!
                //set up all the needed data then create the room
                Vector3 nextRoomPos = currentRoom.transform.position;
                nextRoomPos.x += xOffset;

                Debug.Log("Current room at:" + currentRoom.transform.position);
                Debug.Log("Generating room at: " + nextRoomPos);

                GameObject instantiatedRoom = Instantiate(nextRoom, nextRoomPos, Quaternion.identity);

                //recursively branch!
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos + 1, currentYPos);
            }
            else if(!rooms[currentXPos + 1, currentYPos]) {
                rooms[currentXPos + 1, currentYPos] = true;
                //generate ending room, does't call recursion function
                Debug.Log("hit the right of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x += xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(rightBranchEndRoom, endRoomPos, Quaternion.identity);
            }
        }

        if(roomData.downOpening) {
            if(currentYPos + 1 < height - 1 && !rooms[currentXPos, currentYPos + 1]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos, currentYPos + 1] = true;

                Debug.Log("Next room coordinate: (" + currentXPos + ", " + (currentYPos + 1) + ")");

                //picks a room from possibilities!
                int randIndex = Random.Range(0, upOpeningRooms.Length);
                GameObject nextRoom = upOpeningRooms[randIndex];
                //ensures that there will not be an immediate dead end on a room that should branch
                //just re-pick a room until one works (probably not great practice but there are only 8 choices and only one invalid one so it shouldn't take long)
                while(nextRoom.GetComponent<RoomData>().numOpenings == 1) { 
                    randIndex = Random.Range(0, upOpeningRooms.Length);
                    nextRoom = upOpeningRooms[randIndex];
                }

                //TODO: this is where a grammer will be placed to change the base room's attributes before instantiating!
                //set up all the needed data then create the room
                Vector3 nextRoomPos = currentRoom.transform.position;
                nextRoomPos.y -= yOffset;
                
                Debug.Log("Current room at:" + currentRoom.transform.position);
                Debug.Log("Generating room at: " + nextRoomPos);

                GameObject instantiatedRoom = Instantiate(nextRoom, nextRoomPos, Quaternion.identity);

                //recursively branch!
                recursivelyGenerateNextRoom(currentDepth + 1, instantiatedRoom, currentXPos, currentYPos + 1);
            }
            else if(!rooms[currentXPos, currentYPos - 1]) {
                rooms[currentXPos, currentYPos + 1] = true;
                //generate ending room, does't call recursion function
                Debug.Log("hit the bottom of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.y -= yOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(downBranchEndRoom, endRoomPos, Quaternion.identity);
            }
        }
    }
}
