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
	
	// UI.
	int _selectedCornerIndex = -1;
	
	// Result.
	Matrix4x4 _homography;
	Material _material;
    Mesh _mesh;
	Camera cam;
	Vector2[] _userScreenPoints;

	public GameObject[] Gizs; // for debug
    
	void Awake()
	{
		cam = this.GetComponent<Camera>();

		// Start with user points at source points.
		float w = cam.pixelWidth;
		float h = cam.pixelHeight;
		_userScreenPoints = new Vector2[4];
		_userScreenPoints[0] = new Vector2( 0, 0 );
		_userScreenPoints[1] = new Vector2( 0, h );
		_userScreenPoints[2] = new Vector2( w, h );
		_userScreenPoints[3] = new Vector2( w, 0 );

		// Create material.
		_material = new Material( Shader.Find( "Hidden/HomographytCustomQuadTest" ) );

		// Create mesh.
		_mesh = new Mesh();
		_mesh.vertices = new Vector3[4];
		_mesh.uv = new Vector2[] { new Vector2(0,0), new Vector2(0,1), new Vector2( 1, 1 ), new Vector2(1,0) };
		_mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
	}


	void Update()
	{
		UpdateUserInteraction();
	}

	void OnRenderObject()
	{
		Vector3[] pts = Homography.screenPointsToWorld( cam, _userScreenPoints, 5.0f);
		_mesh.vertices = pts;

		// for( int i = 0; i < 4; i++ ){
		// 	Vector3 pnt = new Vector3( _userScreenPoints[i].x, _userScreenPoints[i].y, cam.nearClipPlane );
		// 	_mesh.vertices[i] = cam.ScreenToWorldPoint( pnt );
		// }

		Matrix4x4 wtc = cam.worldToCameraMatrix;
		Matrix4x4 proj = cam.projectionMatrix;
		Matrix4x4 adjustedProj = GL.GetGPUProjectionMatrix( proj, true );

		_material.SetMatrix( "_Matrix", adjustedProj * wtc );
		
		_material.SetPass( 0 );

		Graphics.DrawMeshNow( _mesh, Matrix4x4.zero );

	}

	void OnDrawGizmos(){
		for( int i = 0; i < 4; i++ ){
			//Debug.Log( _mesh.vertices[i] );
			//Gizmos.DrawWireSphere( _mesh.vertices[i], 0.2f); // causes error messages, when uninitialized (not playing)
		}
	}

	void UpdateUserInteraction()
	{
		Camera cam = Camera.main;

		// Get mouse position and transform it to clip space.
		Vector2 mousePosition = Input.mousePosition;

		// Select (nearest corner) & deselect.
		if( Input.GetMouseButtonDown( 0 ) ){
			float closestDist = float.MaxValue;
			for( int i = 0; i < 4; i++ ){
				float dist = Vector2.Distance( _userScreenPoints[i], mousePosition );
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
			_userScreenPoints[_selectedCornerIndex] = new Vector2( mousePosition.x, mousePosition.y );
		}
	}
}
