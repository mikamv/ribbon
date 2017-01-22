// Eetu Martola, after Mathias Müller-Fischer
using UnityEngine;
using System.Collections;
using System.Collections.Generic; //needed for List

public class RibbonSolve : MonoBehaviour
{
    public bool isRed;
	public int gridWidth = 10;
	public int gridHeight = 10;
	public float gridSize = 0.5f;
	public float stiffness = 100.0f;
    public float damping = 10.0f;
    public float gravity = 5.0f;
    public float targetMatchSpeed = 0.1f;
    public Vector3 posOffset = new Vector3(0.0f, 0.0f, 0.2f);

	public float dampTangent = 0.998f;
	public float dampNormal = 0.9f;

    public bool enforcementBars = true;
	public int substeps = 2;
	public float initVel = 3.0f;
	
	private Vector3[] p_pos, p_targetPos;
	private Vector3[] p_vel;
	private Vector3[] p_force;
	private int[,] p_constraints; //list of constraints per point

	private int[,] c_ends; // endpoint particle indices
	private float[] c_len; //rest length
	private float[] c_stiff; //stiffness
	private float[] c_damp; //dampening
	private bool[] c_active;
	private int c_count;
	
	private Vector3[,] runge;
	
	private int max_constraints_per_point = 8; //inside grid points with enforcement bars has 8 constraints

    public GameObject parentObj;
    public GameObject collisionObj;
    public GameObject controllerObj;
    private Vector3 collPos;
    private Vector3 collisionCapsuleA, collisionCapsuleB;
    private CapsuleCollider collisionCapsule;
    private Transform collisionTransform;

    private SteamVR_TrackedController controller;

	public float getClosestDistance(Vector3 pos)
	{
		float minSqrDistance = float.MaxValue;
		for (int i = 0; i < p_pos.Length; i++)
		{
			float sqrDistance = (p_pos[i] - pos).sqrMagnitude;
			if (sqrDistance < minSqrDistance)
			{
				minSqrDistance = sqrDistance;
			}
		}
		return Mathf.Sqrt(minSqrDistance);
	}

	public float getClosestPoint(Vector3 pos, out Vector3 closestPosition)
	{
		closestPosition = new Vector3(1000.0f, 1000.0f, 1000.0f);
		float minSqrDistance = float.MaxValue;
		for (int i = 0; i < p_pos.Length; i++)
		{
			float sqrDistance = (p_pos[i] - pos).sqrMagnitude;
			if (sqrDistance < minSqrDistance)
			{
				minSqrDistance = sqrDistance;
				closestPosition = p_pos[i];
			}
		}
		return Mathf.Sqrt(minSqrDistance);
	}

	// Use this for initialization
	void Start ()
    {
        p_targetPos = new Vector3[gridWidth * gridHeight];
        Mesh gridmesh = createGrid (gridWidth, gridHeight, controllerObj.transform.position);

		initParticles(gridmesh);
		initConstraints(gridmesh);
		//parentObj = GameObject.Find("Sphere");
		
		GetComponent<MeshFilter>().mesh = gridmesh;
        parentObj.transform.parent = controllerObj.transform;
        collisionCapsule = collisionObj.GetComponent<CapsuleCollider>();
        collisionTransform = collisionObj.transform;
        controller = controllerObj.GetComponent<SteamVR_TrackedController>();
    }

    void FixedUpdate ()
	{
		Mesh gridmesh = GetComponent<MeshFilter>().mesh;
		
        Vector3[] vertices = gridmesh.vertices;
		
		updateParticlesExplicitEuler();
		updateParticlesCollision();
        //updateConstraints(); // fracture
        updateColliderInfo();
        updateController();
		
		int i = 0;
	    while (i < vertices.Length) //copy particle pos to mesh verts
        {
            vertices[i] = transform.TransformPoint(p_pos[i]);
            //vertices[i] = p_pos[i];
            i++;
        }
		gridmesh.vertices = vertices;
		gridmesh.RecalculateNormals();
        gridmesh.RecalculateBounds();
	}

    void updateColliderInfo()
    {
        collisionCapsuleA = collisionTransform.TransformPoint(collisionCapsule.center + new Vector3(0.0f, 0.0f, collisionCapsule.height ));
        collisionCapsuleB = collisionTransform.TransformPoint(collisionCapsule.center - new Vector3(0.0f, 0.0f, collisionCapsule.height ));
        Debug.DrawLine(collisionCapsuleA, collisionCapsuleB);
    }

    void updateController()
    {
        if( controller.triggerPressed )
        {
            addWind();
            //matchShape();
        }
    }

