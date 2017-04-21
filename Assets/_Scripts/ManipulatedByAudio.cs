using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatedByAudio : MonoBehaviour
{
    [Tooltip("Select the audio range that will affect the values used for current manipulation")]
    public AudioRange range = AudioRange.SubBass;
    [Tooltip("How quickly to scale up if actual value is greater than buffer (scale/s)")]
    public float bufferIncreaseSpeed = 1f;
    [Tooltip("How quickly to scale down if actual value is smaller than buffer (scale/s)")]
    public float bufferDecreaseSpeed = 1f;

    public bool scaleDecreaseWithTime = false;
    public bool scaleIncreaseWithTime = false;
    public bool easeOutDecrease = false;
    public bool easeOutIncrease = false;

    [SerializeField]
    protected float valueBuffer = 0f;
    protected float timeSinceLastIncrease = 0f;
    protected float timeSinceLastDecrease = 0f;

    protected virtual void FixedUpdate()
    {
        if (AudioVisualizer.audioIsPlaying())
        {
            //Check if buffer is being used
            if (bufferIncreaseSpeed > 0 || bufferDecreaseSpeed > 0)
            {
                UpdateBuffer();
            }
        }
    }

    protected virtual void UpdateBuffer()
    {
        float currentValue = AudioVisualizer.getRawAudioRange(range);
        float sqrtDifference = Mathf.Sqrt(Mathf.Abs(currentValue - valueBuffer));

        if (currentValue < valueBuffer && bufferDecreaseSpeed != 0)
        {
            float decreaseAmount = bufferDecreaseSpeed;

            if(scaleDecreaseWithTime)
            {
                timeSinceLastIncrease += Time.deltaTime;
                decreaseAmount *= timeSinceLastIncrease;
            }
            else
                decreaseAmount *= Time.deltaTime;

            if (easeOutDecrease)
                decreaseAmount *= sqrtDifference;

            valueBuffer -= decreaseAmount;

            if (scaleIncreaseWithTime)
                timeSinceLastDecrease = 0f;

            if (valueBuffer < currentValue)
            {
                valueBuffer = currentValue;
            }
        }
        else if(currentValue > valueBuffer && bufferIncreaseSpeed != 0)
        {
            float increaseAmount = bufferIncreaseSpeed;   

            if (scaleDecreaseWithTime)
                timeSinceLastIncrease = 0f;

            if (scaleIncreaseWithTime)
            {
                timeSinceLastDecrease += Time.deltaTime;
                increaseAmount *= timeSinceLastDecrease;
            }
            else
                increaseAmount *= Time.deltaTime;

            if (easeOutIncrease)
                increaseAmount *= sqrtDifference;

            valueBuffer += increaseAmount;

            if (valueBuffer > currentValue)
            {
                valueBuffer = currentValue;
            }
        }
    }

}
