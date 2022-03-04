/************** GENERATED AUTOMATICALLY **************/
#import <Foundation/Foundation.h>
#import "DidimoUnityInterface.h"

@implementation DidimoUnityInterface

const char ** cCharArrayFromNSArray ( NSArray* array ){
    unsigned long i, count = array.count;
    const char **cargs = (const char**) malloc(sizeof(char*) * (count + 1));
    for(i = 0; i < count; i++) {        //cargs is a pointer to 4 pointers to char
        NSString *s      = array[i];     //get a NSString
        const char *cstr = s.UTF8String; //get cstring
        unsigned long        len = strlen(cstr); //get its length
        char  *cstr_copy = (char*) malloc(sizeof(char) * (len + 1));//allocate memory, + 1 for ending '\0'
        strcpy(cstr_copy, cstr);         //make a copy
        cargs[i] = cstr_copy;            //put the point in cargs
    }
    return cargs;
}


typedef void (*BuildDidimoFromDirectoryCb)(const char* didimoDirectory, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static BuildDidimoFromDirectoryCb buildDidimoFromDirectoryCb;
+ (void) registerBuildDidimoFromDirectory:(BuildDidimoFromDirectoryCb)cb{
    
    buildDidimoFromDirectoryCb = cb;
}
+ (void) BuildDidimoFromDirectory:(NSString*)didimoDirectory didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    buildDidimoFromDirectoryCb([didimoDirectory UTF8String], [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*CacheAnimationCb)(const char* animationID, const char* filePath, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static CacheAnimationCb cacheAnimationCb;
+ (void) registerCacheAnimation:(CacheAnimationCb)cb{
    
    cacheAnimationCb = cb;
}
+ (void) CacheAnimation:(NSString*)animationID filePath:(NSString*)filePath successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    cacheAnimationCb([animationID UTF8String], [filePath UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*ClearAnimationCacheCb)(SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static ClearAnimationCacheCb clearAnimationCacheCb;
+ (void) registerClearAnimationCache:(ClearAnimationCacheCb)cb{
    
    clearAnimationCacheCb = cb;
}
+ (void) ClearAnimationCache:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    clearAnimationCacheCb(successCallback, errorCallback, objectPointer);
}


typedef void (*DestroyDidimoCb)(const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static DestroyDidimoCb destroyDidimoCb;
+ (void) registerDestroyDidimo:(DestroyDidimoCb)cb{
    
    destroyDidimoCb = cb;
}
+ (void) DestroyDidimo:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    destroyDidimoCb([didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*GetCameraFrameImageCb)(GetCameraFrameImageSuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static GetCameraFrameImageCb getCameraFrameImageCb;
+ (void) registerGetCameraFrameImage:(GetCameraFrameImageCb)cb{
    
    getCameraFrameImageCb = cb;
}
+ (void) GetCameraFrameImage:(GetCameraFrameImageSuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    getCameraFrameImageCb(successCallback, errorCallback, objectPointer);
}


typedef void (*GetDeformableDataCb)(const char* didimoKey, const char* deformableId, GetDeformableDataSuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static GetDeformableDataCb getDeformableDataCb;
+ (void) registerGetDeformableData:(GetDeformableDataCb)cb{
    
    getDeformableDataCb = cb;
}
+ (void) GetDeformableData:(NSString*)didimoKey deformableId:(NSString*)deformableId successCallback:(GetDeformableDataSuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    getDeformableDataCb([didimoKey UTF8String], [deformableId UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*PlayCinematicCb)(const char* cinematicID, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static PlayCinematicCb playCinematicCb;
+ (void) registerPlayCinematic:(PlayCinematicCb)cb{
    
    playCinematicCb = cb;
}
+ (void) PlayCinematic:(NSString*)cinematicID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    playCinematicCb([cinematicID UTF8String], [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*PlayExpressionCb)(const char* animationID, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static PlayExpressionCb playExpressionCb;
+ (void) registerPlayExpression:(PlayExpressionCb)cb{
    
    playExpressionCb = cb;
}
+ (void) PlayExpression:(NSString*)animationID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    playExpressionCb([animationID UTF8String], [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*ResetCameraCb)(Boolean instant, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static ResetCameraCb resetCameraCb;
+ (void) registerResetCamera:(ResetCameraCb)cb{
    
    resetCameraCb = cb;
}
+ (void) ResetCamera:(Boolean)instant successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    resetCameraCb(instant, successCallback, errorCallback, objectPointer);
}


typedef void (*ScrollToDidimoCb)(int didimoIndex, ProgressCallback progressCallback, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static ScrollToDidimoCb scrollToDidimoCb;
+ (void) registerScrollToDidimo:(ScrollToDidimoCb)cb{
    
    scrollToDidimoCb = cb;
}
+ (void) ScrollToDidimo:(int)didimoIndex progressCallback:(ProgressCallback)progressCallback successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    scrollToDidimoCb(didimoIndex, progressCallback, successCallback, errorCallback, objectPointer);
}


typedef void (*SetEyeColorCb)(int eyeColorID, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetEyeColorCb setEyeColorCb;
+ (void) registerSetEyeColor:(SetEyeColorCb)cb{
    
    setEyeColorCb = cb;
}
+ (void) SetEyeColor:(int)eyeColorID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setEyeColorCb(eyeColorID, [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*SetHairColorCb)(int colorPresetId, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetHairColorCb setHairColorCb;
+ (void) registerSetHairColor:(SetHairColorCb)cb{
    
    setHairColorCb = cb;
}
+ (void) SetHairColor:(int)colorPresetId didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setHairColorCb(colorPresetId, [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*SetHairstyleCb)(const char* styleID, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetHairstyleCb setHairstyleCb;
+ (void) registerSetHairstyle:(SetHairstyleCb)cb{
    
    setHairstyleCb = cb;
}
+ (void) SetHairstyle:(NSString*)styleID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setHairstyleCb([styleID UTF8String], [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*SetOrbitControlsEnabledCb)(Boolean enabled, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetOrbitControlsEnabledCb setOrbitControlsEnabledCb;
+ (void) registerSetOrbitControlsEnabled:(SetOrbitControlsEnabledCb)cb{
    
    setOrbitControlsEnabledCb = cb;
}
+ (void) SetOrbitControlsEnabled:(Boolean)enabled successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setOrbitControlsEnabledCb(enabled, successCallback, errorCallback, objectPointer);
}


typedef void (*SetRenderingActiveCb)(Boolean active, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetRenderingActiveCb setRenderingActiveCb;
+ (void) registerSetRenderingActive:(SetRenderingActiveCb)cb{
    
    setRenderingActiveCb = cb;
}
+ (void) SetRenderingActive:(Boolean)active successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setRenderingActiveCb(active, successCallback, errorCallback, objectPointer);
}


typedef void (*SetViewRectCb)(int x, int y, int width, int height, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetViewRectCb setViewRectCb;
+ (void) registerSetViewRect:(SetViewRectCb)cb{
    
    setViewRectCb = cb;
}
+ (void) SetViewRect:(int)x y:(int)y width:(int)width height:(int)height successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setViewRectCb(x, y, width, height, successCallback, errorCallback, objectPointer);
}


typedef void (*SetupARKitCb)(const char* blendshapeNames, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static SetupARKitCb setupARKitCb;
+ (void) registerSetupARKit:(SetupARKitCb)cb{
    
    setupARKitCb = cb;
}
+ (void) SetupARKit:(NSString*)blendshapeNames didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    setupARKitCb([blendshapeNames UTF8String], [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*StopARKitCb)(const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static StopARKitCb stopARKitCb;
+ (void) registerStopARKit:(StopARKitCb)cb{
    
    stopARKitCb = cb;
}
+ (void) StopARKit:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    stopARKitCb([didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*StopDidimoScrollingCb)(SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static StopDidimoScrollingCb stopDidimoScrollingCb;
+ (void) registerStopDidimoScrolling:(StopDidimoScrollingCb)cb{
    
    stopDidimoScrollingCb = cb;
}
+ (void) StopDidimoScrolling:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    stopDidimoScrollingCb(successCallback, errorCallback, objectPointer);
}


typedef void (*StreamARKitCb)(const void* blendshapeWeights, int blendshapeCount, const char* didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static StreamARKitCb streamARKitCb;
+ (void) registerStreamARKit:(StreamARKitCb)cb{
    
    streamARKitCb = cb;
}
+ (void) StreamARKit:(const void*)blendshapeWeights blendshapeCount:(int)blendshapeCount didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    streamARKitCb(blendshapeWeights, blendshapeCount, [didimoKey UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*TextToSpeechCb)(const char* didimoKey, const char* dataPath, const char* clipPath, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static TextToSpeechCb textToSpeechCb;
+ (void) registerTextToSpeech:(TextToSpeechCb)cb{
    
    textToSpeechCb = cb;
}
+ (void) TextToSpeech:(NSString*)didimoKey dataPath:(NSString*)dataPath clipPath:(NSString*)clipPath successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    textToSpeechCb([didimoKey UTF8String], [dataPath UTF8String], [clipPath UTF8String], successCallback, errorCallback, objectPointer);
}


typedef void (*UpdateDeformableCb)(const char* didimoKey, const char* deformableId, const void* deformedData, int dataSize, SuccessCallback successCallback, ErrorCallback errorCallback, const void* objectPointer);
static UpdateDeformableCb updateDeformableCb;
+ (void) registerUpdateDeformable:(UpdateDeformableCb)cb{
    
    updateDeformableCb = cb;
}
+ (void) UpdateDeformable:(NSString*)didimoKey deformableId:(NSString*)deformableId deformedData:(const void*)deformedData dataSize:(int)dataSize successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer{
    
    updateDeformableCb([didimoKey UTF8String], [deformableId UTF8String], deformedData, dataSize, successCallback, errorCallback, objectPointer);
}



@end

#if __cplusplus
extern "C" {
#endif
      
    
    void registerBuildDidimoFromDirectory(BuildDidimoFromDirectoryCb cb) {
 
        [DidimoUnityInterface registerBuildDidimoFromDirectory: cb];
    }
    
    void registerCacheAnimation(CacheAnimationCb cb) {
 
        [DidimoUnityInterface registerCacheAnimation: cb];
    }
    
    void registerClearAnimationCache(ClearAnimationCacheCb cb) {
 
        [DidimoUnityInterface registerClearAnimationCache: cb];
    }
    
    void registerDestroyDidimo(DestroyDidimoCb cb) {
 
        [DidimoUnityInterface registerDestroyDidimo: cb];
    }
    
    void registerGetCameraFrameImage(GetCameraFrameImageCb cb) {
 
        [DidimoUnityInterface registerGetCameraFrameImage: cb];
    }
    
    void registerGetDeformableData(GetDeformableDataCb cb) {
 
        [DidimoUnityInterface registerGetDeformableData: cb];
    }
    
    void registerPlayCinematic(PlayCinematicCb cb) {
 
        [DidimoUnityInterface registerPlayCinematic: cb];
    }
    
    void registerPlayExpression(PlayExpressionCb cb) {
 
        [DidimoUnityInterface registerPlayExpression: cb];
    }
    
    void registerResetCamera(ResetCameraCb cb) {
 
        [DidimoUnityInterface registerResetCamera: cb];
    }
    
    void registerScrollToDidimo(ScrollToDidimoCb cb) {
 
        [DidimoUnityInterface registerScrollToDidimo: cb];
    }
    
    void registerSetEyeColor(SetEyeColorCb cb) {
 
        [DidimoUnityInterface registerSetEyeColor: cb];
    }
    
    void registerSetHairColor(SetHairColorCb cb) {
 
        [DidimoUnityInterface registerSetHairColor: cb];
    }
    
    void registerSetHairstyle(SetHairstyleCb cb) {
 
        [DidimoUnityInterface registerSetHairstyle: cb];
    }
    
    void registerSetOrbitControlsEnabled(SetOrbitControlsEnabledCb cb) {
 
        [DidimoUnityInterface registerSetOrbitControlsEnabled: cb];
    }
    
    void registerSetRenderingActive(SetRenderingActiveCb cb) {
 
        [DidimoUnityInterface registerSetRenderingActive: cb];
    }
    
    void registerSetViewRect(SetViewRectCb cb) {
 
        [DidimoUnityInterface registerSetViewRect: cb];
    }
    
    void registerSetupARKit(SetupARKitCb cb) {
 
        [DidimoUnityInterface registerSetupARKit: cb];
    }
    
    void registerStopARKit(StopARKitCb cb) {
 
        [DidimoUnityInterface registerStopARKit: cb];
    }
    
    void registerStopDidimoScrolling(StopDidimoScrollingCb cb) {
 
        [DidimoUnityInterface registerStopDidimoScrolling: cb];
    }
    
    void registerStreamARKit(StreamARKitCb cb) {
 
        [DidimoUnityInterface registerStreamARKit: cb];
    }
    
    void registerTextToSpeech(TextToSpeechCb cb) {
 
        [DidimoUnityInterface registerTextToSpeech: cb];
    }
    
    void registerUpdateDeformable(UpdateDeformableCb cb) {
 
        [DidimoUnityInterface registerUpdateDeformable: cb];
    }
#if __cplusplus

}
#endif

