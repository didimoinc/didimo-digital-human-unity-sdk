#define ENABLE_LIGHTING
//#define ENABLE_REFLECTIONS
#define ENABLE_AMBIENT_LIGHT
#define ENABLE_SPECULAR
#define ENABLE_DIRECTIONAL_LIGHT
#define ENABLE_POINT_LIGHT
#undef USELIGHTINGHELPERS
#include "shadertoy_defines.glsl"
#include "util_hdr.glsl"
#include "util_camera.glsl"
#include "texture_defines.glsl"
vec4 mouse;

uniform vec2 material_resolution; //= 1024, 512

uniform mediump vec3 vLightVec; //aliases to 'direction_light_positions[0]'
uniform vec3 vLightPos;//= 0.0, 12.0, -5.0
uniform vec3 vPointLightCol;//= 0.0, 12.0, -5.0
uniform vec3 vDirectionalLightCol;//= 1.0, 1.0, 1.0

OUTTYPE mediump vec3 vWorldPosition;
OUTTYPE mediump vec4 vViewSpacePosition;

uniform mat4 u_mvpMatrix;
uniform mat4 u_invmvpMatrix;
uniform mat4 u_view_projection_matrix;
uniform mat4 u_inv_view_projection_matrix;
uniform mat4 u_view_matrix;

uniform vec2		 window_resolution;
#ifndef TARGET_RESOLUTION_DEFINED
uniform vec2         Target_Resolution;

#define TARGET_RESOLUTION_DEFINED
#endif

#ifdef USING_VIEWPORT_COORD
#ifndef VIEWPORTCOORD_DEFINED
OUTTYPE mediump vec4 vViewportCoord;
#define VIEWPORTCOORD_DEFINED
#endif
#endif

#ifdef USE_VERTEX_RAYCAST
INTYPE vec3 vRayStart;
INTYPE vec3 vRayDirection;  
#endif

uniform lowp vec3 sky_colour;
uniform lowp vec3 ground_colour;

uniform vec3 vViewPosition; //should get picked up
uniform vec3 vViewDirection; //should get picked up
uniform vec4 m_eye_base_albedo;
uniform mat4 u_modelMatrix;
uniform vec3 u_modelPosition;
uniform mat3 u_ModelRotationMatrix;
uniform vec2 u_spriteScale;
uniform mediump mat4 u_invProjectionMatrix;
uniform mediump mat4 u_invViewMatrix;
uniform mediump mat4 u_projectionMatrix;


#include "util_maths.glsl"
#include "util_noise.glsl"
#include "util_lighting.glsl"
#include "util_raycast.glsl"
#include "util_shading.glsl"
#include "util_planet.glsl"
uniform sampler2D backbuffer;

float CloudCover(vec2 uv2d);
vec3 GetRayPos(C_Ray ray, float d);
bool RayHitSphere(C_Ray ray, const in vec3 centre, float rad, out float d);
bool RayHitSphere2(C_Ray ray, const in vec3 centre, float rad, out float d1,out float d2);

//land height control
uniform vec3  m_position; //= 0,0,0
uniform float m_radius; //= 4.0, range(0,10000)


//surface properties
uniform float m_fresnel_power; //= 2.0, range(0.0, 100.0)
uniform float m_specular_intensity; //= 0.1, range(0.0, 100.0)
uniform float m_smoothness; //= 0.1, range(0.0, 100.0)
uniform float m_fresnel; //= 0.1, range(0.0, 100.0)
uniform float m_iris_offset; //= 4.07107, range(-40.0, 40.0, 1.0)
uniform float m_iris_radius_ratio; //= 0.664975, range(0,1,0.1)
uniform float m_pupil_offset; //= -0.239595, range(-40,40,1)
uniform float m_pupil_radius_ratio; //= 0.13198, range(0,1,0.1)
uniform vec4 m_iris_colour; //= 0.207989, 0.200521, 0.534722, 0
uniform float m_blendsmoothness; //= 0.31, range(0.0, 100.0, 1.0)
uniform float m_conjunctiva_frespower; //= 1.0, range(0.0, 10.0, 1.0)
uniform float m_conjunctiva_thickness_ratio; //= 1.01, range(0.0, 10.0, 0.1)
uniform float m_conjunctiva_shininess; //= 10.01, range(0.0, 100.0, 0.1)

