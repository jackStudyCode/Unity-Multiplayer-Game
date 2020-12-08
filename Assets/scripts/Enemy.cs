using UnityEngine;
using System;
using System.Collections;

public class Enemy : MonoBehaviour
{
   private Vector3 lastReceivedPosition;
   private long lastInterpolationTime = 0;

   // we'll keep a buffer of 20 network states
   PlayerPositionMessage[] stateBuffer = new PlayerPositionMessage[20];
   int stateCount = 0; // how many states have been recorded

   // how far back to rewind interpolation?
   // Right now I set it at 1 second, try 100 for 1/10 of a second
   // TODO: setting this to 100 doee NOT see to always work on the PC.  Maybe it's framerate related? maybe the interpolation varies based on that?
   // - must be a way to set some coefficient
   // - the effects of this must be investigated more
   // - maybe there's a corralation between this and speed of player, as it keeps jumping back when it interpolates... like it's getting out of date data..?
   private long InterpolationBackTime = 100; //1000;

   // save new state to buffer
   public void BufferState(PlayerPositionMessage state)
   {
      // shift buffer contents to accommodate new state
      for (int i = stateBuffer.Length - 1; i > 0; i--)
      {
         stateBuffer[i] = stateBuffer[i - 1];
      }

      // save state to slot 0
      stateBuffer[0] = state;

      // increment state count
      stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);
   }

   void Update()
   {
      if (stateCount == 0) return; // no states to interpolate

      long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

      // the length of time we constrain the position updates to, so over X time, we may get Y updates
      long interpolationTime = currentTime - InterpolationBackTime;


      // Debug.Log("Current: " + currentTime);
      // Debug.Log("Interpolation: " + interpolationTime);
      // Debug.Log("State: " + stateBuffer[0].timestamp);
      // Debug.Log("if ( " + stateBuffer[0].timestamp + " > " + interpolationTime + " ) then its good");

      // the latest packet is newer than interpolation time - we have enough packets to interpolate
      // once we receive a position state that's timestamped greater than our interpolation time constraint,
      // we're ready to interpolation the characters movement
      //if (stateBuffer[0].timestamp > interpolationTime)

      if (lastInterpolationTime == 0) {
         lastInterpolationTime = (long) stateBuffer[0].timestamp - 100;
      }

      // if the time since we last interpolated plus the interpolate duration is greater than our latest state
      if (stateBuffer[0].timestamp - lastInterpolationTime >= 100)
      {
         Debug.Log("inside");

         StartCoroutine(LerpPosition(stateBuffer[0].position, lastInterpolationTime, (long) stateBuffer[0].timestamp));//interpolationTime, (long) stateBuffer[0].timestamp));//(long) stateBuffer[0].timestamp, currentTime));//interpolationTime, currentTime);
         
         lastInterpolationTime = (long) stateBuffer[0].timestamp;

         // for (int i = 0; i < stateCount; i++)
         // {
         //    Debug.Log("FOR");
         //    // find the closest state that matches network time, or use oldest state
         //    // now that we received a position update that greater than the interpolation time,
         //    // find the next position update that's timestamped earlier than our interpolation time.
         //    if (stateBuffer[i].timestamp <= interpolationTime || i == stateCount - 1)
         //    {
         //       // the state closest to network time
         //       PlayerPositionMessage lhs = stateBuffer[i];

         //       // the state one slot newer
         //       // TODO: but maybe not newer than interpolationTime?? No idea, just throwing this out.. 
         //       // i guess if the rhs timestamp someohow was greater than the interpolation time, what could that mean?
         //       PlayerPositionMessage rhs = stateBuffer[Mathf.Max(i - 1, 0)];

         //       // use time between lhs and rhs to interpolate
         //       // TODO: consider adding this * 10 as a way to make up for speed: 
         //       // Note the formula for distCovered and reanalyze the algorithim: https://docs.unity3d.com/ScriptReference/Vector3.Lerp.html
         //       // - does adding * Time.deltaTime make it speed per second?
         //       double length = (rhs.timestamp - lhs.timestamp);// * 10f * Time.deltaTime;
         //       float t = 0f;
         //       if (length > 0.0001)
         //       {
         //          t = (float)((interpolationTime - lhs.timestamp) / length);
         //       }

         //       // lerp: https://docs.unity3d.com/ScriptReference/Vector3.Lerp.html The second example explains it pretty well
         //       transform.position = Vector3.Lerp(lhs.position, rhs.position, t);
         //       break;
         //    }
         // }
      }
   }

   // OK LEFT OFF HERE: On first try this seems to be a viable solution, it's already much smoother and I have no idea what's going on.
   // - Analyize this algorithm and make improvements - may need to readjust buffer system.
   //IEnumerator LerpPosition(Vector3 targetPosition, float duration)
   // TODO: these should be floats
   // i think endTime needs to be some index back in time in the buffer
   IEnumerator LerpPosition(Vector3 targetPosition, long startTime, long endTime) 
   {  
      Debug.Log("LerpPosition");
      //targetPosition = stateBuffer[0].position;
      float duration = endTime - startTime; //100;// endTime - startTime;
      //float durationInSeconds = duration / 1000;

      float time = 0; //startTime;//0;
      Vector3 startPosition = transform.position; 
      // maybe startPosition should be lhs? or maybe we should just use the enemy's current position
      // maybe targetrPosition should be rhs?

      Debug.Log("time: " + time + " <  duration: " + duration);
      while (time < duration)
      {
         Debug.Log("while");
         transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
         time += Time.deltaTime * 1000;
         yield return null;
      }
      Debug.Log("done while");
      transform.position = targetPosition;
   }

   public void SetActive(bool activeFlag)
   {
      gameObject.SetActive(activeFlag);
   }

   void Awake()
   {
      Debug.Log("Enemy Awake");
      gameObject.SetActive(false);
   }

   void Start()
   {
      Debug.Log("Enemy start");
      //target = GameObject.Find("enemy").transform;
   }
}
