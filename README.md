# Connect4

https://danigilk.itch.io/connect4

Small Connect4 game.  
This was a game development assignment, I only got a black-box callback to use for the game clicking and I implemented the rest.

In developing the game, I was very focused on writing code that easily allows extensions to the game, even if they're generally unnecessary, it is a "skill showcase".  
Features:
- 3 Game settings (COM vs COM/COM Vs Player/Player vs Player)
- The player system is written generally, so any combination can be
configured easily.
- Music system with volume sliders.
- Extendable game grid (can be configured using GameManager object in Unity)
- Decoupled player system that can potentially extend the game beyond 2 players.
- 2 AI settings (Random AI/ Semi-Random AI (Will perform a blocking move if the other player can win next round))
