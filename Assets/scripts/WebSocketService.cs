using UnityEngine;
using NativeWebSocket;

public class WebSocketService : Singleton<WebSocketService>
{
   private StatusController _statusController = null;
   private Menu _menu = null;
   private EnemySpawner _enemySpawner = null;

   public const string FirstToJoinOp = "0";
   public const string RequestStartOp = "1";
   public const string PlayingOp = "11";
   public const string ThrowOp = "5";
   public const string BlockHitOp = "9";
   public const string YouWonOp = "91";
   public const string YouLostOp = "92";
   public const string OpponentPos = "20";

   private bool intentionalClose = false;
   private WebSocket _websocket;
   private string _webSocketDns = "wss://YOUR_SOCKET_DNS/STAGE";

   private Rigidbody localPlayerReference;
   private string matchId;

   // TODO: move to bottom if keeping
   public string getMatchId()
   {
      return matchId;
   }

   async public void FindMatch()
   {
      _websocket.OnOpen += () =>
      {
         Debug.Log("Connection open!");
         intentionalClose = false;
         GameMessage startRequest = new GameMessage("OnMessage", RequestStartOp);
         SendWebSocketMessage(JsonUtility.ToJson(startRequest));
      };

      _websocket.OnError += (e) =>
      {
         Debug.Log("Error! " + e);
      };

      _websocket.OnClose += (e) =>
      {
         Debug.Log("Connection closed!");

         // only do this if someone quit the game session, and not for a game ending event
         if (!intentionalClose)
         {
            UnityMainThreadHelper.wkr.AddJob(() =>
            {
               _menu.Disconnected();
            });
         }

      };

      _websocket.OnMessage += (bytes) =>
      {
         Debug.Log("OnMessage!");
         string message = System.Text.Encoding.UTF8.GetString(bytes);
         Debug.Log(message.ToString());

         ProcessReceivedMessage(message);
      };

      // waiting for messages
      await _websocket.Connect();
   }

   private void ProcessReceivedMessage(string message)
   {
      GameMessage gameMessage = JsonUtility.FromJson<GameMessage>(message);

      if (gameMessage.opcode == PlayingOp)
      {
         matchId = gameMessage.uuid;
         //Debug.Log(gameMessage.uuid);

         _statusController.SetText(StatusController.Playing);
         Debug.Log("PlayingOp code");
         SendPosition(localPlayerReference.position);
      }
      else if (gameMessage.opcode == ThrowOp)
      {
         Debug.Log(gameMessage.message);
      }
      else if (gameMessage.opcode == YouWonOp)
      {
         _statusController.SetText(StatusController.YouWon);
         QuitGame();
      }
      else if (gameMessage.opcode == YouLostOp)
      {
         _statusController.SetText(StatusController.YouLost);
         QuitGame();
      }
      else if (gameMessage.opcode == OpponentPos)
      {
         // TODO: received opponent's position
         Debug.Log("Received opponent position message");
         PlayerPositionMessage posMessage = JsonUtility.FromJson<PlayerPositionMessage>(message);
         Debug.Log(posMessage.position);

         _enemySpawner.UpdatePosition(posMessage);
      } else if (gameMessage.opcode == FirstToJoinOp) {
         matchId = gameMessage.uuid;
      }
   }

   public async void SendWebSocketMessage(string message)
   {
      if (_websocket != null && _websocket.State == WebSocketState.Open)
      {
         // Sending plain text
         await _websocket.SendText(message);
      }
   }

   public void BlockHit()
   {
      GameMessage blockHitMessage = new GameMessage("OnMessage", BlockHitOp);
      SendWebSocketMessage(JsonUtility.ToJson(blockHitMessage));
   }

   public void SendPosition(Vector3 positionIn)
   {
      // Debug.Log("send pos");
      SerializableVector3 posIn = positionIn;
      GameMessage posMessage = new PlayerPositionMessage("OnMessage", OpponentPos, posIn, 0);
      posMessage.uuid = matchId;
      //Debug.Log(posMessage.uuid);
      SendWebSocketMessage(JsonUtility.ToJson(posMessage));
   }

   public void SetLocalPlayerRef(Rigidbody localPlayerReferenceIn)
   {
      localPlayerReference = localPlayerReferenceIn;
   }

   public async void QuitGame()
   {
      intentionalClose = true;
      _menu.ShowFindMatch();
      await _websocket.Close();
   }

   private async void OnApplicationQuit()
   {
      await _websocket.Close();
   }

   void Start()
   {
      Debug.Log("Websocket start");
      intentionalClose = false;
      _statusController = FindObjectOfType<StatusController>();
      _menu = FindObjectOfType<Menu>();
      _enemySpawner = FindObjectOfType<EnemySpawner>();

      _websocket = new WebSocket(_webSocketDns);
      FindMatch();
   }

   void Update()
   {
#if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
#endif
   }

   public void init() { }

   protected WebSocketService() { }
}
