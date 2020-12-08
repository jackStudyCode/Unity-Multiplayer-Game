using System;
using UnityEngine;

// Based on http://answers.unity.com/answers/956580/view.html
[System.Serializable]
public struct SerializableVector3
{
   public float x;

   public float y;

   public float z;

   public SerializableVector3(float rX, float rY, float rZ)
   {
      x = rX;
      y = rY;
      z = rZ;
   }

   // Returns a string representation of the object
   public override string ToString()
   {
      return String.Format("[{0}, {1}, {2}]", x, y, z);
   }

   // Automatic conversion from SerializableVector3 to Vector3
   public static implicit operator Vector3(SerializableVector3 rValue)
   {
      return new Vector3(rValue.x, rValue.y, rValue.z);
   }

   // Automatic conversion from Vector3 to SerializableVector3
   public static implicit operator SerializableVector3(Vector3 rValue)
   {
      return new SerializableVector3(rValue.x, rValue.y, rValue.z);
   }
}
