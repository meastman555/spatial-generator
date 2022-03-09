## Spatial-Generator  
For a class on procedural generation in games at UT.  
  
There are two steps to this generation:  
1. A recursive, depth-first Wang Tile algorithm picks the next room prefab based on that would be room's current neighbors and runs either a first fit (all current openings must match, but does not matter if new ones are created) or a best fit (all current openings must match and no new ones are created) algorithm to pick the room.  
2. The instantiated room runs through a grammar to determine what it's type will be (and debug color), and if any objects should spawn on it. The object can vary as well. Room types are weighted, currently objects assume a uniform distribution based on type.  
  
When put together, you get a dungeon with various room types chained together that the player can then navigate. Pretty basic, but shows a nice proof-of-concept and tech test that I want to expand upon in my final project's room generation. I also want to continue to improve the overall code architecture. 
  
Lots of tings are tunable, such as the starting room, the size of the generation, the depth of recursion, etc. With the grammar, more room types can easily be added, the current ones' properties tweaked to liking.  
  
Known Bugs:  
1. Every couple of runs hangs unity, seemingly from an infinite loop. There is only one place in my code that I use a while loop, and theoretically it should break every time (though obviously not). It is when picking rooms, meaning that in some cases the algorithm can't find a valid room, despite the fact that there should always be one regardless of the current state of the layout. Planning on incorporating a quick fix that breaks after a certain number of tiles are tried, but for the final I'll look more into the root cause.  
