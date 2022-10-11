This is a short simulation to demonstate Artifical Intelligence movement and pathfinding 

Behaviour Characteristics

Tagger
●	Pursues closest untagged character
●	Tags untagged character when within tagging range
●	Switches target to closest untagged character 
  ○	Upon successful tag
  ○	After a periodic timer completes and there is a closer untagged character than the current target
●	Rotates to face the current target

Other
●	If there are no tagged characters
  ○	Do nothing
  ○	When targeted, rotate away and flee from the tagger
●	If there are other tagged characters
  ○	Target, move and rotate towards closest tagged character 
  ○	If targeted by tagger, rotate towards and flee from the tagger
  ○	unless they are the only remaining untagged character, (in that case do above)

Once all non tagger characters are tagged, character positions are randomized and the last tagged character is the new tagger

3D Models created by me
Animations are from mixamo
