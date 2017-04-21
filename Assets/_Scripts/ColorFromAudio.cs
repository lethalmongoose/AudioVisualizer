using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorFromAudio : ManipulatedByAudio {

    [SerializeField]
    protected Color endColor;
    [SerializeField]
    [Tooltip("Interpolates from the initial color to this end color based on audio data")]
    protected float colorChangeSpeed = 1f;
    //Take the initial color from the base material's color
    protected Color initialColor;

    private Renderer rend;

    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        initialColor = rend.material.color;        
    }
	
	// Update is called once per frame
	protected override void FixedUpdate () {
        base.FixedUpdate();
        if (AudioVisualizer.audioIsPlaying())
        {
            UpdateColor();
        }
    }

    protected virtual void UpdateColor()
    {
        float r, g, b, a;
        //Interpolate between initial color and end color based on current audio data, making sure not to go passed the end color
        //If buffer is applied
        if (bufferDecreaseSpeed > 0 || bufferIncreaseSpeed > 0)
        {
            r = initialColor.r + (endColor.r - initialColor.r) * valueBuffer * colorChangeSpeed;
            if ((r > endColor.r && endColor.r > initialColor.r) || (r < endColor.r && endColor.r < initialColor.r))
                r = endColor.r;
            g = initialColor.g + (endColor.g - initialColor.g) * valueBuffer * colorChangeSpeed;
            if ((g > endColor.g && endColor.g > initialColor.g) || (g < endColor.g && endColor.g < initialColor.g))
                g = endColor.g;
            b = initialColor.b + (endColor.b - initialColor.b) * valueBuffer * colorChangeSpeed;
            if ((b > endColor.b && endColor.b > initialColor.b) || (b < endColor.b && endColor.b < initialColor.b))
                b = endColor.b;
            a = initialColor.a + (endColor.a - initialColor.a) * valueBuffer * colorChangeSpeed;
            if ((a > endColor.a && endColor.a > initialColor.a) || (a < endColor.a && endColor.a < initialColor.a))
                a = endColor.a;
        }
        //no buffer
        else
        {
            float raw = AudioVisualizer.getRawAudioRange(range);
            r = (endColor.r - initialColor.r) * raw * colorChangeSpeed;
            if ((r > endColor.r && endColor.r > initialColor.r) || (r < endColor.r && endColor.r < initialColor.r))
                r = endColor.r;
            g = (endColor.g - initialColor.g) * raw * colorChangeSpeed;
            if ((g > endColor.g && endColor.g > initialColor.g) || (g < endColor.g && endColor.g < initialColor.g))
                g = endColor.g;
            b = (endColor.b - initialColor.b) * raw * colorChangeSpeed;
            if ((b > endColor.b && endColor.b > initialColor.b) || (b < endColor.b && endColor.b < initialColor.b))
                b = endColor.b;
            a = (endColor.a - initialColor.a) * raw * colorChangeSpeed;
            if ((a > endColor.a && endColor.a > initialColor.a) || (a < endColor.a && endColor.a < initialColor.a))
                a = endColor.a;
        }
        rend.material.color = new Color(r, g, b, a);
    }
}
