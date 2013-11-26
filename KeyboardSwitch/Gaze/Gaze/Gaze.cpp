// Gaze.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


#include "pxcsession.h" 
#include "pxcsmartptr.h"
#include <windows.h>
#include "wtypes.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <math.h>
#include <float.h>
#include <limits.h>
#include <time.h>
#include <ctype.h>
#include <vector>
#include <iostream>
#include <fstream>
#include <sstream>
#include <opencv\cv.h>
#include <opencv\highgui.h>

#include "CaptureStream.h"
#include "BackgroundMaskCleaner.h"

#include "opencv2/core/core.hpp"
#include "opencv2/contrib/contrib.hpp"
#include "opencv2/highgui/highgui.hpp"
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/objdetect/objdetect.hpp>

using namespace cv;
using namespace std;


String face_cascade_name = "haarcascade_frontalface_alt.xml";
 String eyes_cascade_name = "haarcascade_eye_tree_eyeglasses.xml";
 CascadeClassifier face_cascade;
 CascadeClassifier eyes_cascade;
 string window_name = "Capture - Face detection";
 RNG rng(12345);
 // function declaration for drawing the region of interest around the face
void drawFaceROIFromRect(IplImage *src, CvRect *rect);

// function declaration for finding good features to track in a region
int findFeatures(IplImage *src, CvPoint2D32f *features, CvBox2D roi);

// function declaration for finding a trackbox around an array of points
CvBox2D findTrackBox(CvPoint2D32f *features, int numPoints);

// function declaration for finding the distance a point is from a given cluster of points
int findDistanceToCluster(CvPoint2D32f point, CvPoint2D32f *cluster, int numClusterPoints);

// Storage for the previous gray image
IplImage *prevGray = 0;
// Storage for the previous pyramid image
IplImage *prevPyramid = 0;
// for working with the current frame in grayscale
IplImage *gray = 0;
// for working with the current frame in grayscale2 (for L-K OF)
IplImage *pyramid = 0;

// max features to track in the face region
int const MAX_FEATURES_TO_TRACK = 300;
// max features to add when we search on top of an existing pool of tracked points
int const MAX_FEATURES_TO_ADD = 300;
// min features that we can track in a face region before we fail back to face detection
int const MIN_FEATURES_TO_RESET = 6;
// the threshold for the x,y mean squared error indicating that we need to scrap our current track and start over
float const MSE_XY_MAX = 10000;
// threshold for the standard error on x,y points we're tracking
float const STANDARD_ERROR_XY_MAX = 3;
// threshold for the standard error on x,y points we're tracking
double const EXPAND_ROI_INIT = 1.02;
// max distance from a cluster a new tracking can be
int const ADD_FEATURE_MAX_DIST = 20;


Point detectAndDisplay( Mat frame );

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

    if( !face_cascade.load( face_cascade_name ) ){ printf("--(!)Error loading\n"); return -1; };
    if( !eyes_cascade.load( eyes_cascade_name ) ){ printf("--(!)Error loading\n"); return -1; };

