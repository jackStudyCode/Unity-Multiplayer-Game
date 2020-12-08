using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: can this be Enemy handler or something to update it's postion 
public class EnemySpawner : MonoBehaviour
{
   public Enemy enemy;

   void Start()
   {
      Debug.Log("EnemySpawner start");
   }

   public void UpdatePosition(PlayerPositionMessage posMessage)
   {
      if (!enemy.isActiveAndEnabled)
      {
         Debug.Log("Spawn enemy and set active");
         enemy.SetActive(true);
      }
      enemy.BufferState(posMessage);
   }
}
