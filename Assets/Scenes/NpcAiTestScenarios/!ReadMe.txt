Test Npc Ai behaviour and parts of behaviour
outlines of what should happen for each test:

NPCS MELEE STATE ISSUES:
	currently without proper models, animations etc... making melee weapons collider reliably with hit colliders is a pain
	and will need tweaking anyway once they are finished and so not worth putting too much time in, but when melee weapon hit collider does
	hit npc hit collider system works very well.

NPC TEST TO ADD:
	melee state testing for making melee swing animations and hits more reliable + making sure with melee weapons fleeing is ignored (unless
	fleeing when outnumbered or similar things are implemented)

	stun state and making sure on heavy impact types stun state is always entered (atm on attack from melee weapons stun
	so currently not worth adding while melee hits unrealiable)

2TeamPatrolTest:
1 team moves across the vision of another, both spotting one another and engaging till one side is dead.

4TeamEngagmentTest:
all teams should move towards the center, spotting and engaging the closest enemy in a 4 way shoot out till 1 team is left.

AnimalFleeTest:
same as FleeAndChaseTest, just wanted to ensure it worked for animals.

CoverFarAwayTest:
2 npcs spawn away from each other, 1 pair with cover infront of them that will move to and shoot at other pair, 
the other pair has cover behind them, the pair will move to cover behind them, and semi randomly shoot back while moving to cover. 
(wanted to simulate more human behaviour when cover is far away)

EatCorpseTest:
1 survivor npc should spawn in dead along with 2 zombie npcs, zombie npcs will walk to dead npc corpse (no eat anim atm).
then both will tick zombification progress up on dead corpse, (with 2 zombies takes 10s), once complete corpse dissapears
and a new zombie is spawned in (again lacks proper animations for these)

FleeAndChaseTest:
zombie spawns in flee range of survivor npc, npc should flee semi randomly away from zombie npc. flee direction should update every 2-3 seconds.
zombie will endless chase survivor as speed of them is basically equal and can never properly get in melee range + melee/hit colliders unreliable atm

InvestigateEnemyPosTest:
1 npc watches as another walks by a gape in the wall, while in view, watching npc will update move pos and chase moving npc.
once npc disspears behind the wall again, it will enter investigate state and move to its last known position (viewable when selecting npc as red line)

MovementTypesTest:
3 npcs spawn in set to each of the movement types, 1 will patrol around on a set blue patrol path, one will move randomly around itself,
another will move around randomly in a set dark blue area split into triagnles, npc will still move out of the given area to reach
a point within the area, but should never be given a point to move to outside of said area.

NpcCoverTests:
Contains 2 tests, one for showing how npcs wont use cover if not needed (engaging zombies or animals for example)
and another for showing how npcs will utilize cover (engaging other survivors for example)
In the cover test npcs will spawn see one another, seek to find cover first before shooting. (of course ones facing zombies will shoot straight away)

NpcDoorInteractTest:
1 npc will spawn and path through patrol waypoints, encountering 2 doors that when close enough npc will open.
clicking on the door in hierarchy will allow u to manually debug press interact to close/open the door.

ReactingToSoundAndHurtTest:
2 npcs spawn, one infront of the other, the one behind will shoot the one in front, on hit they will go to investigate the direction they were shot from
quickly spotting the one that shot them. (once sound is properly set up i will add a 3rd npc that will hear the gunshot and investigate)

UseConsumablesTest:
2 npcs spawn shooting at one another, one is invincible and will kill the other, then will start healing themselves.
see StatsHandler script as there health increases again (no use consumable animation or similar atm)