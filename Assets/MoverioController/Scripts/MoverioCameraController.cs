﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Moverio/MoverioCameraController")]

public class MoverioCameraController : MonoBehaviour 
{
	static MoverioCameraController _Instance;
	public static MoverioCameraController Instance
	{
		get
		{
			if(_Instance == null)
			{
				Debug.Log("Please Add MoverioCameraRig Prefab To Scene!");
			}

			return _Instance;
		}
	}

	public Camera LeftEyeCam, RightEyeCam, Cam2D;

	public float PupillaryDistance = 0.05f;
    public Color BackgroundColor = Color.black;

	MoverioDisplayType _displayState;
    bool _muteState;

	void Awake()
	{
		_Instance = this;
	}

	void Start()
	{
		LeftEyeCam.aspect = RightEyeCam.aspect = Screen.width / Screen.height * 2.0f;
		SetPupillaryDistance(PupillaryDistance);
        SetBackgroundColor(BackgroundColor);
	}

	public void SetPupillaryDistance(float pDist)
	{
		PupillaryDistance = pDist;

		LeftEyeCam.transform.localPosition = new Vector3(-PupillaryDistance, 0.0f, 0.0f);
		RightEyeCam.transform.localPosition = new Vector3(PupillaryDistance, 0.0f, 0.0f);
	}

    public void SetBackgroundColor(Color bgCol)
    {
        BackgroundColor = bgCol;

        LeftEyeCam.backgroundColor = RightEyeCam.backgroundColor = Cam2D.backgroundColor = BackgroundColor;
    }

	void OnEnable()
	{
		MoverioController.OnMoverioStateChange += HandleOnMoverioStateChange;
	}

	void OnDisable()
	{
		MoverioController.OnMoverioStateChange -= HandleOnMoverioStateChange;
	}

	void HandleOnMoverioStateChange (MoverioEventType type)
	{
		switch(type)
		{
		    case MoverioEventType.Display3DOff:
			    SetCurrentDisplayType(MoverioDisplayType.Display2D);
			    break;
		    case MoverioEventType.Display3DOn:
			    SetCurrentDisplayType(MoverioDisplayType.Display3D);
			    break;
            //case MoverioEventType.MuteDisplayOff:
            //    SetCurrentMuteType(false);
            //    break;
            //case MoverioEventType.MuteDisplayOn:
            //    SetCurrentMuteType(true);
            //    break;
        }
    }

	public MoverioDisplayType GetCurrentDisplayState()
	{
		return _displayState;
	}

    public bool GetCurrentMuteState()
    {
        return _muteState;
    }

	public void SetCurrentDisplayType(MoverioDisplayType type)
	{
		_displayState = type;

		switch(_displayState)
		{
		case MoverioDisplayType.Display2D:
			LeftEyeCam.enabled = RightEyeCam.enabled = false;
			Cam2D.enabled = true;
			break;
		case MoverioDisplayType.Display3D:
			LeftEyeCam.enabled = RightEyeCam.enabled = true;
			Cam2D.enabled = false;
			break;
		}
	}

    public void SetCurrentMuteType(bool type)
    {
        _muteState = type;

        MoverioController controller = MoverioController.Instance;

        controller.MuteDisplay(_muteState);
        controller.MuteAudio(_muteState);
    }
}
