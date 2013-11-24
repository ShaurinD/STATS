/* OPEN CV TEST CODE
#include "stdafx.h"
#include <stdio.h>
#include <iostream>

int main()
{
	CvCapture* capture = cvCaptureFromCAM(0);
	IplImage* frame;
	while(1){
		frame= cvQueryFrame(capture);
		cvShowImage("u7karsh",frame);
		int key = cvWaitKey(1);
			if(key=='q') break;
						}
			cvReleaseCapture(&capture);
			return 0;
}
*/


#include "stdafx.h" 
#include <stdio.h> 
#include <windows.h> 
#include <wchar.h> 
#include <vector> 
 
#include "pxcsession.h"
#include "pxcsmartptr.h" 
#include "pxccapture.h"
#include "util_render.h"
#include "util_capture_file.h"
#include "util_pipeline.h"
#include <iostream>
#include "opencv2/core/core.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/calib3d/calib3d.hpp"

using namespace cv;
using namespace std;

int _tmain(int argc, _TCHAR* argv[]) 
{ 
	//Initializing objects for analysis
	//Creating the session
	PXCSmartPtr<PXCSession> session;
	PXCSession_Create(&session);
	//Creating face analysis object
	PXCFaceAnalysis *face=0; 
	session->CreateImpl(PXCFaceAnalysis::CUID,(void**)&face);
	//Initializing profile info
	PXCFaceAnalysis::ProfileInfo pinfo; 
	face->QueryProfile(0,&pinfo); 
	//Starting capturex
	UtilCapture capture(session); 
	capture.LocateStreams(&pinfo.inputs); 
	face->SetProfile(&pinfo); 
	//Creating face detection module
	PXCFaceAnalysis::Detection *det=face->DynamicCast<PXCFaceAnalysis::Detection>(); 
	PXCFaceAnalysis::Detection::ProfileInfo dinfo = {0}; 
	det->QueryProfile(0,&dinfo); 
	det->SetProfile(&dinfo); 
	//Creating landmark module 
	PXCFaceAnalysis::Landmark *landmark=face->DynamicCast<PXCFaceAnalysis::Landmark>();
	PXCFaceAnalysis::Landmark::ProfileInfo linfo = {0};
	landmark->QueryProfile(1,&linfo); 
	landmark->SetProfile(&linfo);
	PXCFaceAnalysis::Landmark::LandmarkData ldata[7];
	//Declaring Detection and Landmark data objects for analysis
	PXCFaceAnalysis::Detection::Data data; 
	PXCFaceAnalysis::Landmark::PoseData pdata;
	//Storage containers for images
	PXCImage* mcImage;
	PXCImage::ImageData mCImageData; //Color Image data
	PXCSmartArray<PXCImage> images; 
	PXCSmartSPArray sps(2); 

	for (;;) { 
		// Get samples from input device and pass to the module 
		capture.ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0)); 
		face->ProcessImageAsync(images,sps.ReleaseRef(1)); 
		sps.SynchronizeEx(); 
		//OpenCV objects initializations
		mcImage = capture.QueryImage(&images[0], PXCImage::IMAGE_TYPE_COLOR);
		//Creating Mat from PXCImage
		if (mcImage->AcquireAccess(PXCImage::ACCESS_READ, &mCImageData)>=PXC_STATUS_NO_ERROR) {
			IplImage* colorimg = cvCreateImageHeader(cvSize(640, 480), 8, 3);
			cvSetData(colorimg, (uchar*)mCImageData.planes[0], 640*3*sizeof(uchar));
			cv::Mat image(colorimg);
			//Output Mat
			imshow("display", image);
			/*
			cv::Mat smallImage = image;
			cv::Mat bigWindow = cv::Mat::zeros(960,1280, smallImage.type());

			cv::Rect r(0,0,smallImage.cols, smallImage.rows);
			cv::Mat roi = bigWindow(r);
			smallImage.copyTo(roi);
			cv::namedWindow("Display"); // cv::namedWindow("Display", 0); if you want to be able to  resize window
			cv::imshow("Display", bigWindow);
			cv::waitKey(0);
			*/
		}
		
		// Tracking or recognition results are ready 
		pxcUID fid; pxcU64 ts; 
		cout << "QUERY FACE" << endl;
		if (face->QueryFace(0,&fid,&ts)<PXC_STATUS_NO_ERROR) continue; 
		landmark->QueryLandmarkData(fid, linfo.labels, &ldata[0]);
		landmark->QueryPoseData(fid, &pdata);
		det->QueryData(fid,&data); 
		cout << "DATA   " << data.confidence << " " << data.viewAngle << " " << data.rectangle.x << " " << data.rectangle.y << endl;
		cout << "PDATA  " << pdata.pitch << " " << pdata.roll << " " << pdata.yaw << endl;
						
		/* Iterating through landmarks
		/ 
			j = 0 LABEL_LEFT_EYE_OUTER_CORNER
				1 LABEL_LEFT_EYE_INNER_CORNER
				2 LABEL_RIGHT_EYE_OUTER_CORNER
				3 LABEL_RIGHT_EYE_INNER_CORNER
				6 LABE_NOSE_TIP
		*/
		cout << "LDATA ";
		for (int j = 0; j < 7; j++){
			cout << ldata[j].position.x << " " << ldata[j].position.y << " " << ldata[j].position.z << endl;
		}
		// Process data 
			
	} 
} 