// name the window
	const char *kaskadewindowName = "Robust Face Detection v0.1a";
	// for testing if the stream is finished
	bool finished = false;
	// for storing the features
	CvPoint2D32f features[MAX_FEATURES_TO_TRACK] = {0};
	// for storing the number of current features that we're tracking
	int numFeatures = 0;
	// box for defining the region where a features are being tracked
	CvBox2D featureTrackBox;
	// multiplier for expanding the trackBox
	float expandROIMult = 1.02;
	// threshold number for adding more features to the region
	int minFeaturesToNewSearch = 50;
	vector<Rect> faces;
	// Create a new window 
	namedWindow(kaskadewindowName);



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
	
	
	Mat* currentFrame = captureStream.getCurrentDepthMaskFrame();

	// init the images
	//Size testsize = currentFrame->size();
		//getSize(currentFrame);
	prevGray = cvCreateImage(currentFrame->size(), IPL_DEPTH_8U, 1);
	prevPyramid = cvCreateImage(currentFrame->size(), IPL_DEPTH_8U, 1);
	gray = cvCreateImage(currentFrame->size(), IPL_DEPTH_8U, 1);
	pyramid = cvCreateImage(currentFrame->size(), IPL_DEPTH_8U, 1);

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
		
		Point eyeLocation = detectAndDisplay(*rgbFrame);
		Size s = rgbFrame->size();
		double FrameWidth = s.width;
		double FrameHeight = s.height;
		double horiz, vert;
		RECT desktop;
		const HWND hDesktop = GetDesktopWindow();
		GetWindowRect(hDesktop, &desktop);
		horiz = desktop.right;
		vert = desktop.bottom;
		double cursorPosX = (eyeLocation.x / (double)s.width) * horiz;
		double cursorPosY = (eyeLocation.y / (double)s.height) * vert;

		cout << "CURPOSX:  " << cursorPosX << "Y TIME " << cursorPosY << endl;
		SetCursorPos((int)cursorPosX, (int)cursorPosY);
		

		/*
		//Detect and Display Face detection 
		cvtColor(*rgbFrame, (Mat)gray, CV_BGR2GRAY );
		equalizeHist( (Mat)gray, (Mat)gray );
		if (!faces.empty()) {
			// try for a new face detect rect for the next frame
			if (numFeatures > 0) {
				bool died = false;
				char featureStatus[MAX_FEATURES_TO_TRACK];
				float featureErrors[MAX_FEATURES_TO_TRACK];
				CvSize pyramidSize = cvSize(gray->width + 8, gray->height / 3);
				CvPoint2D32f *featuresB = new CvPoint2D32f[MAX_FEATURES_TO_TRACK];
				CvPoint2D32f *tempFeatures = new CvPoint2D32f[MAX_FEATURES_TO_TRACK];
				cvCalcOpticalFlowPyrLK(prevGray, gray, prevPyramid, pyramid, features, featuresB, numFeatures, cvSize(10,10), 5, featureStatus, featureErrors, cvTermCriteria(CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, -3), 0);
				numFeatures = 0;
				float sumX = 0;
				float sumY = 0;
				float meanX = 0;
				float meanY = 0;
				// copy back to features, but keep only high status points
				// and count the number using numFeatures

				for (int i = 0; i < MAX_FEATURES_TO_TRACK; i++) {
					if (featureStatus[i]) {
						// quick prune just by checking if the point is outside the image bounds
						if (featuresB[i].x < 0 || featuresB[i].y < 0 || featuresB[i].x > gray->width || featuresB[i].y > gray->height) {
							// do nothing
						}
						else {
							// count the good values
							tempFeatures[numFeatures] = featuresB[i];
							numFeatures++;
							// sum up to later calc the mean for x and y
							sumX += featuresB[i].x;
							sumY += featuresB[i].y;
						}
					}
				}	
				// calc the means
				meanX = sumX / numFeatures;
				meanY = sumY / numFeatures;

				// prune points using mean squared error
				// caclulate the squaredError for x, y (square of the distance from the mean)
				float squaredErrorXY = 0;
				for (int i = 0; i < numFeatures; i++) {
					squaredErrorXY += (tempFeatures[i].x - meanX) * (tempFeatures[i].x - meanX) + (tempFeatures[i].y  - meanY) * (tempFeatures[i].y - meanY);
				}
				// calculate mean squared error for x,y
				float meanSquaredErrorXY = squaredErrorXY / numFeatures;

				 // mean squared error must be greater than 0 but less than our threshold (big number that would indicate our points are insanely spread out)
				 if (meanSquaredErrorXY == 0 || meanSquaredErrorXY > MSE_XY_MAX) {
					 numFeatures = 0;
					 died = true;
				 }
				 else {
				 // Throw away the outliers based on the x-y variance
				 // store the good values in the features array
					 int cnt = 0;
					 for (int i = 0; i < numFeatures; i++) {
						 float standardErrorXY = ((tempFeatures[i].x - meanX) * (tempFeatures[i].x - meanX) + (tempFeatures[i].y - meanY) * (tempFeatures[i].y - meanY)) / meanSquaredErrorXY;
						 if (standardErrorXY < STANDARD_ERROR_XY_MAX) {
							 // we want to keep this point
							 features[cnt] = tempFeatures[i];
							 cnt++;
						 }
					  }
					numFeatures = cnt;
				 // only bother with fixing the tail of the features array if we still have points to track
				 if (numFeatures > 0) {
					 // set everything past numFeatures to -10,-10 in our updated features array
					for (int i = numFeatures; i < MAX_FEATURES_TO_TRACK; i++) {
							features[i] = cvPoint2D32f(-10,-10);
					}
				 }
			}
				// check if we're below the threshold min points to track before adding new ones
			if (numFeatures < minFeaturesToNewSearch) {
					// add new features
					// up the multiplier for expanding the region
					expandROIMult *= EXPAND_ROI_INIT;

					// expand the trackBox
					 float newWidth = featureTrackBox.size.width * expandROIMult;
					 float newHeight = featureTrackBox.size.height * expandROIMult;
					 CvSize2D32f newSize = cvSize2D32f(newWidth, newHeight);
					 CvBox2D newRoiBox = {featureTrackBox.center, newSize, featureTrackBox.angle};

					// find new points
					 CvPoint2D32f additionalFeatures[MAX_FEATURES_TO_ADD] = {0};
					 int numAdditionalFeatures = findFeatures(gray, additionalFeatures, newRoiBox);
					 int endLoop = MAX_FEATURES_TO_ADD;
					 if (MAX_FEATURES_TO_TRACK < endLoop + numFeatures)
						endLoop -= numFeatures + endLoop - MAX_FEATURES_TO_TRACK;
					// copy new stuff to features, but be mindful of the array max
					for (int i = 0; i < endLoop; i++) {
						 // TODO check if they are way outside our stuff????
						 int dist = findDistanceToCluster(additionalFeatures[i], features, numFeatures);
						 if (dist < ADD_FEATURE_MAX_DIST) {
							features[numFeatures] = additionalFeatures[i];
							numFeatures++;
						 }
					} 
					 // check if we're below the reset min
					 if (numFeatures < MIN_FEATURES_TO_RESET) {
						 // if so, set to numFeatures 0, null out the detect rect and do face detection on the next frame
						 numFeatures = 0;
						 died = true;
					 }
				}
				else {
					// reset the expand roi mult back to the init
					// since this frame didn't need an expansion
					expandROIMult = EXPAND_ROI_INIT;
				}
				// find the new track box
				if (!died)
					featureTrackBox = findTrackBox(features, numFeatures);
			}
			else {
				// convert the faceDetectRect to a CvBox2D
				CvPoint2D32f center = cvPoint2D32f(faces[0].x + faces[0].width * 0.5, faces[0].y + faces[0].height * 0.5);
				CvSize2D32f size = cvSize2D32f(faces[0].width, faces[0].height);
				CvBox2D roiBox = {center, size, 0};
				// get features to track
				 numFeatures = findFeatures(gray, features, roiBox);

				 // verify that we found features to track on this frame
				 if (numFeatures > 0) {
					 // find the corner subPix
					 cvFindCornerSubPix(gray, features, numFeatures, cvSize(10, 10), cvSize(-1,-1), cvTermCriteria(CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));

					 // define the featureTrackBox around our new points
					 featureTrackBox = findTrackBox(features, numFeatures);
					 // calculate the minFeaturesToNewSearch from our detected face values
					 minFeaturesToNewSearch = 0.9 * numFeatures;

					// wait for the next frame to start tracking using optical flow
				 }
				 else {
					//-- Detect faces
					face_cascade.detectMultiScale( gray, faces, 1.1, 2, 0|CV_HAAR_SCALE_IMAGE, Size(30, 30) );
				 }
			}
		}
		else {
			 // reset the current features
			 numFeatures = 0;
			 // try for a new face detect rect for the next frame
			 face_cascade.detectMultiScale( gray, faces, 1.1, 2, 0|CV_HAAR_SCALE_IMAGE, Size(30, 30) );
		}

		// save gray and pyramid frames for next frame
		cvCopy(gray, prevGray, 0);
		cvCopy(pyramid, prevPyramid, 0);

		// draw some stuff into the frame to show results
		if (numFeatures > 0) {
			// show the features as little dots
			//Features is not being modified
			for(int i = 0; i < numFeatures; i++) {
				CvPoint myPoint = cvPointFrom32f(features[i]);
				cvCircle(rgbFrame, cvPointFrom32f(features[i]), 2, CV_RGB(0, 255, 0), CV_FILLED);
			}
			
			// show the tracking box as an ellipse
			cvEllipseBox(rgbFrame, featureTrackBox, CV_RGB(0, 0, 255), 3);
		}
 */
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

