using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamPlayer : MonoBehaviour
{
    public RawImage webcamRawImage;
    private WebCamTexture _webCamTexture;

    private void Awake()
    {
	    InitWebcam();
    }

    void InitWebcam()
    {
	    if (_webCamTexture != null)
	    {
		    webcamRawImage.texture = null;
		    _webCamTexture.Stop();
		    _webCamTexture = null;
	    }
	    WebCamDevice device = WebCamTexture.devices[0];
	    _webCamTexture = new WebCamTexture(device.name);
	    webcamRawImage.texture = _webCamTexture;
	    _webCamTexture.Play();
    }

    IEnumerator PlayWebcam()
    {
	    while (true)
	    {
		    
		    yield return null;
	    }
	    
	    yield return null;
    } 
}
