using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithAudio : ManipulatedByAudio
{
    [Tooltip("Should this rotate in the X direction based on 'rotFactor.x'")]
    public bool rotX = false;
    [Tooltip("Should this rotate in the Y direction based on 'rotFactor.y'")]
    public bool rotY = true;
    [Tooltip("Should this rotate in the Z direction based on 'rotFactor.z'")]
    public bool rotZ = false;
    [Tooltip("Amount to rotate based on xyz component")]
    public Vector3 rotFactor = Vector3.one;

    protected Vector3 intialRotation = Vector3.zero;

    // Use this for initialization
    protected virtual void Start()
    {
        intialRotation = transform.localRotation.eulerAngles;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (AudioVisualizer.audioIsPlaying())
        {
            UpdateRotation();
        }
    }

    /// <summary>
    /// Rotate x, y, and/or z based on input settings
    /// </summary>
    protected virtual void UpdateRotation()
    {
        //Check if any scaling occurs
        if (rotX || rotY || rotZ)
        {
            Vector3 currentRotation = intialRotation;
            if (rotX)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentRotation.x += valueBuffer * rotFactor.x;
                }
                //no buffer
                else
                {
                    currentRotation.x += AudioVisualizer.getRawAudioRange(range) * rotFactor.x;
                }

            }
            if (rotY)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentRotation.y += valueBuffer * rotFactor.y;
                }
                //no buffer
                else
                {
                    currentRotation.y += AudioVisualizer.getRawAudioRange(range) * rotFactor.y;
                }
            }
            if (rotZ)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentRotation.z += valueBuffer * rotFactor.z;
                }
                //no buffer
                else
                {
                    currentRotation.z += AudioVisualizer.getRawAudioRange(range) * rotFactor.z;
                }
            }
            transform.localRotation = Quaternion.Euler(currentRotation);
        }
    }
}
