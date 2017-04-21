using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithAudio : ManipulatedByAudio
{
    [Tooltip("Should this scale in the X direction based on 'scaleFactor.x'")]
    public bool scaleX = false;
    [Tooltip("Should this scale in the Y direction based on 'scaleFactor.y'")]
    public bool scaleY = true;
    [Tooltip("Should this scale in the Z direction based on 'scaleFactor.z'")]
    public bool scaleZ = false;
    [Tooltip("Amount to scale based on xyz component")]
    public Vector3 scaleFactor = Vector3.one;

    protected Vector3 initialScale;

    protected virtual void Start ()
    {
        initialScale = transform.localScale;
	}
	
	// Update is called once per frame
	protected override void FixedUpdate ()
    {
        base.FixedUpdate();
		if(AudioVisualizer.audioIsPlaying())
        {
            UpdateScale();
        }
	}

    /// <summary>
    /// Scale x, y, and/or z based on input settings
    /// </summary>
    protected virtual void UpdateScale()
    {
        //Check if any scaling occurs
        if (scaleX || scaleY || scaleZ)
        {
            Vector3 currentScale = initialScale;
            if (scaleX)
            {
                //If buffer is applied
                if(bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentScale.x += valueBuffer * scaleFactor.x;
                }
                //no buffer
                else
                {
                    currentScale.x += AudioVisualizer.getRawAudioRange(range) * scaleFactor.x;
                }
                
            }
            if(scaleY)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentScale.y += valueBuffer * scaleFactor.y;
                }
                //no buffer
                else
                {
                    currentScale.y += AudioVisualizer.getRawAudioRange(range) * scaleFactor.y;
                }
            }
             if (scaleZ)
            {
                //If buffer is applied
                if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
                {
                    currentScale.z += valueBuffer * scaleFactor.z;
                }
                //no buffer
                else
                {
                    currentScale.z += AudioVisualizer.getRawAudioRange(range) * scaleFactor.z;
                }
            }
            transform.localScale = currentScale;
        }
    }
}
