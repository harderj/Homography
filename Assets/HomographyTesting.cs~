﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomographyTesting : MonoBehaviour
{
    // Camera space: 	positions relative to the camera position and rotation.
    // Clip space: 	positions relative to a cube that contains what the camera can see: (-1,-1,-1) to (1,1,1).
    // Sensor space: 	positions relative to near clip plane (-1,-1,1) to (1,1,1)
    // To go from camera space to clip space, multiply position by _camera.projectionMatrix.

    // The camera, set in the Unity inspector.
    [SerializeField] Camera _camera;
    
    // Sensor space points to be transformed.
    // Order: lower-left, upper-let, upper-right, lower-right.
    static readonly Vector3[] sensorSourcePoints = { new Vector3( -1, -1, -1), new Vector3( -1, 1, -1 ), new Vector3( 1, 1, -1 ), new Vector3( 1, -1, -1 ) };

    // Points set by the user in 2D at (near clip space).
    Vector3[] _sensorUserPoints = new Vector3[4];

    // UI.
    int _selectedCornerIndex = -1;

    // Result.
    Matrix4x4 _homography;
    
    
    void Awake()
    {
	// Start with user points at source points.
	sensorSourcePoints.CopyTo( _sensorUserPoints, 0 );
    }
    
    
    void Update()
    {
	// Interaction.
	UpdateUserInteraction();

	// Update homography.
	Homography.Find( _sensorUserPoints,  _camera.projectionMatrix, ref _homography );
    }
    
    
    void OnDrawGizmos()
    {
	if( !Application.isPlaying ) return;

	// Draw everything in camera space.
	Gizmos.matrix = _camera.worldToCameraMatrix.inverse;
	Gizmos.color = Color.green;

	// Transform user points from clip space to camera space and draw.
	for( int i = 0; i < 4; i++ )
	{
	    // The projection matrix transforms points from camera space to clip space, so we use the inverse.
	    Vector3 position = _sensorUserPoints[i]; //_camera.projectionMatrix.inverse.MultiplyPoint( _sensorUserPoints[i] );
	    Gizmos.DrawWireSphere( position, 0.4f );
	}

	// Transform source points from clip space to camera space (directly using homography) and draw.
	for( int i = 0; i < 4; i++ ) {
	    Vector3 position = _homography.MultiplyPoint( sensorSourcePoints[i] );
	    // Vector3 position = _homography.GetColumn(i); // proof of concept - worked!
	    Gizmos.DrawSphere( position, 0.3f );
	}
    }
    
    void UpdateUserInteraction()
    {
	// Get mouse position and transform it to clip space.
	Vector2 mousePosition = Input.mousePosition;
	mousePosition.Scale( new Vector2( 2/(float) _camera.pixelWidth, 2/(float) _camera.pixelHeight ) );
	mousePosition -= Vector2.one;

	// Select (nearest corner) & deselect.
	if( Input.GetMouseButtonDown( 0 ) ) {
	    float closestDist = float.MaxValue;
	    for( int i = 0; i < 4; i++ ) {
		float dist = Vector2.Distance( _sensorUserPoints[i], mousePosition );
		if( dist < closestDist ) {
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
