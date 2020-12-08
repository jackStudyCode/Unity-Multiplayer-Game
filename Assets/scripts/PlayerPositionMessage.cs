// TODO: consider making different objects for message out vs message in

[System.Serializable]
public class PlayerPositionMessage : GameMessage
{
   public SerializableVector3 position;

   public PlayerPositionMessage(string actionIn, string opcodeIn, SerializableVector3 positionIn, double timestamp)
      : base(actionIn, opcodeIn)
   {
      position = positionIn;
   }
}
