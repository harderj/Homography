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
    static readonly Vector3[] sensorSourcePoints = { new Vector3( -1, -1, -1 ),
						     new Vector3( -1,  1, -1 ),
						     new Vector3(  1,  1, -1 ),
						     new Vector3(  1, -1, -1 ) };

    public static void Find_( Vector3[] nearClipSpacePoints,
			      ref Matrix4x4 homography )
    {
	if( nearClipSpacePoints.Length < 4 ) return;
	Vector2[] nc = new Vector2[4];
	for( int i=0; i<4; i++ )
	    nc[i] = new Vector2( nearClipSpacePoints[i].x,
				 nearClipSpacePoints[i].y );
	nearClipToMatrix( nc, ref homography );
    }

    public static void nearClipToMatrix( Vector2[] nearClipSpacePoints,
					 ref Matrix4x4 homography )
    {
	nearClipToMatrix( nearClipSpacePoints,
			   5.0f, // very arbitrary
			   sensorSourcePoints,
			   ref homography );
    }

    public static void nearClipToMatrix( Vector2[] nearClipSpacePoints,
					  float length,
					  Vector3[] source,
					  ref Matrix4x4 homography )
    {
	if( nearClipSpacePoints.Length < 4
	    || length <= 0
	    || source.Length < 3 ) return; // bail
	Vector3[] pts = new Vector3[4];
	nearClipToCamera( nearClipSpacePoints, length, ref pts );
        homography = pointsToTransformation( source, pts );
    }

    /// <summary>
    /// Finds the (3d) world space points, from (2d) points oberversed
    /// by the camera (in near clip space)
    /// 
    /// Assuming:
    /// - four sensor space points p[i]=(x[i],y[i],-1) in order
    ///   lower-left, upper-left, upper-right, lower-right, i=0..3
    /// - the world space points form a parallelogram, that is:
    ///   p[1] - p[0] = p[2] - p[3]
    /// </summary>
    public static void nearClipToCamera( Vector2[] nearClipPos,
					 float length,
					 ref Vector3[] pts )
    {	
	Vector2[] nc = nearClipPos; // alias
	int n = nc.Length;
	if (n < 4 || pts.Length < 4) return;
        
	Matrix4x4 em = Matrix4x4.identity; // set up system of eq.
	em[0,0] = nc[0].x; em[0,1] = -nc[1].x; em[0,2] = nc[2].x; em[0,3] = 0;
	em[1,0] = nc[0].y; em[1,1] = -nc[1].y; em[1,2] = nc[2].y; em[1,3] = 0;
	em[2,0] =      -1; em[2,1] =        1; em[2,2] =      -1; em[2,3] = 0;
	em[3,0] =       0; em[3,1] =        0; em[3,2] =       0; em[3,3] = 1;
	Matrix4x4 em_inv = em.inverse;
	Vector4 sc = em_inv.MultiplyPoint( new Vector4( nc[3].x, nc[3].y, -1, 0 ) );
        Vector3 tv = new Vector3( sc.x * nc[0].x - sc.y * nc[1].x, sc.x * nc[0].y - sc.y * nc[1].y, sc.y - sc.x );
	float scale = length / tv.magnitude;
	for ( int i = 0; i < 4; i++ ) pts[i] = new Vector3( nc[i].x, nc[i].y, -1 );
        pts[0] = pts[0] * sc.x * scale;
	pts[1] = pts[1] * sc.y * scale;
	pts[2] = pts[2] * sc.z * scale;
	pts[3] = pts[3] * scale;
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

    public static Matrix4x4 pointsToMatrix( Vector3[] pts ) {
	Matrix4x4 m = Matrix4x4.identity;
	for (int i = 0; i < 3 && i < pts.Length; i ++)
	    m.SetColumn( i, new Vector4( pts[i].x, pts[i].y, pts[i].z, 0) );
	if( pts.Length > 3 )
	    m.SetColumn( 3, new Vector4( pts[3].x, pts[3].y, pts[3].z, 1 ) );
	return m;
    }

    public static Matrix4x4 pointsToTransformation( Vector3[] source,
						    Vector3[] destination ){
	Matrix4x4 s = pointsToMatrix( source );
	Matrix4x4 d = pointsToMatrix( destination );
	return d * s.inverse;
    }

    static Vector4 MxV4( Matrix4x4 mat, Vector4 vec )
    {	    
	return new Vector4 ( mat[0, 0] * vec[0] + mat[0, 1] * vec[1] + mat[0, 2] * vec[2] + mat[0, 3] * vec[3],
			     mat[1, 0] * vec[0] + mat[1, 1] * vec[1] + mat[1, 2] * vec[2] + mat[1, 3] * vec[3],
			     mat[2, 0] * vec[0] + mat[2, 1] * vec[1] + mat[2, 2] * vec[2] + mat[2, 3] * vec[3],
			     mat[3, 0] * vec[0] + mat[3, 1] * vec[1] + mat[3, 2] * vec[2] + mat[3, 3] * vec[3] );
    }
}
