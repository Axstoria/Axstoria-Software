### **BETA TEST PLAN – Axstoria**

## **1. Project context**

Axstoria is a tool to help all TTRPG's players to host online game and create their own world but here in a 3D world.

## **2. User role**

The following roles will be involved in beta testing :

| **Role Name**  | **Description** |
|--------|----------------------|
| Editor       | Outside an online session, the user that will use the edition part of the program to create maps, rules or character sheets. |
| Game Master       | The host of a session, they create the room and manage every aspect of the world on top of having ownership and authority on every object on the server. |
| Player       | Player that has been invited to the session by the host. They have limited ownership on the token that has been attributed to them by the host. |

---

## **3. Feature table**

The following features will be shown during the defense

| **Feature ID** | **User role** | **Feature name** | **Short description** |
|--------------|---------------|-------------------------|--------------------------------------|
| F1 | Editor | Table Grid Creation | Create a custom 2D grid for map layout at map initialization |
| F2 | Editor | UI panel | Show or hide different UI panels |
| F3 | Everyone | Free Camera movement | Move the camera freely in a 3D environment |
| F4 | Everyone | Camera preset angle | Change smoothly the camera between two preset angles : top and isometric |
| F5 | Editor | 3D Object Importation | Import 3D object as game object to use them to create your map |
| F6 | Editor | Object Manipulation | Handle location, rotation and scaling of object on map |
| F7 | Editor | Place object freely | Place 3D objects freely on the map |
| F8 | Editor | Snap object to grid | Place 3D objects on the grid with the snapping tool |
| F9 | Editor | Object preview | Display a preview of where an object will be placed |
| F10 | Editor | Custom Metadata | Add custom metadata (notes, tag, link to sheet) to an object |
| F11 | Editor | Fog of War placement | Place a volume base fog of war to hide a part of the map |
| F12 | Editor | Fog of War depth | Chose the depth of how much layers of class will be affected by the fog (tokens, objects, terrains, tiles) |
| F13 | Editor | Character's Tags | Create tags that the GM can use to assign the visibility and permissions on tokens, objects and fog of war |
| F14 | Editor | Save and Load map | Save and load a map's data |
| F15 | Editor | Character Sheet Creation | Create a character sheets using the different nodes available and variable parameters |
| F16 | Editor | Import and Export Sheets | Import and export the character sheets they created in order to access them during the game |
| F17 | Game Master | Session hosting and invitation | Create a lobby room where they can invite their players via steam |
| F18 | Game Master | Session configuration | Change the parameters of the game such as the state of the map's elements and character sheets in the lobby |
| F19 | Game Master | Tags assignation | Assign the character's tags to player's to replicate the visibility and permission parameters |
| F20 | Game Master | Server Authority | If there's a conflictual action, the game master acts as a server and has the authority on the outcome. |
| F21 | Game Master | Data synchronization | Synchronize the data on the server and send it to the player |
| F22 | Player | Move Owned object | Move an object you own |
| F23 | Game Master | Set Object Visibility | Set the visibility of an object to a specific or every user (can use the tags for this, but it's specifically to let control during the session for last minute changes) |
| F24 | Game Master | Set Metadata visibility | Change the visibility of an object's metadata for set user (ex: a character's token with a link to the character sheet) (can use the tags for this, but it's specifically to let control during the session for last minute changes) |
| F25 | Game Master | Set Ownership | Change the ownership of an object to a specific user (can use the tags for this, but it's specifically to let control during the session for last minute changes) |
| F26 | Editor | Per-Player Fog | Decide which player will be affected by the fog of war (can use the tags for this, but it's specifically to let control during the session for last minute changes) |
| F27 | Player/Game Master | Dice roll | Throw one or more specific dice through the UI or a command line in chat and the result will display visually and in chat |
| F28 | Game Master/Player | Hidden rolls | Hide your dice roll, the result will only display to the user and the game master |
| F29 | Game Master/Player | Distance tool | Calculate and display the distance between two points in meters |
| F30 | Game Master/Player | Ping tool | Display a visual marker on the map with the user's color |
| F31 | Game Master/Player | Line of Sight Tool | Choose two points on the map and display if there's a direct line of sight or not |
| F32 | Game Master | Save and Load map state | User can save the state of a game during a session in order to load it in the exact same configuration in a future session |

---

## **4. Success Criteria**

[Define the metrics and conditions that determine if the beta version is successful.]

| **Feature ID** | **Key success criteria** | **Indicator/metric** | **Result** |
|--------------|---------------------------------------|-----------------------|----------------|
| F1 | Creation of a grid that fits all the parameters | 10 attempts, grid with random parameter properly generated | Result |
| F2 | User can display the desired panel without corruption | 5 attempts, hide or display a random panel | Result |
| F3 | A player can move easily from one point to another | 10 attempts, a new tester can move to a chosen point on a given map without indication | Result |
| F4 | Changing angle must be smooth without abrupt jump or broken view | Change angle 20 times in a row without problem | Result |
| F5 | Object must get all the attributes of a gameobject and be displayable | 10 attempts, object must be visible and interactable | Result |
| F6 | Transformation must give the correct result | 35 attempts, transformation applied without corruption | Result |
| F7 | Object must be displayed at the correct coordinates | 15 attempts, placement and transformation must succeed and stay visible | Result |
| F8 | Object must snap correctly to the middle of the tile | 15 attempts, terrain object must snap correctly | Result |
| F9 | Preview correctly displayed | 10 attempts, object placed with correct preview | Result |
| F10 | Metadata must be correcly saved and displayed | 15 attempts, add data, reload the project and access the data properly | Result |
| F11 | Map must be hidden in the fog | 10 attempts, map must be hidden for a player | Result |
| F12 | The non affected layers must be displayed with a dark mask on | 15 attempts, map must be hidden to the correct depth for a player | Result |
| F13 | Create a tag with no duplicate and assign the correct properties | 20 attempts, can access the tags from the different pannel and properties are saved | Result |
| F14 | Can save/load a map without corruption or loss | 10 attempts, save close and then load a map | Result |
| F15 | User can add the desired block and modify the text and variable | 10 attempts, create a sheet with a random number of block | Result |
| F16 | Can save/load a sheet without corruption or loss | 10 attempts, save close and then load a sheet | Result |
| F17 | User can join the session with an invitation and talk in the chat | 15 attempts, no blocking or message not displayed | Result |
| F18 | Start the session with the desired parameters | 10 attemps, start a session with random parameters | Result |
| F19 | Tags assigned to the player id correctly and properties transmitted | 10 attempts, 5 attemps only one player can see a note, 5 attemps only one player can move a pawn | Result |
| F20 | The server rollback the conflicting user to the server's data | 15 attempts, all rolled back | Result |
| F21 | Every player screen's must be up to date with the server | 20 attempts, do an action on any player or on the host and verify that it's propagated to everyone | Result |
| F22 | Object can be moved only with permission | 20 attempts, try to move an object with and without permission | Result |
| F23 | Object visibility must change on the different screens at runtime | 10 attempts, change object visibility on game master and verify the result on player screen | Result |
| F24 | Metadata must be accessible only when permission is given | 10 attempts, try to access the metadata with or without permission and have correct result | Result |
| F25 | Object parameters must be interactable only when permission is given | 10 attempts, try to access the object with or without permission and have correct result | Result |
| F26 | Fog must be displayed to the correct users | 10 attempts, is the layer mask working as intended | Result |
| F27 | The result must be correctly formatted and displayed | 25 attempts, roll a random set of dice with correct format | Result |
| F28 | Hidden option must hide the dice result to the non concerned user | 15 attempts (10 user, 5 game master), roll a set of dices, result must be displayed to the correct users | Result |
| F29 | Display the correct distance between two points | 20 attempts, no wrong distance displayed (10% error margin) | Result |
| F30 | Ping must be displayed on everyone's screen with the player's color | 20 attempts, proper display | Result |
| F31 | Line of sight must be displayed with breaking point highlated | 30 attempts (10 without obstacle), correctly show the line of sight (15% error margin with obstacle) | Result |
| F32 | Map must be loaded without loss or corruption | 15 attempts without error | Result |
