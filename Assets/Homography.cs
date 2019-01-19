/* Made by Jacob Harder 17-01-2019
   Copyright (C) 2019 Jacob Harder

   Permission is hereby granted, free of charge, to any person obtaining a copy of
   this software and associated documentation files (the "Software"), to deal in
   the Software without restriction, including without limitation the rights to
   use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
   the Software, and to permit persons to whom the Software is furnished to do so,
   subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.

   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
   FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
   COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
   CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE    
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Homography {
    
    // Sensor space points to be transformed.
    // Order: lower-left, upper-let, upper-right, lower-right.
    static readonly Vector3[] sensorSourcePoints = { new Vector3( -1, -1, -1), new Vector3( -1, 1, -1 ), new Vector3( 1, 1, -1 ), new Vector3( 1, -1, -1 ) };
    
    /// <summary>
    /// Finds the transformation of a quads corner positions, from being projected onto the camera sensor (near clip space)
    /// to 3D space in front of the camera.
    /// Assuming:
    /// - four sensor space points (x,y,-1) in order lower-left, upper-let, upper-right, lower-right.
    /// - ...
    /// </summary>
    public static void Find_( Vector2[] nearClipPos, Matrix4x4 cameraToWorld, ref Matrix4x4 homography )
    {
	if (nearClipSpacePoints.Length < 4) return;
	
	Vector4[] dest = new Vector4[4];
	Matrix4x4 dest_mat = Matrix4x4.identity;
	Vector4[] sour = new Vector4[4];
	Matrix4x4 sour_mat = Matrix4x4.identity;
	Matrix4x4 eq_mat = Matrix4x4.identity;
	for (int i = 0; i < 4; i ++) {
	    dest[i] = new Vector4( nearClipSpacePoints[i].x,
				   nearClipSpacePoints[i].y,
				   nearClipSpacePoints[i].z, 1 );
	    sour[i] = new Vector4( sensorSourcePoints[i].x,
				   sensorSourcePoints[i].y,
				   sensorSourcePoints[i].z, 1 );
	    dest_mat.SetColumn( i, dest[i] );
	    sour_mat.SetColumn( i, sour[i] );
	    float sign = 1;
	    if( i==1 ) sign = -1;
	    eq_mat.SetColumn( i, sign * dest[i] );
	}
	dest_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	dest_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	sour_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	sour_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	eq_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	eq_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );

	Matrix4x4 eq_mat_inv = eq_mat.inverse;

	Vector3 alphas = MxV4( eq_mat_inv, dest[3] );
	float alpha4 = 1.0f;
	
	Vector4[] dest3d = new Vector4[4];
	for( int i = 0; i < 3; i ++ ) {
	    dest3d[i] = dest[i]*alphas[i]*alpha4;
	}
	dest3d[3] = dest[3]*alpha4;

	Matrix4x4 dest3d_mat = Matrix4x4.identity;
	for( int i = 0; i < 4; i ++ ) dest3d_mat.SetColumn( i, dest3d[i] );
	dest3d_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	dest3d_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
        
	homography = dest3d_mat * sour_mat.inverse;

	}
    
    static Vector4 MxV4( Matrix4x4 mat, Vector4 vec )
    {	    
	return new Vector4 ( mat[0, 0] * vec[0] + mat[0, 1] * vec[1] + mat[0, 2] * vec[2] + mat[0, 3] * vec[3],
			     mat[1, 0] * vec[0] + mat[1, 1] * vec[1] + mat[1, 2] * vec[2] + mat[1, 3] * vec[3],
			     mat[2, 0] * vec[0] + mat[2, 1] * vec[1] + mat[2, 2] * vec[2] + mat[2, 3] * vec[3],
			     mat[3, 0] * vec[0] + mat[3, 1] * vec[1] + mat[3, 2] * vec[2] + mat[3, 3] * vec[3] );
    }

    public static void Find( Vector3[] nearClipSpacePoints, ref Matrix4x4 homography )
    {
	if (nearClipSpacePoints.Length < 4) return;

	Vector4[] dest = new Vector4[4];
	Matrix4x4 dest_mat = Matrix4x4.identity;
	Vector4[] sour = new Vector4[4];
	Matrix4x4 sour_mat = Matrix4x4.identity;
	Matrix4x4 eq_mat = Matrix4x4.identity;
	for (int i = 0; i < 4; i ++) {
	    dest[i] = new Vector4( nearClipSpacePoints[i].x,
				   nearClipSpacePoints[i].y,
				   nearClipSpacePoints[i].z, 1 );
	    sour[i] = new Vector4( sensorSourcePoints[i].x,
				   sensorSourcePoints[i].y,
				   sensorSourcePoints[i].z, 1 );
	    dest_mat.SetColumn( i, dest[i] );
	    sour_mat.SetColumn( i, sour[i] );
	    float sign = 1;
	    if( i==1 ) sign = -1;
	    eq_mat.SetColumn( i, sign * dest[i] );
	}
	dest_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	dest_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	sour_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	sour_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	eq_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
	eq_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );

	Matrix4x4 eq_mat_inv = eq_mat.inverse;

	Vector3 alphas = MxV4( eq_mat_inv, dest[3] );
	float alpha4 = 1.0f;
	
	Vector4[] dest3d = new Vector4[4];
	for( int i = 0; i < 3; i ++ ) {
	    dest3d[i] = dest[i]*alphas[i]*alpha4;
	}
	dest3d[3] = dest[3]*alpha4;

	Matrix4x4 dest3d_mat = Matrix4x4.identity;
	for( int i = 0; i < 4; i ++ ) dest3d_mat.SetColumn( i, dest3d[i] );
	dest3d_mat.SetRow( 3, new Vector4( 0, 0, 0, 1 ) );
	dest3d_mat.SetColumn( 3, new Vector4( 0, 0, 0, 1 ) );
        
	homography = dest3d_mat * sour_mat.inverse;
    }
}
