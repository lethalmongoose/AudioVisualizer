using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithAudio : ManipulatedByAudio
{
    [Tooltip("Should this move in the X direction based on 'moveFactor.x'")]
    public bool moveX = false;
    [Tooltip("Should this move in the Y direction based on 'moveFactor.y'")]
    public bool moveY = true;
    [Tooltip("Should this move in the Z direction based on 'moveFactor.z'")]
    public bool moveZ = false;
    [Tooltip("Amount to move based on xyz component")]
    public Vector3 moveFactor = Vector3.one;

    protected Vector3 intialPosition = Vector3.zero;

    // Use this for initialization
    protected virtual void Start ()
    {
        intialPosition = transform.position;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (AudioVisualizer.audioIsPlaying())
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// Move x, y, and/or z based on input settings
    /// </summary>
    protected virtual void UpdatePosition()
    {
        //Check if any scaling occurs
        if (moveX || moveY || moveZ)
        {
            Vector3 currentPosition = intialPosition;
            if (moveX)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentPosition.x += valueBuffer * moveFactor.x;
                }
                //no buffer
                else
                {
                    currentPosition.x += AudioVisualizer.getRawAudioRange(range) * moveFactor.x;
                }

            }
            if (moveY)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentPosition.y += valueBuffer * moveFactor.y;
                }
                //no buffer
                else
                {
                    currentPosition.y += AudioVisualizer.getRawAudioRange(range) * moveFactor.y;
                }
            }
            if (moveZ)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentPosition.z += valueBuffer * moveFactor.z;
                }
                //no buffer
                else
                {
                    currentPosition.z += AudioVisualizer.getRawAudioRange(range) * moveFactor.z;
                }
            }
            transform.position = currentPosition;
        }
    }
}
