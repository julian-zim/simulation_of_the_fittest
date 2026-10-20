using System;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{

    #region Variables
    
    // constants
    private const int Accuracy = 4;
    private const double Precision = 0.00000001;
    
    // variables
    private new bool enabled;
    private float sensivity = 10f;
    private double speed = 1f;
    
    #endregion
    
    private void Update()
    {

        #region Mouse Lock
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                enabled = true;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                enabled = false;
            }
        }
        
        #endregion

        if (enabled)
        {

            #region Rotation

            transform.Rotate(-Input.GetAxis("Mouse Y") * sensivity, Input.GetAxis("Mouse X") * sensivity, 0);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            if (transform.rotation.x >= 88)
                sensivity = 0.1f;
            else
                sensivity = 1;

            #endregion

            #region Movement

            // left
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += -transform.right * (float) speed;
            }

            // right
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * (float) speed;
            }

            // foward
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z)) *
                                      (float) speed;
            }

            // back
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += Vector3.Normalize(new Vector3(-transform.forward.x, 0, -transform.forward.z)) *
                                      (float) speed;
            }

            // up
            if (Input.GetKey(KeyCode.Space))
            {
                transform.position += Vector3.up * (float) speed;
            }

            // down
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.position += Vector3.down * (float) speed;
            }

            // speed
            double scrollDelta = Input.mouseScrollDelta.y;

            if (scrollDelta > 0 && speed < 100)
            {
                double addend = Mathf.Pow(10, GetSignificance(speed));
                addend = Math.Round((float) addend, Accuracy);

                speed += addend;
                speed = Math.Round((float) speed, Accuracy);
            }

            if (scrollDelta < 0)
            {
                double subtrahend = Mathf.Pow(10, GetSignificance(speed));
                subtrahend = Math.Round((float) subtrahend, Accuracy);

                if (DoubleEqual(speed, subtrahend))
                {
                    subtrahend /= 10;
                    subtrahend = Math.Round((float) subtrahend, Accuracy);
                }

                speed -= subtrahend;
                speed = Math.Round((float) speed, Accuracy);
            }

            #endregion
            
        }

    }

    #region Helper Functions
    
    // returns the significance ("distance to the slot before the comma") of the first relevant digit (... 100.0 = 2, 10.00 = 1, 1.000 = 0, 0.100 = -1, 0.010 = -2 ...)
    private static float GetSignificance(double x) {
        
        if (x >= 1)
            return x.ToString().Length - 1;
        
        if (x < 1) {
            int counter = 0;
            while (true)
            {
                counter++;
                if (x * Mathf.Pow(10, counter) >= 1)
                    return -counter;
            }
        }
        
        return -1;
        
    }

    private static bool DoubleEqual(double a, double b)
    {
        return Mathf.Abs((float)a - (float)b) <= Precision;
    }
    
    #endregion

}
