using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//just a regular C# class, used to store data; does not interact with unity or game objects at all
//for now english grammars and their game-space equivalents are defined here, not in editor
public class RoomGrammar
{
    //TODO: something for room size (short, long, big, small, horizontal vs vertical, enormous, etc)
    //something else? nested grammars?
    public static Dictionary<string, int> roomSizeDict = new Dictionary<string, int>() 
    {
        { "test", 0 }
    };

    //a separate array for pulling a type from so that they can be weighted with how often each should appear
    public static string[] roomTypes = new string[] { "normal", "normal", "enemy", "enemy", "enemy", "enemy", "enemy", "item", "item", "treasure"};

    //maps am english-grammar room type to a color
    //TODO: for now just uses default Unity colors, add option for user to input their own types/colors in editor?
    public static Dictionary<string, Color> roomTypeDict = new Dictionary<string, Color>() 
    {
        { "normal", Color.white },
        { "enemy", Color.red },
        { "treasure", Color.yellow },
        { "item", Color.blue },
    };
}