uniform vec4  m_albedo_boost_colour; //= 0.0, 0.0, 0.0, 1.0
uniform vec4  m_albedo_colour; //= 0.3, 0.01, 0.01, 0.5
uniform float m_seed; //= 0.2, range(0.0, 100.0)
uniform float m_rotate_y; //= 200.0, range(0.0, 100.0)
uniform float m_coast_sharpness; //= 0.1, range(0.0, 100.0)
uniform float m_cloud_cover; //= 0.3, range(0.0, 100.0);


//out float gl_FragDepth;

#define LAND_LATITUDE_PALETTE 2.0
#define LAND_HEIGHT_PALETTE 1.0

C_Material GetObjectMaterial( inout C_ContextInformation cinfo);

//#define ENABLE_TEST_CYLINDER
 
float GetShadow( const in vec3 vPos, const in vec3 vLightDir, const in float fLightDistance );

vec4 getGradient(float d, float grad)
{
	return texture2D(iChannel1, vec2(d, ((grad * 2.0)+ 0.5) / 256.0), -100.0);
}  

vec3 GetSkyGradient( const in vec3 vDir )
{
	float fBlend = clamp(vDir.y * 0.5 + 0.5, 0.0, 1.0);
	return mix(sky_colour.rgb, ground_colour.rgb, fBlend);
}

vec3 GetDirectionalLightDir()
{
	return vLightVec;
}

vec3 GetDirectionalLightCol()
{
	return vDirectionalLightCol;
}

vec3 GetLightPos()
{
	vec3 vRetLightPos = vLightPos;
	#ifdef ENABLE_MONTE_CARLO         
	vRetLightPos += gRandomNormal * 0.2;
	#endif
	return vRetLightPos;
}
vec3 GetLightCol()
{
	return vPointLightCol;
}

vec3 GetLightSpecularCol()
{
   return vPointLightCol;   
}

vec3 GetAmbientLight(const in vec3 vNormal)
{
	return GetSkyGradient(vNormal);
}


vec2 getAtmosphereUV(vec3 normal)
{
	return vec2(atan(normal.x,-normal.z)*INV_PI, acos(normal.y )*INV_PI) * 0.5;
}

vec4 GetDistanceBox( const in vec3 p, const in vec3 b )
{
	vec4 vResult = vec4(10000.0, 4, p.x, p.y);    
	vec3 d = abs(p) - b;
	vResult.x = min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
	return vResult;
}

vec4 GetDistanceSphere(const in vec3 vPos, const in float r)
{
	vec4 vResult = vec4(10000.0, 4, vPos.x, vPos.y);	
	vResult.x = length(vPos.xyz) - (r); 
	return vResult;
}

//#define TEST_PLANET_AS_SPHERE
/********************************************//**
* \brief 	GetDistancePlanet
* \param 	vPos [in] <TODO> 
* \return 	Properties of intersected object. See 'GetDistanceScene' for details
* \details 	
***********************************************/

//#define TEST_PLANET_AS_SPHERE


/*float getSpriteScaleFactor()
{
  return u_spriteScale.x * 0.9;  
} */

vec2 getSphereUV(vec3 normal)
{
	return vec2(atan(normal.x,normal.y)*INV_PI, acos(normal.z )*INV_PI) ;
}

