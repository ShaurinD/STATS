#include "stdafx.h" 
#include <pxcsession.h>
#include <pxcsmartptr.h>
#include <pxcface.h>
#include <pxccapture.h>
#include <util_capture.h>

#include <iostream>

using namespace std;


int _tmain(int argc, _TCHAR* argv[]) { 

	//Session Pointer
	PXCSmartPtr<PXCSession> session; 
	PXCSession_Create(&session); 
	//Creating instance of face tracking module
	PXCFaceAnalysis* face = 0;
	session->CreateImpl(PXCFaceAnalysis::CUID, (void**) face);
	
	//Initialize the tracking module
	PXCFaceAnalysis::ProfileInfo pinfo; 
	face->QueryProfile(0,&pinfo); 
	UtilCapture capture(session); 
	capture.LocateStreams(&pinfo.inputs); 
	face->SetProfile(&pinfo);

	//Initialize tracking features
	PXCFaceAnalysis::Detection *det = face->DynamicCast <PXCFaceAnalysis::Detection>(); 
	PXCFaceAnalysis::Detection::ProfileInfo dinfo; 
	det->QueryProfile(0,&dinfo); 
	det->SetProfile(&dinfo);
	PXCFaceAnalysis::Landmark *landmark = face->DynamicCast<PXCFaceAnalysis::Landmark>();

	

	//Modules for color image and depth image samples
	PXCSmartArray<PXCImage> images; 
	PXCSmartSPArray sps(2); 	
	//Video Capture and Processing Loop
	for(int i = 0;;i++) {
		//Get samples from input device and pass to the module 
		capture.ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0)); 
		face->ProcessImageAsync(images,sps.ReleaseRef(1)); 
		sps.SynchronizeEx(); 
		
		
		pxcUID fid = 0; pxcU64 ts; 
		if (face->QueryFace(i,&fid,&ts)<PXC_STATUS_NO_ERROR) break; 
		PXCFaceAnalysis::Detection::Data data; 
		if (det->QueryData(fid,&data)<PXC_STATUS_NO_ERROR) break; 
		//Getting landmark location of 7 points on face
		PXCFaceAnalysis::Landmark::LandmarkData ldata;
		if (landmark->QueryLandmarkData(fid, PXCFaceAnalysis::Landmark::LABEL_7POINTS, 0, &ldata)<PXC_STATUS_NO_ERROR) break;  
		//Getting pose data of face
		PXCFaceAnalysis::Landmark::PoseData pdata; 
		if (landmark->QueryPoseData(fid,&pdata)<PXC_STATUS_NO_ERROR) break; 
				
		//Tracking or recognition results are ready
		//Now processing the results

		//Analyzing 3D location of Face
		PXCPoint3DF32 lposition = ldata.position;

		//Analyzing Pose Data
		pxcF32 yaw = pdata.yaw;
		pxcF32 roll = pdata.roll;
		pxcF32 pitch = pdata.pitch;
		
		// testing pose data
		cout << "pitch: " << pitch << "  roll: " << roll << "   yaw: " << yaw << endl;



	}
	
	return 0; 
} 