    void addWind()
    {
        int i = 0;
        Vector3 windVec;
        while (i < p_pos.Length)
        {
            windVec = collisionTransform.TransformVector( new Vector3(0,0,1));
            p_vel[i] += targetMatchSpeed * windVec;
            i++;
        }
    }

    public void matchShape()
    {
        int i = 0;
        Vector3 targetVec;
        while (i < p_pos.Length)
        {
            targetVec = p_targetPos[i] - p_pos[i];
            if (targetVec.magnitude > 0.01)
            {
                p_vel[i] += targetMatchSpeed * targetVec;
            }
            i++;
        }
    }

    void updateParticlesExplicitEuler()
    {
        for (int substep = 0; substep < substeps; substep++)
        {
            int i = 0;
            while (i < p_pos.Length)
            {
                p_force[i] = gravity * ( transform.TransformVector( Vector3.down ) ) + calcForce( i ); //gravity plus spring forces
                //p_force[i][2] = 0.0f; // no force in z, keep planar
				

                p_vel[i] *= 0.9985f; // damping
                i++;
            }
/*
			// Alternate way to do damping
			int lengthCount = p_pos.Length / 2;
			for (int j = 0; j < p_pos.Length / 2; j++)
			{
				Vector3 sideA = p_pos[j] - p_pos[j + lengthCount];
				Vector3 fwdA = p_pos[j] - p_pos[j > 0 ? j - 1 : j + 1];
				Vector3 normal = Vector3.Cross(sideA, fwdA).normalized;
				float dot = Mathf.Abs(Vector3.Dot(p_vel[j].normalized, normal));
				p_vel[j] *= Mathf.Lerp(dampTangent, dampNormal, dot);
				p_vel[j + lengthCount] *= Mathf.Lerp(dampTangent, dampNormal, dot);
			}
*/
            i = 0;
            while (i < p_pos.Length)    //explicit euler integration
            {
                p_vel[i] += p_force[i] * Time.fixedDeltaTime / substeps;
	            p_pos[i] += p_vel[i]   * Time.fixedDeltaTime / substeps;
				i++;
	        }
		} //substep	
	}
	
	void updateParticlesRungeKutta()
    {
		for(int substep = 0; substep < substeps; substep++)
        {
			int i = 0;
			while (i < p_pos.Length)
            {
				runge[0, i] = p_vel[ i ];                                                                          //a1
				runge[1, i] = gravity * transform.TransformDirection( Vector3.down ) + calcForce( i );                //a2
				i++;
	        }
			i = 0;
			while (i < p_pos.Length)
            {
				runge[2, i] = p_vel[i] + Time.fixedDeltaTime / ( 2 * substeps ) * runge[1, i];	                 //b1
				runge[3, i] = gravity * transform.TransformDirection( Vector3.down ) + calcForceRK( i );         //b2
				
	            p_vel[i] += p_force[i] * Time.fixedDeltaTime / substeps;
	            p_pos[i] += p_vel[i]   * Time.fixedDeltaTime / substeps;
				i++;
	        }
		}//substep
	}
	
	Vector3 calcForceRK(int point)
    {
        return Vector3.zero;
	}

	Vector3 calcForce(int point)
    {
		float	dist	= 0.0f;
		Vector3	diff	= Vector3.zero;
		Vector3 diff_v	= Vector3.zero;
		int i = 0;
//		print ("point " + point); //debug

		Vector3 force = Vector3.zero;
		Vector3 total_force = Vector3.zero;
		while ( i < max_constraints_per_point )                 // i constraints per point
        { 
			int c_curr	= p_constraints[point, i];	            // i-th constraint from this point's constraint list
			if( c_curr == -1 || c_active[c_curr] == false)      // if no constraint or broken constraint
            { 
				i++;
				continue;
			}
//			print ("i " + i + " c_curr " + c_curr); // debug

			int p_start	= c_ends[c_curr, 0];		// the point index this constraint starts at
			int p_end 	= c_ends[c_curr, 1];		// and ends at

			diff = p_pos[p_end] - p_pos[p_start];
			dist = diff.magnitude;
			
			force =  c_stiff[i] * diff  * (dist - c_len[c_curr]) / dist;	// spring force	(3.1)

			diff_v = p_vel[p_end] - p_vel[p_start];
			force +=  c_damp[i] * Vector3.Project( diff_v, diff );		    // damping force (3.3)
			
			if(p_end == point) force *= -1.0f;      // negate the constraint force if current is at the hind end of the constraint
			
			total_force += force / dist;
			i++;		
		}
		return total_force;
	}
	
