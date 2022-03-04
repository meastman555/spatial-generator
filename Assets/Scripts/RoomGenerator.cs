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
    //if the next placed room would be outside the defined layout size, that is ok, just place a room with ONLY the complementary opening (no additional branches) and thus end recursion on that path
    //if the next placed room would be in a spot that already has a room (multiple shared openings that create a loop), don't place anything and move on (end recursion)
    //if this iteration would exceed the specified recursion depth, don't do anything

    //TODO: something is being a little wonky with the room placement, and I think I'm hitting grid edges too quickly?
    //there is a problem with reuse of spaces, so need to double check the boolean stuff is working as intended (rooms are overwriting existing ones incorrectly)
    //added in branch end rooms, but it appears that some openings are left opened -- check bounds to make sure they're correct?
    void recursivelyGenerateNextRoom(int currentDepth, GameObject currentRoom, int currentXPos, int currentYPos) {

        //depth check
        if(currentDepth > recursionDepth) {
            return;
        }

        RoomData roomData = currentRoom.GetComponent<RoomData>();

        Debug.Log("number of openings on the room: " + roomData.numOpenings);

        //check each opening *independently*
        //order doesn't really matter
        //find a way to loop these? each room keeps track of the number of openings it has

        if(roomData.upOpening) {
            if(currentYPos - 1 >= 0 && !rooms[currentXPos, currentYPos - 1]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos, currentYPos - 1] = true;

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
            else {
                //generate ending room, does't call recursion function
                Debug.Log("hit the top of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.y += yOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(upBranchEndRoom, endRoomPos, Quaternion.identity);
            }

        }

        if(roomData.leftOpening) {
            if(currentXPos - 1 >= 0 && !rooms[currentXPos - 1, currentYPos]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos - 1, currentYPos] = true;

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
            else {
                //generate ending room, does't call recursion function
                Debug.Log("hit the left of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x -= xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(leftBranchEndRoom, endRoomPos, Quaternion.identity);
            }
        }

        if(roomData.rightOpening) {
            if(currentXPos + 1 < width && !rooms[currentXPos + 1, currentYPos]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos - 1, currentYPos] = true;

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
            else {
                //generate ending room, does't call recursion function
                Debug.Log("hit the right of the grid, placing barrier room to stop recursion");
                Vector3 endRoomPos = currentRoom.transform.position;
                endRoomPos.x += xOffset;
                //don't think I need to save reference to this since I'm not using it
                GameObject instantiatedEndRoom = Instantiate(rightBranchEndRoom, endRoomPos, Quaternion.identity);
            }
        }

        if(roomData.downOpening) {
            if(currentYPos + 1 < height && !rooms[currentXPos, currentYPos + 1]) {
                //this room is in the grid and there isn't anything already there
                rooms[currentXPos, currentYPos + 1] = true;

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
            else {
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
