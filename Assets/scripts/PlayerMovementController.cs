using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementController : MonoBehaviour
{
   public Rigidbody player;
   public Rigidbody ball;
   public Transform playerCamera;
   public float maxSpeed = 10;
   public float baseBallThrust = 20.0f;

   private float _throwKeyPressedStartTime;
   private BallActionHandler _ballActionHandler;

   private Vector3 lastX, lastZ;
   private float inputHorX, inputVertY;

   void Start()
   {
      // For now just hit this variable to create the singleton
      WebSocketService.Instance.init();

      player = GetComponent<Rigidbody>();
      _ballActionHandler = new BallActionHandler(playerCamera, ball, baseBallThrust);

      // Give the websocket a reference to the object so it can know where its position is
      WebSocketService.Instance.SetLocalPlayerRef(player);
   }

   void Update()
   {
      inputHorX = Input.GetAxis("Horizontal");
      inputVertY = Input.GetAxis("Vertical");
      // actual player update is performed in FixedUpdate

      if (Input.GetMouseButtonDown(0))
      {
         _throwKeyPressedStartTime = Time.time;
      }

      if (Input.GetMouseButtonUp(0))
      {

         // allows us to click the button with over it with the mouse
         if (EventSystem.current.IsPointerOverGameObject())
            return;

         _ballActionHandler.ThrowBall(player.transform.position, player.transform.forward, _throwKeyPressedStartTime);
      }
   }

   void FixedUpdate() {
      PlayerMovement(inputHorX, inputVertY);
   }

   // TODO: maybe don't call the SendPosition when there isn't any change in movement - rethink this
   // TODO: Maybe don't starting pushing out these until game has started
   void PlayerMovement(float x, float y)
   {

      Vector3 playerMovement = new Vector3(x, 0f, y) * maxSpeed * Time.deltaTime;

      // TODO: This doesn't seem to be useful anymore as our actual player updates are applied here, and this would prevent movement, remove.
      // if (lastX.x == playerMovement.x && lastZ.z == playerMovement.z)
      // {
      //    return;
      // }
      // Debug.Log("Position change");

      // lastX.x = playerMovement.x;
      // lastZ.z = playerMovement.z;

      transform.Translate(playerMovement, Space.Self);

      WebSocketService.Instance.SendPosition(transform.position);
   }
}