highp vec4 GetDistanceInnerEye(const in vec3 vPos, const in float r, const in float offsetiris, const in float irisradiusratio, const in float offsetpupil, const in float pupilradiusratio)
{
	vec4 vResult = vec4(10000.0, 4, vPos.x, vPos.y);
	float len = length(vPos);
	if (len == 0.0)
		return vec4(0.0, 1, vPos.x, vPos.y);
	float pupilLen = length(vPos + vec3(r - offsetpupil,0,0));
	float irisLen = length(vPos + vec3(offsetiris,0,0));

	vResult.y = 1.0;
	vResult.z = 0.0;	
	vResult.w = 0.0;

	float d = len - r; //main sphere
	float d2 = pupilLen - r * pupilradiusratio; //subtractive sphere for iris

	//todo optimise this by parametising out of distance loop based on sphere ray intersection
	vec3 normal = normalize(vPos - u_modelPosition);		
	vec2 uv = getSphereUV(normal);	
	vec4 diffuse_tex_sample = SampleSRGBTexture(DiffuseTextureSampler, uv);

	float d3 = irisLen - r * irisradiusratio; //additive sphere for iris (n.b noise on this?)	
	d3 -= diffuse_tex_sample.r * 0.15;
	vec4 d_v4 = DistCombineUnionSmooth(vec4(d,1.0,0.0,0.0),vec4(d3,2.0,0.0,0.0),m_blendsmoothness);  
	vResult = DistCombineSubtractSmooth(vec4(d2,3,0.0,0.0),d_v4,m_blendsmoothness);
	//vResult = d_v4;
	//vResult.g = clamp((vResult.g - 1.0) / 3.0, 0.001, 1.0);
	//d = DistCombineUnionSmooth(d,d3, 0.2);  
	//vResult.x = DistCombineSubtractSmooth(d2,d,0.2);
	return vResult;
}


//#define TEST_CUBE
/********************************************//**
* \brief 	GetDistanceScene
* \param 	vPos [in] <TODO> 
* \return 	vec4 where 'x' is distance, 'y' is a material identifier (used by GetObjectMaterial, at which point it's actually in 'x')
*			z and w are generic parameters.
* \details 	
			z and w are interpreted as you like by 'GetObjectMaterial'.
			'yzw' of the result of this functiona are transposted to 'xyz' in 'GetObjectMaterial'
			This transposition is performed in the function 'raymarch' located in 'util_raycast'
***********************************************/
highp vec4 GetDistanceScene(const in vec3 vPos)
{
	highp vec4 vResult = vec4(10000.0, 0.0, 0.0, 0.0);
	// * u_ModelRotationMatrix    
	vec3 planetPos = vPos - u_modelPosition;//u_modelPosition.xyz;
	//planetPos = RotateX(planetPos,PI * 0.5);
	vResult = GetDistanceInnerEye(planetPos, m_radius, m_iris_offset, m_iris_radius_ratio, m_pupil_offset, m_pupil_radius_ratio);// * u_ModelMatrix
	//vResult = opU(vResult, GetDistanceSphere(pos, m_radius));
	#ifdef TEST_CUBE
	float r = m_radius * 0.4;
	
	
	planetPos = RotateY(planetPos,0.2);
	vResult = opU(vResult, GetDistanceBox(planetPos,vec3(r,r,r)));
	#endif
		
	/*vResult = formula(vec4(1.0+vPos * 100.0,1.0));//
					  DistCombineUnion(vResult, formula(vec4(1.0+vPos * 10000.0,0.5)));*/
	return vResult;
}

