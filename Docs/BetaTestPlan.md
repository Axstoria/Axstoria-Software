### **BETA TEST PLAN – Axstoria**

## **1. Project context**

Axstoria is a tool to help all TTRPG's player to host online game and create their own world but here in a 3D world.

## **2. User role**

The following roles will be involved in beta testing

| **Role Name**  | **Description** |
|--------|----------------------|
| Editor       | Outside an online session, the user that will use the edition part of the program to create maps, rules or character sheets |
| Game Master       | The host of a session, they create the room and manage every aspect of the world on top of having ownership and authority on every object as server. |
| Player       | Player that has been invited to the session by the host. They have limited ownership on the token that has been attributed to them by the host |

---

## **3. Feature table**

The following features will be shown during the defense

| **Feature ID** | **User role** | **Feature name** | **Short description** |
|--------------|---------------|-------------------------|--------------------------------------|
| F1 | Everyone | Free Camera movement | Ability to move the camera freely in a 3D environment |
| F2 | Everyone | Camera preset angle | Can change smoothly the camera between two preset angles top and isometric |
| F3 | Editor | Table Grid Creation | Create a custom 2D grid for map layout at map initialization |
| F4 | Editor | Tile management | Create move and delete tiles freely to act as a base for the map layout |
| F5 | Editor | Object Manipulation | Handle location, rotation and scaling of object on map |
| F6 | Editor | Object preview | Display a preview of where an object will be placed |
| F7 | Editor | Fix terrain object on tile | Terrain object must be able to snap on the tile they sit on |
| F8 | Editor | 3D Object Importation | Can import 3D object as game object to use them to create your map |
| F9 | Editor | Save and Load map | Can save a map with a custom name and laod it in the according menu later |
| F10 | Editor | Free decorative object manipulation | Non-terrain object can be placed freely on the map no matter grid or 2D placement |
| F11 | Editor | UI panel | User can activate or deactivate different UI panels |
| F12 | Editor | Character Sheet Creation | User can create a character sheets using the different "block" availables and variable parameters |
| F13 | Editor | Import and Export Sheets | User can import and export the character sheets they created in order to access them in the game state |
| F14 | Game Master | Session hosting and invitation | User can create a lobby chat room where he can invite his players via steam |
| F15 | Game Master | Session configuration | During the lobby, the user can change the parameter of the game such as which rules and characters sheets the game will be using |
| F16 | Game Master | Server Authority | If there's a conflictual action, the game master acts as a server and has the authority on the outcome. |
| F17 | Game Master | Data synchronization | Synchronize the data on the server and sends it to the player |
| F18 | Editor | Custom Metadata | Add custom metadata (notes, tag, link to sheet) to an object |
| F19 | Game Master | Set Object Visibility | User can set the visibility of an object to specific or all the users |
| F20 | Game Master | Set Metadata visibility | Change the visibility of an object's metadata for set user (ex: a character's token with a link to the character sheet) |
| F21 | Game Master | Set Ownership | Can change the ownership of an object to a specific user |
| F22 | Player | Move Owned object | User can move an object they own |
| F23 | Player/Game Master | Dice roll | User can throw on or more specific dice through the UI or a command line in chat and the result will display visually and in chat |
| F24 | Game Master/Player | Hidden rolls | User can chose to hide his dice roll, the result will only display to the user and the game master |
| F25 | Game Master | Fog of War placement | User can put a volume base fog of war to hide a part of the map |
| F26 | Game Master | Fog of War depth | User can chose the depth of how much layers of class will be affected by the fog (tokens, objects, terrains, tiles) |
| F27 | Game Master | Per-Player Fog | User can decide which player will be affected by the fog of war |
| F28 | Player | Fog of war dissipation | When the user token gets inside the fog of war it reveals part of the world around him |
| F29 | Game Master | Dissipation power | Set for a token the dissipation power when it enters a the fog of war |
| F30 | Game Master/Player | Distance tool | Calculate and display the distance between two points in meters |
| F31 | Game Master/Player | Ping tool | Display a visual marker on the map with the user's color |
| F32 | Game Master/Player | Line of Sight Tool | Choose two points on the map and display if there's a direct line of sight or not |
| F33 | Game Master | Save and Load map state | User can save the state of a map during a session in order to load it in the exact same configuration in a future session |

