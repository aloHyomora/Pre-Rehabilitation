using System;
using UnityEngine;

public class ScreenFixedResolution : MonoBehaviour
{
	private void Awake()
	{
		SetResolution();
	}

	// 해상도 고정
	public void SetResolution()
	{
		int setWidth = 1920;
		int setHeight = 1080;
		
		// 해상도를 설정값에 따라 변경
		// 3번째 파라미터는 풀스크린 모드 설정 true : 풀스크린, false : 창모드
		Screen.SetResolution(setWidth,setHeight, false);
	}
}