vec4 CalculateSheen(vec4 incol, C_Ray ray, float planetSurfaceDist, float rayAtmoTravelDistance, inout float outDist)
{
	float d,d2,d3;
	const float fDelta = 0.025;
	 
	float conjunctiva_radius = m_radius * m_conjunctiva_thickness_ratio;
	//ray.vOrigin -= u_modelPosition;
   
	float radius = m_radius;

	vec3 centre = u_modelPosition;
	if (RayHitSphere2(ray, centre, conjunctiva_radius, d, d2))	
	{
		outDist = d;
		vec3 rpos = GetRayPos(ray, d) - centre;
	   
		vec3 normal = (rpos) / conjunctiva_radius;
		C_Material material = initSpecularOnlyMaterial();
		#ifdef USECUSTOMFRESNELPOWER
			material.fFresnelPower = m_conjunctiva_frespower;		///< 0..n power curve applied in Schlick
			material.fSpecPowerMul = 1.0;
		#endif
		material.fSmoothness = m_conjunctiva_shininess;			///< 0..1 reflection power factor- higher means more mirror like, lower more diffuse	
    	material.fSpecularIntensity = 1.0;
		vec3 cReflection = vec3(0,0,0);
		vec3 lighting = GetObjectLighting(ray, rpos, material,
		#ifdef USELIGHTINGHELPERS
		cinfo.lightingInfo,
		#endif
		normal, cReflection);		
	
					
		incol.rgb += lighting;
		incol.a = incol.a;
		return incol;
	}
	else
		return incol;
}

void GetCameraRayWorldScale( vec3 vPos, out C_Ray ray)
{       
	vPos *= 5.0;
	ray.vDir = normalize( vPos - vViewPosition.xyz);
	ray.vOrigin = vPos - ray.vDir * 5.0;// * 0.001;         
}

vec3 normalToColour(vec3 normal)
{
   return vec3((normal.x * 0.5) + 0.5,(normal.y * 0.5) + 0.5, (normal.z * 0.5) + 0.5); 
}
//#define TEST_MATERIAL_IDX_BLEND
C_Material GetObjectMaterial(inout C_ContextInformation cinfo)
{
		C_Material mat;   			
		vec3 normal = normalize(cinfo.hitInfo.vPos - u_modelPosition);		
		vec2 uv = getSphereUV(normal);		
		//uv.x *= 100.0;

		vec4 diffuse_tex_sample = SampleSRGBTexture(DiffuseTextureSampler, uv);    
		mat.cAlbedo  = diffuse_tex_sample.rgb;
		float matidx = cinfo.hitInfo.vObjId.x;
		if (matidx < 1.0)
		{}
		else if (matidx < 2.5)
			mat.cAlbedo = mix(mat.cAlbedo, mat.cAlbedo * m_iris_colour.rgb,  clamp(1.0 - (2.0 - matidx),0.0,1.0) );
		else if (matidx < 5.5)
			mat.cAlbedo = mix(mat.cAlbedo * m_iris_colour.rgb, vec3(0.0,0.0,0.0),clamp(1.0 -  (3.0 - matidx), 0.0,1.0));
		
		#ifdef TEST_MATERIAL_IDX
		mat.cAlbedo = clamp(vec3(matidx / 8.0),vec3(0.001),vec3(1.0));
		#endif
		#ifdef TEST_MATERIAL_IDX_BLEND
		float dx = 0.0;
		
		if (matidx < 2.5)
			dx = (2.0 - matidx) ;
		else if (matidx < 3.5)
			dx = (3.0 - matidx);
		mat.cAlbedo = clamp(vec3(dx),vec3(0.001),vec3(1.0));
		#endif
				
		mat.fSmoothness = m_smoothness;		
		mat.fR0 = m_fresnel;	
		mat.fSpecularIntensity = m_specular_intensity;
		#ifdef USECUSTOMFRESNELPOWER
		mat.fFresnelPower = m_fresnel_power;
		mat.fSpecPowerMul = 1.0;
		#endif
		return mat;
}

 
vec3 GetSceneNormal( const in vec3 vPos )
{
	// tetrahedron normal
	float fDelta = 0.025;

	vec3 vOffset1 = vec3( fDelta, -fDelta, -fDelta);
	vec3 vOffset2 = vec3(-fDelta, -fDelta,  fDelta);
	vec3 vOffset3 = vec3(-fDelta,  fDelta, -fDelta);
	vec3 vOffset4 = vec3( fDelta,  fDelta,  fDelta);

	float f1 = GetDistanceScene( vPos + vOffset1 ).x;
	float f2 = GetDistanceScene( vPos + vOffset2 ).x;
	float f3 = GetDistanceScene( vPos + vOffset3 ).x;
	float f4 = GetDistanceScene( vPos + vOffset4 ).x;

	vec3 vNormal = vOffset1 * f1 + vOffset2 * f2 + vOffset3 * f3 + vOffset4 * f4;

	return normalize( vNormal );
}