---

## **4. Success Criteria**

[Define the metrics and conditions that determine if the beta version is successful.]

| **Feature ID** | **Key success criteria** | **Indicator/metric** | **Result** |
|--------------|---------------------------------------|-----------------------|----------------|
| F1 | A player can move easily from one point to another | 10 attempts, a new tester can move to a chosen point on a given map without indication | Result |
| F2 | Angle changement must be smooth without abrupt jump or broken view | Change angle 20 times in a row without problem | Result |
| F3 | Creation of a grid that fits all the parameters | 10 attempts, grid with random parameter properly generated | Result |
| F4 | Manipulation must give the correct result | 50 attempts,operation executed without corruption | Result |
| F5 | Transformation must give the correct result | 35 attempts, transformation applied without corruption | Result |
| F6 | Preview correctly displayed | 10 attempts, object placed with correct preview | Result |
| F7 | Object must snap correctly to the middle of the tile | 15 attempts, terrain object must snap correctly | Result |
| F8 | Object must get all the attributes of a gameobject and be displayable | 10 attempts, object must be visible and interactable | Result |
| F9 | Can save/load a map without corruption or loss | 10 attempts, save close and then load a map | Result |
| F10 | Object must be displayed at the correct coordinates | 15 attempts, placement and transformation must succeed and stay visible | Result |
| F11 | User can display the desired panel without corruption | 5 attempts, hide or display a random panel | Result |
| F12 | User can add the desired block and modify the text and variable | 10 attempts, create a sheet with a random number of block | Result |
| F13 | Can save/load a sheet without corruption or loss | 10 attempts, save close and then load a sheet| Result |
| F14 | User can join the session with an invitation and talk in the chat | 15 attempts, no blocking or message not displayed | Result |
| F15 | Start the session with the desired parameters | 10 attemps, start a session with random parameters | Result |
| F16 | The server rollback the conflicting user to the server's data | 15 attempts, all rolled back | Result |
| F17 | Every player screen's must be up to date with the server | 20 attempts, do an action on any player or on the host and verify that it's propagated to everyone | Result |
| F18 | Metadata must be correcly saved and displayed | 15 attempts, add data, reload the project and access the data properly | Result |
| F19 | Object must be hidden when the option is activated | 10 attempts, change object visibility on game master and verify the result on player screen | Result |
| F20 | Metadata must be accessible only when permission is given | 10 attempts, try to access the metadata with or without permission and have correct result | Result |
| F21 | Object parameters must be interactable only when permission is given | 10 attempts, try to access the object with or without permission and have correct result | Result |
| F22 | Object can be moved only wiht permission | 20 attempts, try to move an object with and without permission | Result |
| F23 | The result must be correctly formatted and displayed | 25 attempts, roll a random set of dice with correct format | Result |
| F24 | Hidden option must hide the result to the non concerned user | 15 attempts (10 user, 5 game master), roll a set of dices, result must be displayed to the correct users | Result |
| F25 | Map must be hidden in the fog | 10 attempts, map must be hidden for a player | Result |
| F26 | The non affected layers must be displayed with a dark mask on | 15 attempts, map must be hidden to the correct depth for a player | Result |
| F27 | Fog must be displayed to the correct users | 10 attempts, is the layer mask working as intended | Result |
| F28 | The fog must be dissipated by (constant distance value) around the player | 10  attempts, the fog must dissipate properly on the player's screen only with  | Result |
| F29 | Same as before but with a variable value | 15  attempts, the fog must dissipate properly on the player's screen only with a consistent radius | Result |
| F30 | Display the correct distance between two points | 20 attempts, no wrong distance displayed (10% error margin) | Result |
| F31 | Ping must be displayed on everyone's screen with the player's color | 20 attempts, proper display | Result |
| F32 | Line of sight must be displayed with breaking point highlated | 30 attempts (10 without obstacle), correctly show the line of sight (15% error margin with obstacle) | Result |
| F33 | Map must be loaded without loss or corruption | 15 attempts without error | Result |
