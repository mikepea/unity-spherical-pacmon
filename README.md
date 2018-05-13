This is a first bash at getting a projected sphere game
going in Unity3D.

The game displays the output of four cameras placed
around a revolving earth. Theoretically when played back
via a projector with the same depth of field onto a spherical
surface, we should get a projection mapped globe. Hopefully ;)

# TODO

* Make it so game rotates to start position when player dies. Runaround!
* Test to see if HDMI cable will work as input to Datapath X4
* Find DP to DVI adapter

# Setup Notes

* Put a big X on the start spot

# Gameplay Notes

## Pacman

First xbox controller registered ('1') controls pacman. Works as normal. Wakka Wakka.

## Ghosts

Ghosts can't stop moving or turn backwards. Players can 'influence' their direction when at intersections, but if no direction is set they will just keep on moving.

Pressing 'A' will flash the ghost that the controller is controlling. It can be hard to figure out otherwise (and controllers can move about, due to limitations of Xbox wireless adapter and InControl)

There are always 4 ghosts - if a ghost isn't controlled by a player it will be AI-based.

