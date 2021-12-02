//
//  DidimoUnityInterface.m
//  UnityFramework
//
//  Created by Hugo Pereira on 05/04/2021.
//

#import <Foundation/Foundation.h>
#import "DidimoUnityInterface.h"

@implementation DidimoUnityInterface

typedef void (*BuildDidimoCb)(const void* object, const char* didimoDirectory, const char* didimoKey, SuccessCb success, ErrorCb error);
static BuildDidimoCb buildDidimoCb;
+ (void) registerBuildDidimo:(BuildDidimoCb)cb{
    
    buildDidimoCb = cb;
}
+ (void) BuildDidimoFromDirectory:(NSString*)didimoDirectory didimoKey:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    buildDidimoCb(ptr, [didimoDirectory UTF8String], [didimoKey UTF8String], success, error);
}

typedef void (*SetHairColorCb)(const void* object,const char* didimoKey, int colorIdx, SuccessCb success, ErrorCb error);
static SetHairColorCb setHairColorCb;
+ (void) registerSetHairColor:(SetHairColorCb)cb{
    
    setHairColorCb = cb;
}
+ (void) SetHairColor:(int)colorIdx forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    setHairColorCb(ptr, [didimoKey UTF8String], colorIdx, success, error);
}

typedef void (*SetHairstyleCb)(const void* object,const char* didimoKey, const char* styleId, SuccessCb success, ErrorCb error);
static SetHairstyleCb setHairstyleCb;
+ (void) registerSetHairstyle:(SetHairstyleCb)cb{
    
    setHairstyleCb = cb;
}
+ (void) SetHairstyleId:(NSString*)styleId forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    setHairstyleCb(ptr, [didimoKey UTF8String], [styleId UTF8String], success, error);
}

typedef void (*SetEyeColorCb)(const void* object,const char* didimoKey, int colorIdx, SuccessCb success, ErrorCb error);
static SetEyeColorCb setEyeColorCb;
+ (void) registerSetEyeColor:(SetEyeColorCb)cb{
    
    setEyeColorCb = cb;
}
+ (void) SetEyeColor:(int)colorIdx forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    setEyeColorCb(ptr, [didimoKey UTF8String], colorIdx, success, error);
}

typedef void (*PlayExpressionCb)(const void* object,const char* didimoKey,const char* animationId, SuccessCb success, ErrorCb error);
static PlayExpressionCb playExpressionCb;
+ (void) registerPlayExpression:(PlayExpressionCb)cb{
    
    playExpressionCb = cb;
}
+ (void) PlayExpressionWithdidimoKey:(NSString*)didimoKey animationId:(NSString*)animationId successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    playExpressionCb(ptr, [didimoKey UTF8String], [animationId UTF8String], success, error);
}

typedef void (*DestroyDidimoCb)(const void* object,const char* didimoKey, SuccessCb success, ErrorCb error);
static DestroyDidimoCb destroyDidimoCb;
+ (void) registerDestroyDidimo:(DestroyDidimoCb)cb{
    
    destroyDidimoCb = cb;
}
+ (void) DestroyDidimoWithId:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    destroyDidimoCb(ptr, [didimoKey UTF8String], success, error);
}

typedef void (*CacheAnimationCb)(const void* object,const char* animationId, const char* filePath, SuccessCb success, ErrorCb error);
static CacheAnimationCb cacheAnimationCb;
+ (void) registerCacheAnimation:(CacheAnimationCb)cb{
    
    cacheAnimationCb = cb;
}
+ (void)  CacheAnimationWithId:(NSString*)animationId filePath:(NSString*)filePath successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    cacheAnimationCb(ptr, [animationId UTF8String], [filePath UTF8String], success, error);
}

typedef void (*ClearAnimationCacheCb)(const void* object, SuccessCb success, ErrorCb error);
static ClearAnimationCacheCb clearAnimationCacheCb;
+ (void) registerClearAnimationCache:(ClearAnimationCacheCb)cb{
    
    clearAnimationCacheCb = cb;
}
+ (void)  ClearAnimationCacheWithSuccessCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    clearAnimationCacheCb(ptr, success, error);
}

typedef void (*SetOrbitControlsCb)(const void* object,bool enabled, SuccessCb success, ErrorCb error);
static SetOrbitControlsCb setOrbitControlsCb;
+ (void) registerSetOrbitControls:(SetOrbitControlsCb)cb{
    
    setOrbitControlsCb = cb;
}
+ (void) SetOrbitControlsEnabled:(bool)enabled successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    setOrbitControlsCb(ptr, enabled, success, error);
}

typedef void (*TextToSpeechCb)(const void* object,const char* didimoKey, const char* dataPath, const char* clipPath, SuccessCb success, ErrorCb error);
static TextToSpeechCb textToSpeechCb;
+ (void) registerTextToSpeech:(TextToSpeechCb)cb{
    
    textToSpeechCb = cb;
}
+ (void) TextToSpeechForDidimo:(NSString*)didimoKey dataPath:(NSString*)dataPath clipPath:(NSString*)clipPath successCallback:(SuccessCb) success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    textToSpeechCb(ptr, [didimoKey UTF8String], [dataPath UTF8String], [clipPath UTF8String], success, error);
}


