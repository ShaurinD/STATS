//=================================
// include guard
#ifndef _BACKGROUND_MASK_CLEANER
#define _BACKGROUND_MASK_CLEANER

//=================================
// forward declared dependencies
//class Foo;
//class Bar;

//=================================
// included dependencies
#include "stdafx.h" 
#include <stdio.h>
#include <opencv\cv.h>

//=================================
// the actual class
class BackgroundMaskCleaner
{
	public:
	BackgroundMaskCleaner();
	~BackgroundMaskCleaner();

	void cleanMask(cv::Mat src);

	private:
	IplConvKernel *se21;
	IplConvKernel *se11;
	//cv::Mat element1;
	//cv::Mat element2;
};

#endif // _BACKGROUND_MASK_CLEANER