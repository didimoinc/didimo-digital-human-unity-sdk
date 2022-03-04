/************** GENERATED AUTOMATICALLY **************/


#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

__attribute__ ((visibility("default")))
@interface DidimoUnityInterface : NSObject{}

typedef void (*SuccessCallback)(const void* objectPointer);
typedef void (*ErrorCallback)(const void* objectPointer, const char* msg);
typedef void (*GetCameraFrameImageSuccessCallback)(const void* pngImageData, int dataSize, const void* objectPointer);
typedef void (*GetDeformableDataSuccessCallback)(const void* obj, const void* data, int dataSize);
typedef void (*ProgressCallback)(const void* objectPointer, float progress);


+ (void) BuildDidimoFromDirectory:(NSString*)didimoDirectory didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) CacheAnimation:(NSString*)animationID filePath:(NSString*)filePath successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) ClearAnimationCache:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) DestroyDidimo:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) GetCameraFrameImage:(GetCameraFrameImageSuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) GetDeformableData:(NSString*)didimoKey deformableId:(NSString*)deformableId successCallback:(GetDeformableDataSuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) PlayCinematic:(NSString*)cinematicID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) PlayExpression:(NSString*)animationID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) ResetCamera:(Boolean)instant successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) ScrollToDidimo:(int)didimoIndex progressCallback:(ProgressCallback)progressCallback successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetEyeColor:(int)eyeColorID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetHairColor:(int)colorPresetId didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetHairstyle:(NSString*)styleID didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetOrbitControlsEnabled:(Boolean)enabled successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetRenderingActive:(Boolean)active successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetViewRect:(int)x y:(int)y width:(int)width height:(int)height successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) SetupARKit:(NSString*)blendshapeNames didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) StopARKit:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) StopDidimoScrolling:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) StreamARKit:(const void*)blendshapeWeights blendshapeCount:(int)blendshapeCount didimoKey:(NSString*)didimoKey successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) TextToSpeech:(NSString*)didimoKey dataPath:(NSString*)dataPath clipPath:(NSString*)clipPath successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


+ (void) UpdateDeformable:(NSString*)didimoKey deformableId:(NSString*)deformableId deformedData:(const void*)deformedData dataSize:(int)dataSize successCallback:(SuccessCallback)successCallback errorCallback:(ErrorCallback)errorCallback objectPointer:(const void*)objectPointer;


@end

NS_ASSUME_NONNULL_END
