using UnityEditor.Animations;
using Game.MyPlayer;
using UnityEngine;

[ExecuteAlways]
public class PlayerSpawner : MonoBehaviour
{
    public bool PlayerSpawned;
    public GameObject Player;
   // public GameObject Camera;
    public GameObject SpawnPoint;

    public void SpawnPlayer()
    {
        if (PlayerSpawned) return;
        else PlayerSpawned = true;

        GameObject playerInstance = Instantiate(Player, transform.position, Quaternion.identity);
      //  GameObject cameraInstance = Instantiate(Camera, transform.position, Quaternion.identity);
        GameObject spawnPointInstance = Instantiate(SpawnPoint, transform.position, Quaternion.identity);

        //  -----   Player(Camera)  ----- 
        // create veriable stateMachine
        PlayerStateMachine stateMachine = playerInstance.GetComponent<PlayerStateMachine>();

        // store Camera Components in the Player State Machine
       // stateMachine.PlayerCamera = cameraInstance.GetComponent<PlayerCamera>();      /////////// commented out line number 27
       // stateMachine.CameraTransform = cameraInstance.transform;

        //  -----   Player(SpawnPoint)  -----
        // store the Spawn Point in the Player State Machine
        stateMachine.SpawnPoint = spawnPointInstance.transform;           /////////// commented out line number 32

        //  -----   Camera(Player)  -----
        // create veriable playerCamera
      //  PlayerCamera playerCamera = cameraInstance.GetComponent<PlayerCamera>();

        // store Player's components in the Camera 
       // playerCamera.stateMachine = stateMachine;
      //  playerCamera.StandingCameraTarget = stateMachine.StandingCameraTarget;  /////////// commented out line number 40
      //  playerCamera.CrouchedCameraTarget = stateMachine.CrouchedCameraTarget; /////////// commented out line number 41
      //  playerCamera.ThirdPersonBody = stateMachine.ThirdPersonBody; /////////// commented out line number 42

    }
}
