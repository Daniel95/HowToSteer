# InfiniteHorizon
A game where you steer a cube through a procedual generated terrain

--Most important world generating scripts--

//controls the progess of generating a chunk
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/World%20Management/MapGenerator.cs

//spawns a terrain of chunks around the player that get removed and spawn when the player moves
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/World%20Management/EndlessTerrain.cs

//islands spawning & logic
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/Biomes/Islands.cs

--noise editing scripts--:

//scripts with general static function and maths to control the noisemap
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/NoiseEditors/NoiseEditor.cs

//paths
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/NoiseEditors/Paths.cs

//drawing paths
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/TerrainTrails/TerrainTrail.cs

--mapdata queueing script--

//script used for queueing mapdata, to that neighbours have the chance to edit it before its mesh gets spawned
https://github.com/Daniel95/InfiniteHorizon/blob/master/Assets/Scripts/World%20Generation/World%20Management/NeighbourKnowledgeQue.cs
