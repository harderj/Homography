﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HomographyBlitTest : MonoBehaviour
{
	// Camera space: 	positions relative to the camera position and rotation.
	// Clip space: 	positions relative to a cube that contains what the camera can see: (-1,-1,-1) to (1,1,1).
	// Sensor space: 	positions relative to near clip plane (-1,-1,1) to (1,1,1)
	// To go from camera space to clip space, multiply position by _camera.projectionMatrix.

	
	// Sensor space points to be transformed.
	// Order: lower-left, upper-let, upper-right, lower-right.
	static readonly Vector3[] sensorSourcePoints = { new Vector3( -1, -1, -1), new Vector3( -1, 1, -1 ), new Vector3( 1, 1, -1 ), new Vector3( 1, -1, -1 ) };
	
	// Points set by the user in 2D at (near clip space).
	Vector3[] _sensorUserPoints = new Vector3[4];
	
	// UI.
	int _selectedCornerIndex = -1;
	
	// Result.
	Matrix4x4 _homography;

	Camera _camera;
	Material _material;
	
	
	void Awake()
	{
		// Start with user points at source points.
		sensorSourcePoints.CopyTo( _sensorUserPoints, 0 );

		// Get components.
		_camera = GetComponent<Camera>();

		// Create material.
		_material = new Material( Shader.Find( "Hidden/HomographytBlitTest" ) );
	}


	void Update()
	{
		UpdateUserInteraction();
	}


	void OnRenderImage( RenderTexture src, RenderTexture dest )
	{
		Homography.Find( _sensorUserPoints, ref _homography );
		_material.SetMatrix( "_Homography", _homography );

		Graphics.Blit( src, dest, _material, 0 );
		Graphics.Blit( src, dest, _material, 1 );
	}


	void UpdateUserInteraction()
	{
		// Get mouse position and transform it to clip space.
		Vector2 mousePosition = Input.mousePosition;
		mousePosition.Scale( new Vector2( 2/(float) _camera.pixelWidth, 2/(float) _camera.pixelHeight ) );
		mousePosition -= Vector2.one;

		// Select (nearest corner) & deselect.
		if( Input.GetMouseButtonDown( 0 ) ){
			float closestDist = float.MaxValue;
			for( int i = 0; i < 4; i++ ){
				float dist = Vector2.Distance( _sensorUserPoints[i], mousePosition );
				if( dist < closestDist ){
					closestDist = dist;
					_selectedCornerIndex = i;
				}
			}
		} else if( Input.GetMouseButtonUp( 0 ) ) {
			_selectedCornerIndex = -1;
		}

		// Update selected user point from mouse position.
		if( _selectedCornerIndex != -1 ) {
			_sensorUserPoints[_selectedCornerIndex].Set( mousePosition.x, mousePosition.y, -1 );
		}
	}
}