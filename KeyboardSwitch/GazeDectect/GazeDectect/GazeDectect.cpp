#include "stdafx.h"
#include <opencv/cv.h>
#include "opencv2/highgui/highgui.hpp"
#include <stdio.h>

//#include "stdafx.h"
#include "util_render.h"
#include "util_pipeline.h"
#include <iostream>


using namespace std;
using namespace cv;

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
	//Starting capture
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



	UtilPipeline pipeline;
	pipeline.EnableImage(PXCImage::COLOR_FORMAT_RGB24,640,480);
	pipeline.EnableImage(PXCImage::COLOR_FORMAT_DEPTH,320,240); //depth resolution 320,240
	pipeline.Init();
	UtilRender color_render(L"Color Stream");
	UtilRender depth_render(L"Depth Stream");

	///////////// OPENCV
	IplImage *image=0;
	CvSize gab_size;
	gab_size.height=480;
	gab_size.width=640;
	image=cvCreateImage(gab_size,8,3);

	PXCImage::ImageData idata;

	unsigned char *rgb_data;//=new unsigned char[];
	PXCImage::ImageInfo rgb_info;

	Mat colorMat;
	namedWindow("Test");
	namedWindow("Test2");
	///////
	for (;;) {
		capture.ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0)); 
		face->ProcessImageAsync(images,sps.ReleaseRef(1)); 
		sps.SynchronizeEx(); 

		if (!pipeline.AcquireFrame(true)) break;
		PXCImage *color_image=pipeline.QueryImage(PXCImage::IMAGE_TYPE_COLOR);

		color_image->AcquireAccess(PXCImage::ACCESS_READ_WRITE,PXCImage::COLOR_FORMAT_RGB24,&idata); 

		rgb_data=idata.planes[0];


		for(int y=0; y<480; y++)
		{
			for(int x=0; x<640; x++)
			{ 
				for(int k=0; k<3 ; k++)
				{
					image->imageData[y*640*3+x*3+k]=rgb_data[y*640*3+x*3+k];
				}
			}
		}

		color_image->ReleaseAccess(&idata);
		/*
		cvShowImage("rgb_cv", image);
		cvShowImage("depth_cv2", depth);
		*/
		colorMat = image;

		//imshow("ShaurinSucks", colorMat);



		// Tracking or recognition results are ready 
		pxcUID fid; pxcU64 ts; 
		//cout << "QUERY FACE" << endl;
		if (face->QueryFace(0,&fid,&ts)<PXC_STATUS_NO_ERROR) continue; 
		landmark->QueryLandmarkData(fid, linfo.labels, &ldata[0]);
		landmark->QueryPoseData(fid, &pdata);
		det->QueryData(fid,&data); 
		//cout << "DATA   " << data.confidence << " " << data.viewAngle << " " << data.rectangle.x << " " << data.rectangle.y << endl;
		//cout << "PDATA  " << pdata.pitch << " " << pdata.roll << " " << pdata.yaw << endl;
						
		//cout << "LDATA ";
		for (int j = 0; j < 7; j++){
			//cout << ldata[j].position.x << " " << ldata[j].position.y << " " << ldata[j].position.z << endl;
		}

		//Point center( ldata[6].position.x, ldata[6].position.y);
		//ellipse(colorMat, center, Size( 10, 10), 0, 0, 360, Scalar(255, 0, 255), 4, 8, 0);


		cout << ldata[0].position.x << " " << ldata[0].position.y << " | " << ldata[1].position.x << " " << ldata[1].position.y << endl;
		//Point2f start1(ldata[0].position.x, ldata[0].position.y);
		//Point2f end1(ldata[1].position.x, ldata[1].position.y);

		//Point2f start2(ldata[2].position.x, ldata[2].position.y);
		//Point2f end2(ldata[3].position.x, ldata[3].position.y);

		//int rad = abs((ldata[0].position.x+ldata[1].position.x)/2 - ldata[0].position.x);

		//Point2f mid1((ldata[0].position.x+ldata[1].position.x)/2, (ldata[0].position.y+ldata[1].position.y)/2);
		//ellipse(colorMat, mid1, Size( 50, 50), 0, 0, 360, Scalar(255, 0, 255), 4, 8, 0);

		Point2f corner1(ldata[0].position.x, ldata[0].position.y+10);
		Point2f corner2(ldata[1].position.x, ldata[1].position.y-10);

		//rectangle(colorMat, corner1, corner2, Scalar(0, 255, 255), 2, 8);

		Rect temp(corner1, corner2);
		//colorMat(temp);


		//line(colorMat, start1, end1, Scalar(255,0,255), 2, 8);
		//line(colorMat, start2, end2, Scalar(255,0,255), 2, 8);

		imshow("Test", colorMat);
		Mat frame_gray;
		cvtColor(colorMat, frame_gray, CV_BGR2GRAY);
		equalizeHist(frame_gray, frame_gray);
		Mat faceROI = frame_gray(temp);
		imshow("Test2", faceROI);
				
		if( cvWaitKey(10) >= 0 )
		break;
		//IF THIS IS AFTER. THEN THE STREAM STOPS IF FACE ISNT IN THERE
		pipeline.ReleaseFrame();

	}
	
	cvReleaseImage(&image);
	pipeline.Close();
	return 0;
}