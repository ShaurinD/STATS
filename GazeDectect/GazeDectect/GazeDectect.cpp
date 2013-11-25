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

	IplImage *depth=0;
	CvSize gab_size_depth;
	gab_size_depth.height=240;
	gab_size_depth.width=320;
	depth=cvCreateImage(gab_size,8,1);

	PXCImage::ImageData idata;
	PXCImage::ImageData data_depth;

	unsigned char *rgb_data;//=new unsigned char[];
	float *depth_data;
	PXCImage::ImageInfo rgb_info;
	PXCImage::ImageInfo depth_info;

	cvNamedWindow("depth_cv2",0);
	cvResizeWindow("depth_cv2",320,240);

	///////
	for (;;) {
		capture.ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0)); 
		face->ProcessImageAsync(images,sps.ReleaseRef(1)); 
		sps.SynchronizeEx(); 

		if (!pipeline.AcquireFrame(true)) break;
		PXCImage *color_image=pipeline.QueryImage(PXCImage::IMAGE_TYPE_COLOR);
		PXCImage *depth_image=pipeline.QueryImage(PXCImage::IMAGE_TYPE_DEPTH );

		color_image->AcquireAccess(PXCImage::ACCESS_READ_WRITE,PXCImage::COLOR_FORMAT_RGB24,&idata); 
		depth_image->AcquireAccess(PXCImage::ACCESS_READ,&data_depth);

		rgb_data=idata.planes[0];
		depth_data=(float*)data_depth.planes[0];


		pxcStatus stat1 = depth_image->QueryInfo(&depth_info);
		int w1=depth_info.width;
		int h1=depth_info.height;

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

		for(int y=0; y<240; y++)
		{
			for(int x=0; x<320; x++)
			{ 
				depth->imageData[y*320+x]=depth_data[y*320+x]/16;
			}
		}

		color_image->ReleaseAccess(&idata);
		depth_image->ReleaseAccess(&data_depth);

		cvShowImage("rgb_cv", image);
		cvShowImage("depth_cv2", depth);

		/*
		Mat colorMat(image);

		imshow("Shaurin Sucks", colorMat);
		*/
		
		if( cvWaitKey(10) >= 0 )
		break;
		//IF THIS IS AFTER. THEN THE STREAM STOPS IF FACE ISNT IN THERE
		if (!depth_render.RenderFrame(depth_image)) break;
		pipeline.ReleaseFrame();


		// Tracking or recognition results are ready 
		pxcUID fid; pxcU64 ts; 
		//cout << "QUERY FACE" << endl;
		if (face->QueryFace(0,&fid,&ts)<PXC_STATUS_NO_ERROR) continue; 
		landmark->QueryLandmarkData(fid, linfo.labels, &ldata[0]);
		landmark->QueryPoseData(fid, &pdata);
		det->QueryData(fid,&data); 
		cout << "DATA   " << data.confidence << " " << data.viewAngle << " " << data.rectangle.x << " " << data.rectangle.y << endl;
		cout << "PDATA  " << pdata.pitch << " " << pdata.roll << " " << pdata.yaw << endl;
						
		cout << "LDATA ";
		for (int j = 0; j < 7; j++){
			cout << ldata[j].position.x << " " << ldata[j].position.y << " " << ldata[j].position.z << endl;
		}

	}
	
	cvReleaseImage(&image);
	cvReleaseImage(&depth);
	pipeline.Close();
	return 0;
}