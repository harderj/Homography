﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomQuadTest : MonoBehaviour
{
	// Camera space: 	positions relative to the camera position and rotation.
	// Clip space: 	positions relative to a cube that contains what the camera can see: (-1,-1,-1) to (1,1,1).
	// Sensor space: 	positions relative to near clip plane (-1,-1,1) to (1,1,1)
	// To go from camera space to clip space, multiply position by _camera.projectionMatrix.

	// Sensor space points to be transformed.
	// Order: lower-left, upper-let, upper-right, lower-right.
	static readonly Vector3[] sensorSourcePoints = {
							new Vector3( -1, -1, 0),
							 new Vector3( -1, 1, 0 ),
							 new Vector3( 1, 1, 0 ),
							 new Vector3( 1, -1, 0 ) };
	
	// Points set by the user in 2D at (near clip space).
	Vector3[] _sensorUserPoints = new Vector3[4];
	
	// UI.
	int _selectedCornerIndex = -1;
	
	// Result.
	Matrix4x4 _homography;

	Material _material;

    Mesh _mesh;
    
	
	void Awake()
	{
		// Start with user points at source points.
		sensorSourcePoints.CopyTo( _sensorUserPoints, 0 );

		// Create material.
		_material = new Material( Shader.Find( "Hidden/HomographytCustomQuadTest" ) );

		// Create mesh.
		_mesh = new Mesh();
		_mesh.vertices = new Vector3[] {
						new Vector3( -1, -1, 0 ),
						 new Vector3( -1, 1, 0 ),
						 new Vector3( 1, 1, 0 ),
						 new Vector3( 1, -1, 0 ) };
		_mesh.uv = new Vector2[] { new Vector2(0,0), new Vector2(0,1), new Vector2( 1, 1 ), new Vector2(1,0) };
		_mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };

		Camera cam = gameObject.GetComponent<Camera>();
	}


	void Update()
	{
		UpdateUserInteraction();
	}


	void OnRenderObject()
	{
		Camera cam = Camera.current;
		if( cam.cameraType != CameraType.Game ) return;
		
		Matrix4x4 tMat = Matrix4x4.identity;
		// tMat[0,0] = -1.0f;
		// tMat[1,1] = -1.0f;
		// tMat[2,2] = -1.0f;

		// Transformed quad.
		//Homography.Find( _sensorUserPoints, ref _homography );
		//_material.SetMatrix( "_Matrix", _homography );
		//Debug.Log( _homography.MultiplyPoint ( _sensorUserPoints [ 0 ] ) );
		//Debug.Log( _homography.MultiplyPoint ( _sensorUserPoints [ 1 ] ) );
		//Debug.Log( _homography.MultiplyPoint ( _sensorUserPoints [ 2 ] ) );
		//Debug.Log( _homography.MultiplyPoint ( _sensorUserPoints [ 3 ] ) );

		// Debug.Log( "0: " + _sensorUserPoints [ 0 ]  );
		// Debug.Log( "1: " + _sensorUserPoints [ 1 ]  );
		// Debug.Log( "2: " + _sensorUserPoints [ 2 ]  );
		// Debug.Log( "3: " + _sensorUserPoints [ 3 ]  );

		// _mesh.vertices = _sensorUserPoints;

		// Debug.Log( cam.worldToCameraMatrix );
		
		_material.SetPass( 0 );

		Vector3 _pos = this.transform.localPosition;

		//Debug.Log( _pos );

	        
		
		Graphics.DrawMeshNow( _mesh, Matrix4x4.zero );

		/*
		// Handles.
		for( int i = 0; i < 4; i++ ){
			//Debug.Log( _sensorUserPoints[i] );
			Matrix4x4 mat = Matrix4x4.TRS( (Vector2) _sensorUserPoints[i], Quaternion.identity, new Vector3(1,cam.aspect,1) * 0.01f );
			_material.SetMatrix( "_Matrix", mat );
			_material.SetPass( 0 );
			Graphics.DrawMeshNow( _mesh, Matrix4x4.identity );
		}
		*/
	}


	void UpdateUserInteraction()
	{
		Camera cam = Camera.main;

		// Get mouse position and transform it to clip space.
		Vector2 mousePosition = Input.mousePosition;
		mousePosition.y = cam.pixelHeight - mousePosition.y; // flip vertically
		mousePosition.Scale( new Vector2( 2/(float) cam.pixelWidth, 2/(float) cam.pixelHeight ) );
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
			_sensorUserPoints[_selectedCornerIndex].Set( mousePosition.x, mousePosition.y, 0 );
		}
	}
}