Point detectAndDisplay( Mat frame )
{
  std::vector<Rect> faces;
  Mat frame_gray;
  Point eyeLocation;
  cvtColor( frame, frame_gray, CV_BGR2GRAY );
  equalizeHist( frame_gray, frame_gray );

  //-- Detect faces
  face_cascade.detectMultiScale( frame_gray, faces, 1.1, 2, 0|CV_HAAR_SCALE_IMAGE, Size(30, 30) );

  for( int i = 0; i < faces.size(); i++ )
  {
    Point center( faces[i].x + faces[i].width*0.5, faces[i].y + faces[i].height*0.5 );
    ellipse( frame, center, Size( faces[i].width*0.5, faces[i].height*0.5), 0, 0, 360, Scalar( 255, 0, 255 ), 4, 8, 0 );

    Mat faceROI = frame_gray( faces[i] );
    std::vector<Rect> eyes;
	
    //-- In each face, detect eyes
    eyes_cascade.detectMultiScale( faceROI, eyes, 1.1, 2, 0 |CV_HAAR_SCALE_IMAGE, Size(30, 30) );

    for( int j = 0; j < eyes.size(); j++ )
     {
       Point center( faces[i].x + eyes[j].x + eyes[j].width*0.5, faces[i].y + eyes[j].y + eyes[j].height*0.5 );
	   if (i ==0 && j == 0)
		eyeLocation = center;
       int radius = cvRound( (eyes[j].width + eyes[j].height)*0.25 );
       circle( frame, center, radius, Scalar( 255, 0, 0 ), 4, 8, 0 );
     }
	 
  }
  //-- Show what you got
  imshow( window_name, frame );

  return eyeLocation;
 }