typedef void (*GetDeformableDataCb)(const void* object,const char* didimoKey, const char* deformableId, GetDeformableDataSuccessCb success, ErrorCb error);
static GetDeformableDataCb getDeformableDataCb;
+ (void) registerGetDeformableData:(GetDeformableDataCb)cb{
    
    getDeformableDataCb = cb;
}

+ (void) GetDeformableDataForDidimo:(NSString*)didimoKey deformableId:(NSString*)deformableId successCallback:(GetDeformableDataSuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    return getDeformableDataCb(ptr, [didimoKey UTF8String], [deformableId UTF8String], success, error);
}

typedef void (*UpdateDeformableCb)(const void* object,const char* didimoKey, const char* deformableId, char* deformedData, int dataSize, SuccessCb success, ErrorCb error);
static UpdateDeformableCb updateDeformableCb;
+ (void) registerUpdateDeformable:(UpdateDeformableCb)cb{
    
    updateDeformableCb = cb;
}

+ (void) UpdateDeformableForDidimo:(NSString*)didimoKey deformableId:(NSString*)deformableId deformedData:(void*)deformedData dataSize:(int)dataSize successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    updateDeformableCb(ptr, [didimoKey UTF8String], [deformableId UTF8String], deformedData, dataSize, success, error);
}

typedef void (*SetCameraCb)(const void* object,float* position, float* rotation, SuccessCb success, ErrorCb error);
static SetCameraCb setCameraCb;
+ (void) registerSetCamera:(SetCameraCb)cb{
    
    setCameraCb = cb;
}
+ (void) SetCameraPosition:(float[_Nonnull 3])position rotation:(float[_Nonnull 4])rotation successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    setCameraCb(ptr, position, rotation, success, error);
}

typedef void (*ResetCameraCb)(const void* object,bool instant, SuccessCb success, ErrorCb error);
static ResetCameraCb resetCameraCb;
+ (void) registerResetCamera:(ResetCameraCb)cb{
    
    resetCameraCb = cb;
}
+ (void) ResetCameraInstant:(bool)instant successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    resetCameraCb(ptr, instant, success, error);
}

typedef void (*GetCameraFrameImageCb)(const void* object, GetCameraFrameImageSuccessCb success, ErrorCb error);
static GetCameraFrameImageCb getCameraFrameImageCb;
+ (void) registerGetCameraFrameImage:(GetCameraFrameImageCb)cb{
    
    getCameraFrameImageCb = cb;
}
+ (void) GetCameraFrameImageWithSuccessCallback:(GetCameraFrameImageSuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr{
    
    getCameraFrameImageCb(ptr, success, error);
}
@end

#if __cplusplus
extern "C" {
#endif
    
    void registerBuildDidimo(BuildDidimoCb cb) {
        
        [DidimoUnityInterface registerBuildDidimo: cb];
    }
    
    void registerSetHairColor(SetHairColorCb cb) {
        
        [DidimoUnityInterface registerSetHairColor: cb];
    }
    
    void registerSetHairstyle(SetHairstyleCb cb) {
        
        [DidimoUnityInterface registerSetHairstyle: cb];
    }
    
    void registerSetEyeColor(SetEyeColorCb cb) {
        
        [DidimoUnityInterface registerSetEyeColor: cb];
    }
    
    void registerPlayExpression(PlayExpressionCb cb) {
        
        [DidimoUnityInterface registerPlayExpression: cb];
    }
    
    void registerDestroyDidimo(DestroyDidimoCb cb) {
        
        [DidimoUnityInterface registerDestroyDidimo: cb];
    }
    
    void registerCacheAnimation(CacheAnimationCb cb) {
        
        [DidimoUnityInterface registerCacheAnimation: cb];
    }
    
    void registerClearAnimationCache(ClearAnimationCacheCb cb) {
        
        [DidimoUnityInterface registerClearAnimationCache: cb];
    }
    
    void registerSetOrbitControls(SetOrbitControlsCb cb) {
        
        [DidimoUnityInterface registerSetOrbitControls: cb];
    }
    
    void registerTextToSpeech(TextToSpeechCb cb) {
        
        [DidimoUnityInterface registerTextToSpeech: cb];
    }
    
    void registerUpdateDeformable(UpdateDeformableCb cb) {
        
        [DidimoUnityInterface registerUpdateDeformable: cb];
    }
 
    void registerGetDeformableData(GetDeformableDataCb cb) {
        
        [DidimoUnityInterface registerGetDeformableData: cb];
    }
    
    void registerSetCamera(SetCameraCb cb) {
        
        [DidimoUnityInterface registerSetCamera: cb];
    }
    
    void registerResetCamera(ResetCameraCb cb) {
        
        [DidimoUnityInterface registerResetCamera: cb];
    }
    
    void registerGetCameraFrameImage(GetCameraFrameImageCb cb) {
        
        [DidimoUnityInterface registerGetCameraFrameImage: cb];
    }
#if __cplusplus
}
#endif
