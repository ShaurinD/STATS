#include "stdafx.h"
#include <opencv/cv.h>
#include "opencv2/highgui/highgui.hpp"
#include <stdio.h>
#include <future>
#include <Windows.h>
#include "wtypes.h"
#include <WinUser.h>
#include "util_render.h"
#include "util_pipeline.h"
#include <iostream>
#include <cmath>

using namespace std;
using namespace cv;

#define PI 3.14159265

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
	PXCSmartArray<PXCImage> images; 
	PXCSmartSPArray sps(2);
	//Enabling image stream
	UtilPipeline pipeline;
	pipeline.EnableImage(PXCImage::COLOR_FORMAT_RGB24,640,480);
	pipeline.Init();

	//OpenCV declarations
	IplImage *image;
	CvSize gab_size;
	gab_size.height=480;
	gab_size.width=640;
	image=cvCreateImage(gab_size,8,3);
	PXCImage::ImageData idata;
	unsigned char *rgb_data;
	Mat colorMat;

	//Desktop Window Information
	RECT desktop;
	const HWND hDesktop = GetDesktopWindow();
	GetWindowRect(hDesktop, &desktop);
	double horiz = desktop.right;
	double vert = desktop.bottom;

	int up = 0, down = 0, left = 0, right = 0;

	// Frame capturing and data processing loop
	for (int q = 0;;q++) {
		capture.ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0)); 
		face->ProcessImageAsync(images,sps.ReleaseRef(1)); 
		sps.SynchronizeEx(); 

		if (!pipeline.AcquireFrame(true)) break;
		PXCImage *color_image=pipeline.QueryImage(PXCImage::IMAGE_TYPE_COLOR);
		color_image->AcquireAccess(PXCImage::ACCESS_READ_WRITE,PXCImage::COLOR_FORMAT_RGB24,&idata); 
		rgb_data=idata.planes[0];
		for(int y=0; y<480; y++) {
			for(int x=0; x<640; x++) { 
				for(int k=0; k<3 ; k++)	{
					image->imageData[y*640*3+x*3+k]= rgb_data[y*640*3+x*3+k];
				}
			}
		}
		color_image->ReleaseAccess(&idata);
		colorMat = image;
		
		// Tracking or recognition results are ready 
		pxcUID fid; pxcU64 ts; 
		if (face->QueryFace(0,&fid,&ts)<PXC_STATUS_NO_ERROR) continue; 
		landmark->QueryLandmarkData(fid, linfo.labels, ldata);
		landmark->QueryPoseData(fid, &pdata);
		det->QueryData(fid,&data); 
		//If position has meaning
		if (ldata[0].position.x >= 1) {
			Point2f lcorner1(ldata[0].position.x-1, ldata[0].position.y+20);
			Point2f lcorner2(ldata[1].position.x+1, ldata[1].position.y-20);
			Point2f rcorner1(ldata[2].position.x-1, ldata[2].position.y+20);
			Point2f rcorner2(ldata[3].position.x+1, ldata[3].position.y-20);
			Rect leyeBox(lcorner1, lcorner2);
			Rect reyeBox(rcorner1, rcorner2);
			
			Mat frame_gray; 
			cvtColor(colorMat, frame_gray, CV_RGB2GRAY);
			equalizeHist(frame_gray, frame_gray);
			Mat LfaceROI = frame_gray(leyeBox);
			Mat RfaceROI = frame_gray(reyeBox);
			

			Point lpupil, rpupil;
			minMaxLoc(LfaceROI, NULL, NULL, &lpupil);
			minMaxLoc(RfaceROI, NULL, NULL, &rpupil);
			//Getting midpoints of each eye
			Point2f lcenter((ldata[0].position.x+ldata[1].position.x)/2, (ldata[0].position.y+ldata[1].position.y)/2);
			Point2f rcenter((ldata[2].position.x+ldata[3].position.x)/2, (ldata[2].position.y+ldata[3].position.y)/2);
			
			//Creating a point for the nose
			Point2f nose(ldata[6].position.x, ldata[6].position.y);
			//Creating Points for each pupil
			Point2f leftpup(lcorner1.x + lpupil.x, lcorner1.y - lpupil.y - 10);
			Point2f rightpup(rcorner1.x + rpupil.x, rcorner1.y - rpupil.y - 10);
			
			ellipse(colorMat, leftpup, Size(2, 2), 0, 0, 360, Scalar(100,100,100),2,8);
			imshow("im", colorMat);
			if( cvWaitKey(10) >= 0 ) break;
			
			//Cursor control logic
			//Getting 100 samples
			if (q%100 != 0) {
				//getting counts for which direction the pupil is moving
				if ((leftpup.x > lcorner1.x+leyeBox.width*.3) && (leftpup.x < lcorner1.x+leyeBox.width*.7)) {
					if ((leftpup.y > lcorner1.y - leyeBox.height/2 - 6) ) {
						up++;
					}
					else {
						down++;
					}
				}
				else if (leftpup.x < lcorner1.x+leyeBox.width*.3) {
					right++;
				}
				else {
					left++;
				}
			}
			else {
				POINT cursorPos;
				GetCursorPos(&cursorPos);
				vector<int> hits;
				hits.push_back(up);
				hits.push_back(down);
				hits.push_back(left);
				hits.push_back(right);
				sort(hits.begin(), hits.end());
				// Move cursor in the direction with the highest clicks
				if (left == hits[3]) {
					for (int x = cursorPos.x; x > 0; x--) {
						SetCursorPos(x, cursorPos.y);
						Sleep(50);
						//Break cursor movement on press of CNTRL
						if (GetAsyncKeyState(VK_CONTROL) != 0) {
							break;
						}
					}
				}
				else if (right == hits[3]) {
					for (int x = cursorPos.x; x < horiz; x++) {
						SetCursorPos(x, cursorPos.y);
						Sleep(50);
						if (GetAsyncKeyState(VK_CONTROL) != 0) {
							break;
						}
					}
				}
				else if (up == hits[3]) {
					for (int y = cursorPos.y; y > 0; y--) {
						SetCursorPos(cursorPos.x, y);
						Sleep(50);
						if (GetAsyncKeyState(VK_CONTROL) != 0) {
							break;
						}
					}
				}
				else {
					for (int y = cursorPos.y; y < vert; y++) {
						SetCursorPos(cursorPos.x, y);
						Sleep(50);
						if (GetAsyncKeyState(VK_CONTROL) != 0) {
							break;
						}
					}
				}
				up = 0, down = 0, left = 0, right = 0;
			}
		}
		//IF THIS IS AFTER. THEN THE STREAM STOPS IF FACE ISNT IN THERE
		pipeline.ReleaseFrame();
		
	}
	
	cvReleaseImage(&image);
	pipeline.Close();
	return 0;
}