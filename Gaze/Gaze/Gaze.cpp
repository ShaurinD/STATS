// Gaze.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "pxcsession.h" 
#include "pxcsmartptr.h"
#include <iostream>
#include <stdio.h>
#include <opencv\cv.h>
#include <opencv\highgui.h>

#include "CaptureStream.h"
#include "BackgroundMaskCleaner.h"

using namespace std;
using namespace cv;

static void help()
{
    // print a welcome message, and the OpenCV version
    cout << "\nThis is a demo of Masking RGB inputs with Depth Data,\n"
		"Using OpenCV version #" << CV_VERSION << "\n"
            << endl;

    cout << "\nHot keys: \n"
            "\tESC - quit the program\n"
			"\tc - toggle depth mask cleaning On/Off (default is On)\n"
			"\td - toggle depth mask On/Off (default is On)\n"
			"\t- - Move max depth threshold in by 10 mm\n"
			"\t+ - Move max depth threshold out by 10mm\n"
			"\tf - toggle FPS display On/Off (default is On)\n" << endl;
}

int main(int argc, char **argv) 
{
	help(); //Debugging stuffz 

	// setup window(s) to view output(s)
	const string rgbWindowName = "RGB";
	namedWindow(rgbWindowName);

	const string maskWindowName = "Mask";
	namedWindow(maskWindowName);

	bool useDepthData = true;
	bool createDepthMask = true;
	bool showFPS = true;
	bool cleanMask = true;

	// initialize the stream capture
	CaptureStream captureStream;
	int captureStatus = captureStream.initStreamCapture();
	if (captureStatus > 0)
	{
		return 3;
	}

	// initialize the background mask cleaner
	BackgroundMaskCleaner maskCleaner;

	int frameCnt = 0;
	int totalMS = 0;
	// this is where we loop to capture frames
	// loop is infinite until the user hits the ESC key.
	while(1)
	{
		// start a count to see how long this takes
		double t = (double)cvGetTickCount();

		// tell the capture stream to advance to the next available frame
		bool streamAdvanceSuccess = captureStream.advanceFrame(useDepthData, createDepthMask);
		if (!streamAdvanceSuccess)
			break;
		
		frameCnt++;

		//t = (double)cvGetTickCount() - t;
		//cout << "time(ms): " << t / ((double)cvGetTickFrequency() * 1000.0F) << endl;

		// create an OpenCV image for working with each frame
		Mat *rgbFrame = captureStream.getCurrentRGBFrame();

		// apply the depth mask (if it's turned on)
		if (useDepthData && createDepthMask)
		{
			// get the depth mask from the capture object
			Mat depthMaskFrame = *captureStream.getCurrentDepthMaskFrame();

			// clean up the mask (is very noisy)
			if (cleanMask)
				maskCleaner.cleanMask(depthMaskFrame);

			// TODO temp show the depth mask frame
			imshow(maskWindowName, depthMaskFrame);

			// create an all black image (for background of masked frame)
			// originally was going to do blue or green, but all of our image processing
			// is done on a grayscale image and black definitely gave the fastest results
			// Other colors seemed to confuse the issue a bit and the face would not be detected
			Mat temp(depthMaskFrame.rows, depthMaskFrame.cols, CV_8UC3, Scalar(0, 0, 0));

			// apply the mask to the image
			temp.copyTo(*rgbFrame, depthMaskFrame);
		}
		
		t = (double)cvGetTickCount() - t;
		totalMS += (int)(t / (cvGetTickFrequency() * 1000.0F));
		// print out the FPS
		if (showFPS)
		{
			int fps = (int)(frameCnt / ((float)totalMS / 1000.0F));
			char text[255]; 
			sprintf(text, "FPS: %d", fps);
			string textStr = text;
			Point org;
			org.x = 10;
			org.y = 20;
			putText(*rgbFrame, textStr, org, FONT_HERSHEY_SIMPLEX, 0.6F, Scalar(0, 255, 0), 1, 8, false);
		}

		// show the frame in the window
		imshow(rgbWindowName, *rgbFrame); //DEBUG STUFFZ

		// check on the frames
		if (frameCnt > 5000)
		{
			frameCnt = 0;
			totalMS = 0;
		}

		// check for user key strokes
		char c = (char)waitKey(1);
		int currentDepth;
		// if they hit the ESC key, bust out of the endless loop
        if (c == 27)
		{
			cout << "Quitting demo......." << endl;
			break;
		}
		//Keep his in for debugging purposes. 
		 switch(c)
        {
			case 'c':
				// toggle the depth mask cleaning
				cleanMask = !cleanMask;
				// reset the FPS counters as well
				frameCnt = 0;
				totalMS = 0;
				break;
			case 'd':
				// toggle the depth data
				createDepthMask = !createDepthMask;
				// reset the FPS counters as well
				frameCnt = 0;
				totalMS = 0;
				break;
			case 'f':
				// toggle the fps display
				showFPS = !showFPS;
				break;
			case '-':
				// move the max depth in
				currentDepth = *captureStream.getMaxDepth() - 10;
				captureStream.setMaxDepth(&currentDepth);
				break;
			case '+':
				// move the max depth out
				currentDepth = *captureStream.getMaxDepth() + 10;
				captureStream.setMaxDepth(&currentDepth);
				break;
		 }
	}

	// destroy the output window(s)
	destroyWindow(rgbWindowName);
	destroyWindow(maskWindowName);

	return 0;
}