// draws a region of interest in the src frame based on the given rect
void drawFaceROIFromRect(IplImage *src, CvRect *rect)
{
 // Points to draw the face rectangle
 CvPoint pt1 = cvPoint(0, 0);
 CvPoint pt2 = cvPoint(0, 0);

 // setup the points for drawing the rectangle
 pt1.x = rect->x;
 pt1.y = rect->y;
 pt2.x = pt1.x + rect->width;
 pt2.y = pt1.y + rect->height;

 // Draw face rectangle
 cvRectangle(src, pt1, pt2, CV_RGB(255,0,0), 2, 8, 0 );
}

// finds features and stores them in the given array
// TODO move this method into a Class
int findFeatures(IplImage *src, CvPoint2D32f *features, CvBox2D roi)
{
 //cout << "findFeatures" << endl;
 int featureCount = 0;
 double minDistance = 5;
 double quality = 0.01;
 int blockSize = 3;
 int useHarris = 0;
 double k = 0.04;

 // Create a mask image to be used to select the tracked points
 IplImage *mask = cvCreateImage(cvGetSize(src), IPL_DEPTH_8U, 1);

 // Begin with all black pixels
 cvZero(mask);

 // Create a filled white ellipse within the box to define the ROI in the mask.
 cvEllipseBox(mask, roi, CV_RGB(255, 255, 255), CV_FILLED);
            
 // Create the temporary scratchpad images
 IplImage *eig = cvCreateImage(cvGetSize(src), IPL_DEPTH_8U, 1);
 IplImage *temp = cvCreateImage(cvGetSize(src), IPL_DEPTH_8U, 1);

 // init the corner count int
 int cornerCount = MAX_FEATURES_TO_TRACK;

 // Find keypoints to track using Good Features to Track
 cvGoodFeaturesToTrack(src, eig, temp, features, &cornerCount, quality, minDistance, mask, blockSize, useHarris, k);

 // iterate through the array
 for (int i = 0; i < cornerCount; i++)
 {
 if ((features[i].x == 0 && features[i].y == 0) || features[i].x > src->width || features[i].y > src->height)
 {
 // do nothing
 }
 else
 {
 featureCount++;
 }
 }
 //cout << "nfeatureCount = " << featureCount << endl;

 // return the feature count
 return featureCount;
}

// finds the track box for a given array of 2d points
// TODO move this method into a Class
CvBox2D findTrackBox(CvPoint2D32f *points, int numPoints)
{
 //cout << "findTrackBox" << endl;
 //cout << "numPoints: " << numPoints << endl;
 CvBox2D box;
 // matrix for helping calculate the track box 
 CvMat *featureMatrix = cvCreateMat(1, numPoints, CV_32SC2);
 // collect the feature points in the feature matrix
 for(int i = 0; i < numPoints; i++)
 cvSet2D(featureMatrix, 0, i, cvScalar(points[i].x, points[i].y));
 // create an ellipse off of the featureMatrix
 box = cvFitEllipse2(featureMatrix);
 // release the matrix (cause we're done with it)
 cvReleaseMat(&featureMatrix);
 // return the box
 return box;
}

int findDistanceToCluster(CvPoint2D32f point, CvPoint2D32f *cluster, int numClusterPoints)
{
 int minDistance = 10000;
 for (int i = 0; i < numClusterPoints; i++)
 {
 int distance = abs(point.x - cluster[i].x) + abs(point.y - cluster[i].y);
 if (distance < minDistance)
 minDistance = distance;
 }
 return minDistance;
}