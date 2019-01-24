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

    public static void Find( Vector3[] nearClipSpacePoints,
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
	Vector3[] pts = new Vector3[4];
	nearClipToCamera( nearClipSpacePoints, ref pts );
        homography = pointsToTransformation( sensorSourcePoints, pts );
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
					 ref Vector3[] pts )
    {	
	Vector2[] nc = nearClipPos; // alias
	int n = nc.Length;
	if (n < 4 || pts.Length < 4) return; // bail
        // set up system of eq. (p[1] - p[0] = p[2] - p[3])
	Matrix4x4 em = Matrix4x4.identity; 
	em[0,0] = nc[0].x; em[0,1] = -nc[1].x; em[0,2] = nc[2].x; em[0,3] = 0;
	em[1,0] = nc[0].y; em[1,1] = -nc[1].y; em[1,2] = nc[2].y; em[1,3] = 0;
	em[2,0] =      -1; em[2,1] =        1; em[2,2] =      -1; em[2,3] = 0;
	em[3,0] =       0; em[3,1] =        0; em[3,2] =       0; em[3,3] = 1;
	// solve system of eq.
	// sc.x, sc.y, sx.z, 1 are the (negative) z values of p[0..3]
	// before scaling
	Matrix4x4 em_inv = em.inverse;
	Vector4 sc = em_inv.MultiplyPoint( new Vector4( nc[3].x, nc[3].y,
							-1, 0 ) );
	// return resulting Vector3's
	for ( int i = 0; i < 4; i++ )
	    pts[i] = new Vector3( nc[i].x, nc[i].y, -1 );
        pts[0] = pts[0] * sc.x;
	pts[1] = pts[1] * sc.y;
	pts[2] = pts[2] * sc.z;
	float mx = max4 ( Mathf.Abs( pts[0].z ),
			  Mathf.Abs( pts[1].z ),
			  Mathf.Abs( pts[2].z ),
			  Mathf.Abs( pts[3].z ) );
	float scale = 1 / mx;
	for ( int i = 0; i < 4; i++ )
	    pts[i] = pts[i] * scale;
    }

    public static void nearClipToCamera( Vector2[] nearClipPos,
					 float length,
					 ref Vector3[] pts )
    {	
	Vector2[] nc = nearClipPos; // alias
	int n = nc.Length;
	if (n < 4 || pts.Length < 4) return; // bail
        // set up system of eq. (p[1] - p[0] = p[2] - p[3])
	Matrix4x4 em = Matrix4x4.identity; 
	em[0,0] = nc[0].x; em[0,1] = -nc[1].x; em[0,2] = nc[2].x; em[0,3] = 0;
	em[1,0] = nc[0].y; em[1,1] = -nc[1].y; em[1,2] = nc[2].y; em[1,3] = 0;
	em[2,0] =      -1; em[2,1] =        1; em[2,2] =      -1; em[2,3] = 0;
	em[3,0] =       0; em[3,1] =        0; em[3,2] =       0; em[3,3] = 1;
	// solve system of eq.
	// sc.x, sc.y, sx.z, 1 are the (negative) z values of p[0..3] before scaling
	Matrix4x4 em_inv = em.inverse;
	Vector4 sc = em_inv.MultiplyPoint( new Vector4( nc[3].x, nc[3].y, -1, 0 ) );
	// find |p[1] - p[0]| to normalize and scale properly
        Vector3 tv = new Vector3( sc.x * nc[0].x - sc.y * nc[1].x,
				  sc.x * nc[0].y - sc.y * nc[1].y,
				  sc.y - sc.x ); // 
	float scale = length / tv.magnitude;
	// return resulting Vector3's
	for ( int i = 0; i < 4; i++ ) pts[i] = new Vector3( nc[i].x, nc[i].y, -1 );
        pts[0] = pts[0] * sc.x * scale;
	pts[1] = pts[1] * sc.y * scale;
	pts[2] = pts[2] * sc.z * scale;
	pts[3] = pts[3] * scale;
    }

    private static float max4 ( float x, float y, float z, float w ) {
	return Mathf.Max( Mathf.Max( Mathf.Max( x, y ), z ), w ); }

    private static float min4 ( float x, float y, float z, float w ) {
    	return -max4( -x, -y, -z, -w ); }

    private static Matrix4x4 pointsToMatrix( Vector3[] pts ) {
	Matrix4x4 m = Matrix4x4.identity;
	for (int i = 0; i < 3 && i < pts.Length; i ++)
	    m.SetColumn( i, new Vector4( pts[i].x, pts[i].y, pts[i].z, 0) );
	if( pts.Length > 3 )
	    m.SetColumn( 3, new Vector4( pts[3].x, pts[3].y, pts[3].z, 1 ) );
	return m;
    }

    private static Matrix4x4 pointsToTransformation( Vector3[] source,
						    Vector3[] destination ){
	Matrix4x4 s = pointsToMatrix( source );
	Matrix4x4 d = pointsToMatrix( destination );
	return d * s.inverse;
    }
}