	void updateParticlesCollision()
    {	
		// ground collisions
		int i = 0;
		while (i < p_pos.Length)
        {
            if( transform.TransformPoint( p_pos[i] )[1] < 0.01f ) // ground level
            {
                //p_pos[i][1] = -3.99f;				
                p_pos[i][1] = transform.TransformPoint(new Vector3(0, 0.01f, 0))[1];
                p_vel[i] *= 0.9f; //drag
				p_vel[i][1] = -0.8f * transform.TransformPoint( p_vel[i] )[1]; //bounciness
			}
			i++;
        }
		collPos = parentObj.transform.position;
        /*
		// collider object collisions
		float collRadius = 0.5f;
		Vector3 impulse = Vector3.zero;
		Vector3 impulseTotal = Vector3.zero;
		Vector3 p_diff = new Vector3 ( 0, 0, 0 );
		for ( int p = 0; p < p_pos.Length; p++ )
        {
			p_diff = p_pos[p] - collPos; // vector from collider center to current point
            if( p_diff.magnitude < collRadius ) // collider radius
            { 
				p_pos[p] += ( collRadius - p_diff.magnitude ) * p_diff.normalized; // move point to outside edge of collider
				impulse = 0.5f * p_diff.normalized + 0.3f * parentObj.GetComponent<Rigidbody>().velocity; //bounciness/friction. point gets velocity both along collider surface normal direction as well as collider velocity direction 
				p_vel[p] += impulse;
				impulseTotal += impulse - 0.3f * p_vel[p];
			}
        }
        */

		// collider object collisions
		float collRadius = collisionCapsule.radius;
        Vector3 collVel = parentObj.GetComponent<Rigidbody>().velocity;
        Vector3 impulse = Vector3.zero;
		Vector3 impulseTotal = Vector3.zero;
		Vector3 p_diff = new Vector3 ( 0, 0, 0 );
		for ( int p = 0; p < p_pos.Length; p++ )
        {
			//p_diff = p_pos[p] - collPos; // vector from collider center to current point
            p_diff = p_pos[p] - ProjectPointLine(p_pos[p], collisionCapsuleA, collisionCapsuleB);
            if( p_diff.magnitude < collRadius ) // collider radius
            { 
				p_pos[p] += ( collRadius - p_diff.magnitude ) * p_diff.normalized; // move point to outside edge of collider
				impulse = 0.5f * p_diff.normalized + 0.3f * collVel; //bounciness/friction. point gets velocity both along collider surface normal direction as well as collider velocity direction 
				p_vel[p] += impulse;
				impulseTotal += impulse - 0.3f * p_vel[p];
			}
        }

        //feedback to collider
        //parentObj.GetComponent<Rigidbody>().velocity -= 0.12f * impulseTotal;

        //parent first point to parentObj
        p_pos[0] = parentObj.transform.position + controllerObj.transform.TransformVector( posOffset);
        p_vel[0] = parentObj.GetComponent<Rigidbody>().velocity;

        p_pos[gridWidth] = parentObj.transform.position - controllerObj.transform.TransformVector( new Vector3(0.0f, 0.0f, -gridSize) - posOffset);
        p_vel[gridWidth] = parentObj.GetComponent<Rigidbody>().velocity;

    }

    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 line = lineEnd - lineStart;
        float num2 = Mathf.Clamp(Vector3.Dot(line, rhs), 0f, line.magnitude);
        Vector3 pointInLine = (lineStart + ((Vector3)(line * num2)));
        return pointInLine;
    }

    void updateConstraints()
    {
		Vector3 diff = Vector3.zero;
		float dist = 0.0f;
		
		for(int i = 0; i<c_count; i++)
        {
			int c_start	= c_ends[i, 0];		// the point index this constraint starts at
			int c_end 	= c_ends[i, 1];		// and ends at
		
			diff = p_pos[c_end] - p_pos[c_start];
			dist = diff.magnitude;
		
			if (dist > 0.9f) // fracturing
            {
				c_active[i] = false;
			}
		}
	}
	
	Mesh createGrid(int hres, int vres, Vector3 origin)      //horizontal, vertical resolution
    {
		Mesh gridmesh = new Mesh();
        Vector3[] vertices	= new Vector3[hres * vres];
        Vector2[] uvs	= new Vector2[hres * vres];
		int isize = 6;
		int[] triangles = new int[isize * ( ( hres - 1 ) * ( vres - 1 ) ) ];
		float size = gridSize;
		
		int i = 0;
		int j = 0;
		int trindex = 0;
		
		//create vertices
		for(j = 0; j < vres; j++)
        {
			for(i = 0; i < hres; i++)
            {
                vertices[hres * j + i] = transform.TransformPoint( new Vector3( size * i, size * j, 0.0f ) + origin );
                p_targetPos[hres * j + i] = vertices[hres * j + i];
                uvs[hres * j + i] = new Vector2( i / hres, j / vres );
			}	
		}
		//create tris
		for(j = 0; j < vres - 1; j++)
        {
			for(i = 0; i < hres - 1; i++)
            {
				trindex = ( ( hres - 1 ) * j + i ) * isize;

				triangles[trindex + 0] = hres * j + i;		    	//tri 1 vert 0
				triangles[trindex + 1] = hres * ( j + 1 ) + i;		//tri 1 vert 1
				triangles[trindex + 2] = hres * ( j + 1 ) + i + 1;	//tri 1 vert 2
			
				triangles[trindex + 3] = hres * j + i;		    	//tri 2 vert 0
				triangles[trindex + 4] = hres * ( j + 1 ) + i + 1;	//tri 2 vert 1
				triangles[trindex + 5] = hres * j + i + 1;	    	//tri 2 vert 2
				
			}	// i	
		}		// j
		
		gridmesh.vertices = vertices;
		gridmesh.uv = uvs;
		gridmesh.triangles = triangles;
        //Debug.Log("mesh points " + vertices.Length );
		return gridmesh;
	}

	void initParticles( Mesh gridmesh )
    {
		p_pos = new Vector3[gridWidth * gridHeight];
		p_vel = new Vector3[p_pos.Length];
		p_force = new Vector3[p_pos.Length];
		p_constraints = new int[p_pos.Length, max_constraints_per_point];
		
		runge = new Vector3[4, p_pos.Length];
		
		int i = 0;
		while (i < p_pos.Length)
        {
            p_pos[i] = gridmesh.vertices[i];
            p_vel[i] = Vector3.zero;
            p_force[i] = Vector3.zero;
			for(int c = 0; c < max_constraints_per_point; c++)
            {
				p_constraints[i, c] = -1; //init constraint list to -1
			}	
			i++;
        }
        //p_vel[0] = transform.TransformDirection(Vector3.up) * initVel; //test

        p_pos[0] = parentObj.transform.position; // init the first points to collider obj
        p_pos[gridWidth] = parentObj.transform.position - new Vector3(0.0f, -gridSize, 0.0f);
    }



    void initConstraints( Mesh gridmesh )
    {
		c_count = ((gridWidth - 1) * (gridHeight - 1)) * 4 + gridWidth + gridHeight - 2; // //quad grid with enforcement bars

		c_ends = new int[c_count, 2];
		c_len = new float[c_count];
		c_stiff = new float[c_count];
		c_damp = new float[c_count];
		c_active = new bool[c_count];

		int i = 0;
		while (i < c_count)
        {
			c_len[i] = 0f;		//todo: init lengths from endpoints below
			c_stiff[i] = stiffness;		
			c_damp[i] = damping;
			c_active[i] = true;
			i++;
        }
		int j = 0;
		int c_index = 0;
		int c_size = 4; 	//constraints per point
		int vres = gridHeight;
		int hres = gridWidth;
		
		for(j = 0; j < vres-1; j++)
        {
			for(i = 0; i < hres-1; i++)
            {           					// loop all points except last row and last column
				c_index = c_size * ((hres - 1) * j + i);
				// define constraint end points
				/*
				+--1--->
				|\    /
				| \  3
				0  2/
				|  /\
				| /  \
				|/    >
				
				*/
				c_ends[c_index + 0, 0] = hres * j + i;		//start point of first constraint for this point
				c_ends[c_index + 0, 1] = hres * ( j + 1 ) + i;	//end point of first constraint for this point
				c_len[c_index + 0] = gridSize;

				c_ends[c_index + 1, 0] = hres * j + i;
				c_ends[c_index + 1, 1] = hres * j + i + 1;
				c_len[c_index + 1] = gridSize;

				c_ends[c_index + 2, 0] = hres * j + i;
				c_ends[c_index + 2, 1] = hres * ( j + 1 ) + i + 1;
				c_len[c_index + 2] = gridSize * 1.4142f;

				c_ends[c_index + 3, 0] = hres * ( j + 1 ) + i;
				c_ends[c_index + 3, 1] = hres * j + i + 1;
				c_len[c_index + 3] = gridSize * 1.4142f;
			}
		}
		
		for(j = 0; j < vres; j++)
        {
			for(i = 0; i < hres; i++)
            {
				
				if( j == vres - 1 && i < hres - 1)     //last row
                { 					
					c_index = c_size * ((hres-1) * j) + i;
					c_ends[c_index + 0, 0] = hres * j + i;
					c_ends[c_index + 0, 1] = hres * j + i + 1;
                    c_len[c_index + 0] = gridSize;
                }
                if ( j < vres - 1 && i == hres - 1 )           //last column
                { 					
					c_index = c_size * ( ( hres - 1 ) * ( vres - 1 ) ) + ( hres - 1) + j;
					c_ends[c_index + 0, 0] = hres * j + i;	
					c_ends[c_index + 0, 1] = hres * (j + 1) + i;
                    c_len[c_index + 0] = gridSize;
                }
            }
		}
		
		//precalc per point which constraints affect given point
		for(i = 0; i < p_pos.Length; i++)
        {
			j = 0;
			for(int c = 0; c < c_count; c++)
            {
				if(c_ends[c,0]==i ||c_ends[c,1]==i )
                {
					p_constraints[i,j] = c;
					j++;
				} 
			}
		}
		/*
		for(j = 0; j < vres-1; j++){
			for(i = 0; i < hres-1; i++){					// loop all points except last row and column
				c_index = c_size * ((hres-1)*j + i);	//this point's index into the constraint array
				//print (c_index);
				
				// precalc table of which constraints affect which points
				
				p_constraints[(hres-0)*j + i, 0] = c_index + 0;
				p_constraints[(hres-0)*j + i, 1] = c_index + 1;		//first the three constraints defined above
				p_constraints[(hres-0)*j + i, 2] = c_index + 2;
				
				if(i==0) {	//first column
					p_constraints[(hres-0)*j + i, 3] = -1;
					p_constraints[(hres-0)*j + i, 4] = -1;
					p_constraints[(hres-0)*j + i, 7] = -1;
				}
				else {
					p_constraints[(hres-0)*j + i, 3] = c_index - 3;	//constraints from previous point
					p_constraints[(hres-0)*j + i, 4] = c_index - 1;
					p_constraints[(hres-0)*j + i, 7] = c_index - c_size * (hres-1) - 2;	//constraints from above & left point
				}
				if(j==0) {	//first row
					p_constraints[(hres-0)*j + i, 5] = -1;
					p_constraints[(hres-0)*j + i, 6] = -1;
					p_constraints[(hres-0)*j + i, 7] = -1;
				}
				else {
					p_constraints[(hres-0)*j + i, 5] = c_index - c_size * (hres-1) + 0;	//constraints from above point
					//print(c_index - c_size * (hres-1) + 0);	//constraints from above point
					p_constraints[(hres-0)*j + i, 6] = c_index - c_size * (hres-1) + 3;
					p_constraints[(hres-0)*j + i, 7] = c_index - c_size * (hres-1) - 2;	//constraints from above & left point
				}
				
				if( j==0 || i==0 ) {	//first row and column have no aboveleft constraint
					p_constraints[(hres-0)*j + i, 7] = -1;
				}
			}//i
		}//j
		
		//last column
		i = hres-1;	
		for(j = 0; j < vres-1; j++){ 
			for(int c = 0; c < max_constraints_per_point; c++) {
				p_constraints[hres*j + i, c] = -1;
			}	
			c_index = c_size * ((hres-1)*j + i - 1); //point to the left
			p_constraints[hres*j + i, 3] = c_index + 1;
			p_constraints[hres*j + i, 4] = c_index + 3;
			if(j!=0){
				c_index = c_size * ((hres-1)*(j-1) + i - 1); //point to the left&up
				p_constraints[hres*j + i, 7] = c_index + 2;
			} else {
				p_constraints[hres*j + i, 7] = -1;
			}				
		
		}
		
		//last row
		j = vres-1;		
		for(i = 0; i < hres-1; i++){ 
			for(int c = 0; c < max_constraints_per_point; c++) {
				p_constraints[hres*j + i, c] = -1;
			}	
			c_index = c_size * ((hres-1)*(j-1) + i); //point above
			p_constraints[hres*j + i, 5] = c_index + 0;
			p_constraints[hres*j + i, 6] = c_index + 3;
			if(j!=0){
				c_index = c_size * ((hres-1)*(j-1) + i-1); //point above&left
				p_constraints[hres*j + i, 7] = c_index + 2;
			} else {
				p_constraints[hres*j + i, 7] = -1;
			}
		}
		
		// last vertex
		i = hres-1;
		for(int c = 0; c < max_constraints_per_point; c++) {
				p_constraints[hres*j + i, c] = -1;
		}	
		c_index = c_size * ((hres-1)*(j-1) + i -1); //point above&left
		p_constraints[hres*j + i, 7] = c_index + 2;
		 */
	}	
	
	
} //class
