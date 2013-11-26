#include "stdafx.h"
#include <Windows.h>
#include <vector>
#include <iostream>
#include "util_pipeline.h"

extern volatile bool g_stop;
volatile bool g_disconnected=false;
/*
void DrawBitmap(HWND,PXCImage*);
void DrawLocation(HWND,PXCFaceAnalysis::Detection::Data*);
void DrawLandmark(HWND,PXCFaceAnalysis::Landmark::LandmarkData*,int);

static bool DisplayDeviceConnection(HWND hwndDlg, bool state) {
    if (state) {
        g_disconnected = true;
    } else {
        g_disconnected = false;
    }
    return g_disconnected;
}
*/
/*
void DisplayLocation(HWND hwndDlg, PXCFaceAnalysis *face) {
	PXCFaceAnalysis::Detection *faced=face->DynamicCast<PXCFaceAnalysis::Detection>();
	for (int i=0;;i++) {
		int fid; 
		if (face->QueryFace(i,&fid)<PXC_STATUS_NO_ERROR) break;
		PXCFaceAnalysis::Detection::Data data;
		if (faced->QueryData(fid,&data)>=PXC_STATUS_NO_ERROR)
			DrawLocation(hwndDlg,&data);
	}
}

void DisplayLandmark(HWND hwndDlg, PXCFaceAnalysis *face) {
	PXCFaceAnalysis::Landmark *facel=face->DynamicCast<PXCFaceAnalysis::Landmark>();
	PXCFaceAnalysis::Landmark::ProfileInfo pinfo;
	facel->QueryProfile(&pinfo);
	int ndata=pinfo.labels&PXCFaceAnalysis::Landmark::LABEL_SIZE_MASK;
	PXCFaceAnalysis::Landmark::LandmarkData *data=new PXCFaceAnalysis::Landmark::LandmarkData[ndata];
	for (int i=0;;i++) {
		int fid; 
		if (face->QueryFace(i,&fid)<PXC_STATUS_NO_ERROR) break;
		if (facel->QueryLandmarkData(fid,pinfo.labels,data)>=PXC_STATUS_NO_ERROR)
			DrawLandmark(hwndDlg,data,ndata);
	}
	delete [] data;
}
*/
// Adnvanced Pipeline without the hwnd
void AdvancedPipeline() {
	std::cout << "HERE" << std::endl;
    PXCSmartPtr<PXCSession> session;
    pxcStatus sts=PXCSession_Create(&session);
    if (sts<PXC_STATUS_NO_ERROR) {
        return;
	}

    /* Set Module */
    PXCSession::ImplDesc desc;
	memset(&desc,0,sizeof(desc));
	const wchar_t* module = L"Face Analysis (Intel)";
	wcscpy_s<sizeof(desc.friendlyName)/sizeof(pxcCHAR)>(desc.friendlyName, module);

    PXCSmartPtr<PXCFaceAnalysis> face;
    sts=session->CreateImpl<PXCFaceAnalysis>(&desc,&face);
    if (sts<PXC_STATUS_NO_ERROR) {
        return;
    }

    /* Set Source */
    PXCSmartPtr<UtilCapture> capture;
	capture=new UtilCapture(session);
	pxcCHAR* device = L"DepthSense Device 325V2";
	capture->SetFilter(device);

    for (int i=0;;i++) {
        PXCFaceAnalysis::ProfileInfo pinfo;
        sts=face->QueryProfile(i,&pinfo);
        if (sts<PXC_STATUS_NO_ERROR) break;
        sts=capture->LocateStreams(&pinfo.inputs);
        if (sts<PXC_STATUS_NO_ERROR) continue;
        sts=face->SetProfile(&pinfo);
        if (sts>=PXC_STATUS_NO_ERROR) break;
    }
    if (sts<PXC_STATUS_NO_ERROR) {
        return;
    }

	/* Set Detection Profile */
	PXCFaceAnalysis::Detection::ProfileInfo pinfo1;
	PXCFaceAnalysis::Detection *faced=face->DynamicCast<PXCFaceAnalysis::Detection>();
	faced->QueryProfile(0,&pinfo1);
	faced->SetProfile(&pinfo1);

	/* Set Landmark Profile */
	PXCFaceAnalysis::Landmark::ProfileInfo pinfo2;
	PXCFaceAnalysis::Landmark *facel=face->DynamicCast<PXCFaceAnalysis::Landmark>();
	facel->QueryProfile(0,&pinfo2);
	facel->SetProfile(&pinfo2);

    PXCSmartArray<PXCImage> images;
    PXCSmartSPArray sps(2);
    while (!g_stop) {
		std::cout << "HERE" << std::endl;
        sts=capture->ReadStreamAsync(images.ReleaseRefs(),sps.ReleaseRef(0));
        if (sts<PXC_STATUS_NO_ERROR) break;

        sts=face->ProcessImageAsync(images,sps.ReleaseRef(1));
        if (sts<PXC_STATUS_NO_ERROR) break;

        sts=sps.SynchronizeEx();
        if (sts<PXC_STATUS_NO_ERROR) break;

        /* Display Results */
        //DrawBitmap(hwndDlg,capture->QueryImage(images,PXCImage::IMAGE_TYPE_COLOR));
        //DisplayLocation(hwndDlg,face);
        //DisplayLandmark(hwndDlg,face);
    }
}