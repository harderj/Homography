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
    
	/// <summary>
    /// Finds (3d) world space points, from (2d) points observed
    /// by the camera (in screen space) assuming the world space
	/// points form a parallelogram, that is:
    /// p[1] - p[0] = p[2] - p[3]
    /// </summary>
	public static Vector3[] screenPointsToWorld ( Camera cam, Vector2[] points )
	{
		// get points on virtual camera screen (world coordinates of camera screen)
		Vector3[] onVirtualScreen = new Vector3[4];
		for( int i = 0; i < 4; i++ )
			onVirtualScreen[i] = cam.ScreenToWorldPoint( new Vector3( points[i].x, points[i].y, cam.nearClipPlane ) );
		
		return virtualScreenPointsToWorld ( cam, onVirtualScreen );
	}

	public static Vector3[] virtualScreenPointsToWorld ( Camera cam, Vector3[] points )
	{
		Vector3[] vs = points; //alias

		// set up matrix for solving p[1] - p[0] = p[2] - p[3]
		// using 4x4-matrix since I don't want to implement 3x3-matrix inversion
		Matrix4x4 em = Matrix4x4.identity;
		em[0,0] = vs[0].x; em[0,1] = -vs[1].x; em[0,2] = vs[2].x; em[0,3] = 0;
		em[1,0] = vs[0].y; em[1,1] = -vs[1].y; em[1,2] = vs[2].y; em[1,3] = 0;
		em[2,0] = vs[0].z; em[2,1] = -vs[1].z; em[2,2] = vs[2].z; em[2,3] = 0;
		em[3,0] =       0; em[3,1] =        0; em[3,2] =       0; em[3,3] = 1;

		Vector3 scales = em.inverse.MultiplyPoint( vs[3] );

		Vector3[] result = { vs[0] * scales.x, vs[1] * scales.y, vs[2] * scales.z, vs[3] };

		return result;
	}

	public static Vector3[] screenPointsToWorld ( Camera cam, Vector2[] points, float length )
	{
		Vector3[] inPlane = screenPointsToWorld ( cam, points );

		float scale = length / (inPlane[0] - inPlane[1]).magnitude;

		Vector3[] result = new Vector3[4];

		for( int i = 0; i < 4; i++ )
			result[i] = inPlane[i] * scale;

		return result;
	}

	public static Matrix4x4 Find ( Camera cam, Vector2[] points ){
		// TODO
		return Matrix4x4.identity;
	}

	private static Matrix4x4 pointsToMatrix( Vector3[] pts ) {
		Matrix4x4 m = Matrix4x4.identity;
		for (int i = 0; i < 3 && i < pts.Length; i ++)
			m.SetColumn( i, new Vector4( pts[i].x, pts[i].y, pts[i].z, 1) );
		if( pts.Length > 3 )
			m.SetColumn( 3, new Vector4( pts[3].x, pts[3].y, pts[3].z, 1 ) );
		return m;
    }

    private static Matrix4x4 pointsToTransformation( Vector3[] source, Vector3[] destination ){
		Matrix4x4 s = pointsToMatrix( source );
		Matrix4x4 d = pointsToMatrix( destination );
		return d * s.inverse;
    }

}