uniform vec3 m_view_offset; //= vec3(0, 0, 0)

float distanceToElipse(in vec2 pt, in vec2 c, in vec2 r)
{
	float2 rpt = pt-c;
	return sqrt((rpt.x * rpt.x) / (r.x * r.x) + (rpt.y * rpt.y) / (r.y * r.y));
}

//#define VISUALISE_BACKGROUND
#define DOIT
void main( void )
{
	#ifdef DOIT
	C_Ray ray;	
	mouse.x = m_rotate_y;
	mouse.zw = iMouse.zw / iResolution.xy;
		
	vec2 npos = ((gl_FragCoord.xy / window_resolution.xy)) ;
	#ifdef USE_VERTEX_RAYCAST
		vec2 ndc = (npos - 0.5);// * 2.0;//((gl_FragCoord.xy / iResolution.xy) - vec2(0.5, 0.5)) *512.0 ;// * vec2(2.0, 2.0 );
		mat4 mat = u_invProjectionMatrix;//inverse(u_view_projection_matrix);//u_invmvpMatrix;
		vec4 p = mat * vec4 (ndc.x,  ndc.y, 0, 1);
		p.xyz *= p.w;
		p = u_invViewMatrix * vec4(p.xyz,1.0);
		ray.vOrigin = p.xyz;
		ray.vDir = normalize(vWorldPosition.xyz - p.xyz);

	#else
		GetCameraRayWorld2( vWorldPosition.xyz, ray);
	#endif	

	
	#ifdef VISUALISE_BACKGROUND
		vec4 backgroundcolour = vec4(vWorldPosition.x,vWorldPosition.y,0,0.1);
	#else
		vec4 backgroundcolour = vec4(0.0, 0.0, 0.0, 0.0);
	#endif
	float outDist;
	vec4 cScene = GetSceneColour( ray, backgroundcolour, 0.01, outDist);

	#ifdef ENABLE_APPLY_COLOUR_CORRECTION
		float fExposure = 2.5;
		cScene.rgb = cScene.rgb * fExposure;
		vec4 cCurr;
		cCurr.rgb= Tonemap(cScene.rgb );
		cCurr.a = cScene.a;
	#else
		vec4 cCurr = cScene.rgba;
	#endif
  //cCurr.r *= 2.0;
	
	vec4 cFinal = cCurr;


	float planetSurfaceDist = outDist;
	cFinal.rgba = CalculateSheen(cFinal.rgba , ray, planetSurfaceDist,1.0, outDist);
	
	
	vec3 worldpos = GetRayPos(ray, outDist);
	vec4 screenpos = u_view_projection_matrix * vec4(worldpos,1.0);

	float ndc_depth = ((screenpos.z / screenpos.w) + 1.0) /2.0;
	float camRange =  gl_DepthRange.diff;//getCameraDepthRange();
	float camNear =  gl_DepthRange.near;//getCameraNear();
	gl_FragDepth = camRange * ndc_depth + camNear;
	
	#ifndef VISUALISE_BACKGROUND
		if (cFinal.a < 0.2)
		{
			cFinal.r = 1.0;
			cFinal.g = 0.5;
			//discard;
		}


		//fuggle
		
	

	#endif
	#else
		vec4 cFinal = cScene;	
	#endif
	FrameBufferWrite( cFinal.rgba );		
	//FrameBufferWrite( vec4(mix(cFinal.rg.xy ,npos.xy, 0.91), cFinal.z, 1.0 ));		
	//FrameBufferWrite( vec4( cFinal.a, cFinal.a, cFinal.a, 1.0));       
}
