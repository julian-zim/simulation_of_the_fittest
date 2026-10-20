using System;
using UnityEngine;

public static class Utils
{

    public static double VectorToAngle(Vector3 vector)
    {
        Vector3 sinusVector = new Vector3(0, 0, vector.z);
        double sinus = sinusVector.magnitude;
        double angleRad = Math.Asin(sinus);
        double angleDeg = 180 / Math.PI * angleRad;
        if (vector.z < 0)
            angleDeg *= -1;
        if (vector.x < 0)
            angleDeg += (90 - angleDeg) * 2;

        return angleDeg;
    }

    public static Vector3 AngleToVector(double angleDeg)
    {
        double angleRad = angleDeg * Math.PI / 180;
        double sinus = Math.Sin(angleRad);
        double cosinus = Math.Cos(angleRad);
        Vector3 sinusVector = new Vector3(0, 0, (float) sinus);
        Vector3 cosinusVector = new Vector3((float) cosinus, 0, 0);
        Vector3 vector = sinusVector + cosinusVector;
        
        return Vector3.Normalize(vector);
    }
    
}
