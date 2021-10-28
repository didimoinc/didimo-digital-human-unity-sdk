//
//  DidimoUnityInterface.m
//  UnityFramework
//
//  Created by Hugo Pereira on 05/04/2021.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

__attribute__ ((visibility("default")))
@interface DidimoUnityInterface : NSObject{}

typedef void (*ErrorCb)(const void* someInstance, const char* errorMsg);
typedef void (*SuccessCb)(const void* obj);
typedef void (*GetCameraFrameImageSuccessCb)(const void* obj, const void* pngImageData, int dataSize);
typedef void (*GetDeformableDataSuccessCb)(const void* someInstance, const void* bytes, int dataSize);

// call these any time after UnityFrameworkLoad
+ (void) BuildDidimoFromDirectory:(NSString*)didimoDirectory didimoKey:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) SetHairColor:(int)colorIdx forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) SetHairstyleId:(NSString*)styleId forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) SetEyeColor:(int)colorIdx forDidimo:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) PlayExpressionWithdidimoKey:(NSString*)didimoKey animationId:(NSString*)animationId successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) DestroyDidimoWithId:(NSString*)didimoKey successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) CacheAnimationWithId:(NSString*)animationId filePath:(NSString*)filePath successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) ClearAnimationCacheWithSuccessCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) SetOrbitControlsEnabled:(bool)enabled successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) TextToSpeechForDidimo:(NSString*)didimoKey dataPath:(NSString*)dataPath clipPath:(NSString*)clipPath successCallback:(SuccessCb) success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) GetDeformableDataForDidimo:(NSString*)didimoKey deformableId:(NSString*)deformableId successCallback:(GetDeformableDataSuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) UpdateDeformableForDidimo:(NSString*)didimoKey deformableId:(NSString*)deformableId deformedData:(void*)deformedData dataSize:(int)dataSize successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) SetCameraPosition:(float[_Nonnull 3])position rotation:(float[_Nonnull 4])rotation successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) ResetCameraInstant:(bool)instant successCallback:(SuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

+ (void) GetCameraFrameImageWithSuccessCallback:(GetCameraFrameImageSuccessCb)success errorCallback:(ErrorCb)error objectPointer:(const void*)ptr;

@end

NS_ASSUME_NONNULL_END
