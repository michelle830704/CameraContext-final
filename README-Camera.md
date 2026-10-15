# Context & Camera — Godot 4.5 .NET / C#
Separate comparison project built from your adjusted original project.
Original character, colours, materials and original level meshes/transforms preserved.
A new course extends forward beyond the original green floor, reusing its palette.
CameraCourse creates the extension when running, so it appears in the Remote tree.

## Run
Extract all files, open Godot 4.5 mono/.NET, Import project.godot, Import & Edit, F5.
Requires .NET 8 SDK. Open project.godot to run, game1.sln to edit C# only.
Plug in a controller. HUD reports its name; first connected controller is used.

## Controls
| Input | Action |
| --- | --- |
| Left stick | Camera-relative walking |
| Right stick | Horizontal and vertical camera orbit |
| A / Cross (Godot JoyButton.A) | Jump |
| WASD / arrows | Keyboard walking preview |
| Q/E | Keyboard horizontal orbit |
| I/K | Keyboard vertical orbit |
| Space | Keyboard jump |
| R | Reset player |
| F2 | Toggle visible whisker rays (cyan clear, red hit) |

Keyboard is provided for convenience; demonstrate with joystick for the assignment.
The prior movement modes/crouch/hover remain available but are not required here.

## Walkthrough
1. Walk on original floor. Stop and jump: focus and camera position follow smoothly.
2. Orbit with right stick in both directions and vertically. Pitch clamped -10 to 65 degrees.
3. Turn and walk, release right stick. After 1s, camera slowly aligns behind travel direction.
4. Walk forward through the labelled wall corridor beyond the original floor.
   Orbit near left/right wall: lateral feelers push sideways; sight rays shorten camera distance.
5. Pass small yellow/pink boxes. They collide with player but do not trigger camera avoidance.
6. Jump up steps on the left to the raised surface. Lower camera (right stick upward / I)
   near platform/ground: downward feeler raises camera and reduces its distance.
7. Enter cyan plaza marker: FOV widens (towards 78), position becomes higher/farther and aim shifts.
8. Enter pink marker near the yellow tower: FOV narrows (towards 42), camera shifts sideways
   and aims towards the landmark. Exit to smoothly blend back. HUD shows active hint/FOV.

## Requirement mapping
CameraRig.cs: follow smoothing, smoothed joystick orbit, vertical limits, automatic alignment,
lateral and diagonal feelers, downward ground feeler, radial line-of-sight ray bundle,
safety clamp after smoothing and two distance-based position/aim/FOV hints.
CameraCourse.cs: large walls, small obstacles, raised ground, landmark, hint markers and HUD.
Player.cs: camera-relative joystick movement and jump, based on original adjusted controller.
Camera blocker collision bit is 4 (Inspector Layer 3); large bodies use mask value 5
(layer 1 for character and layer 3 for camera). Small bodies use value 1 only.
Hint influence uses smooth distance weights instead of Areas, allowed by supplied specification.
Hints can override player orbit within the zones; move out to restore ordinary orbit.

Stop game, select Camera3D, adjust exported settings in Inspector, save and run.
F2 rays are a visual debugging aid, not obstacle geometry.

## Validation / limits
Scene references, project XML, original material blocks, original model bytes and ZIP checked.
Godot and .NET SDK unavailable here: NOT compiled or play-tested in this environment.
This is a comparison implementation of supplied requirements, not an exact reproduction of
Journey's camera. Course video and textbook details were not independently reproduced.
Thin ray bundles are an approximation: test corners, ground and hint overlap locally and tune.
No course grade or full correctness is established merely because it starts successfully.

Official API references:
https://docs.godotengine.org/en/4.5/classes/class_input.html
https://docs.godotengine.org/en/4.5/classes/class_physicsrayqueryparameters3d.html
