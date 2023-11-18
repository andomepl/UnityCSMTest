#ifndef SHADOW_UTILITY
#define SHADOW_UTILITY


#define MAX_CASCADES 4


float4x4 cascadesMatrices[MAX_CASCADES];
float4x4 cascadesWorldToCameraMatrices[MAX_CASCADES];


int cascadesNums;

float4 far_z;

int GetIndexOfTextureSlices(float z){
	
	int index=3;
	if(z<far_z.x){
		index=0;
	}
	else if(z<far_z.y){
		index=1;
	}
	else if(z<far_z.z){
		index=2;
	}

}

float GetCompareZ(float3 pos){


	return 1.0f;
}




#endif

