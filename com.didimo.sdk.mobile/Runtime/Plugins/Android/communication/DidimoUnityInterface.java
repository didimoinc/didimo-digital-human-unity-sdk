package com.unity3d.communication;

public class DidimoUnityInterface {

    // Default response interface with success and error message callback.
    // Methods invoked with this are not necessarily asynchronous
    public interface DefaultResponseInterface {
        // Receive from Unity
        void onSuccess();

        // Receive from Unity
        void onError(String message);
    }

    // BUILD DIDIMO FROM DIRECTORY
    public interface BuildDidimoFromDirectoryInterface {
        // Send to Unity
        void sendToUnity(String didimoPath, String didimoKey, DefaultResponseInterface response);
    }

    private BuildDidimoFromDirectoryInterface buildDidimoFromDirectoryInterface;

    private void RegisterForCommunication(BuildDidimoFromDirectoryInterface buildDidimoFromDirectoryInterface) {
        this.buildDidimoFromDirectoryInterface = buildDidimoFromDirectoryInterface;
    }

    public void BuildDidimoFromDirectory(String didimoPath, String didimoKey, DefaultResponseInterface responseInterface) {

        buildDidimoFromDirectoryInterface.sendToUnity(didimoPath, didimoKey, responseInterface);
    }

    // CACHE ANIMATION
    public interface CacheAnimationInterface {
        // Send to Unity
        void sendToUnity(String id, String filePath, DefaultResponseInterface response);
    }

    private CacheAnimationInterface cacheAnimationInterface;

    private void RegisterForCommunication(CacheAnimationInterface cacheAnimationInterface) {
        this.cacheAnimationInterface = cacheAnimationInterface;
    }

    public void CacheAnimation(String id, String filePath, DefaultResponseInterface responseInterface) {

        cacheAnimationInterface.sendToUnity(id, filePath, responseInterface);
    }

    // CLEAR ANIMATION CACHE
    public interface ClearAnimationCacheInterface {
        // Send to Unity
        void sendToUnity(DefaultResponseInterface response);
    }

    private ClearAnimationCacheInterface clearAnimationCacheInterface;

    private void RegisterForCommunication(ClearAnimationCacheInterface clearAnimationCacheInterface) {
        this.clearAnimationCacheInterface = clearAnimationCacheInterface;
    }

    public void ClearAnimationCache(DefaultResponseInterface responseInterface) {

        clearAnimationCacheInterface.sendToUnity(responseInterface);
    }

    // DESTROY DIDIMO
    public interface DestroyDidimoInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, DefaultResponseInterface response);
    }

    private DestroyDidimoInterface destroyDidimoInterface;

    private void RegisterForCommunication(DestroyDidimoInterface destroyDidimoInterface) {
        this.destroyDidimoInterface = destroyDidimoInterface;
    }

    public void DestroyDidimo(String didimoKey, DefaultResponseInterface responseInterface) {

        destroyDidimoInterface.sendToUnity(didimoKey, responseInterface);
    }

    // PLAY EXPRESSION
    public interface PlayExpressionInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, String animationId, DefaultResponseInterface response);
    }

    private PlayExpressionInterface playExpressionInterface;

    private void RegisterForCommunication(PlayExpressionInterface playExpressionInterface) {
        this.playExpressionInterface = playExpressionInterface;
    }

    public void PlayExpression(String didimoKey, String animationId, DefaultResponseInterface responseInterface) {

        playExpressionInterface.sendToUnity(didimoKey, animationId, responseInterface);
    }

    // RESET CAMERA
    public interface ResetCameraInterface {
        // Send to Unity
        void sendToUnity(boolean instant, DefaultResponseInterface response);
    }

    private ResetCameraInterface resetCameraInterface;

    private void RegisterForCommunication(ResetCameraInterface resetCameraInterface) {
        this.resetCameraInterface = resetCameraInterface;
    }

    public void ResetCamera(boolean instant, DefaultResponseInterface responseInterface) {

        resetCameraInterface.sendToUnity(instant, responseInterface);
    }

    // SET CAMERA
    public interface SetCameraInterface {
        // Send to Unity
        void sendToUnity(float[] positionVec3, float[] rotationQuaternion, float fieldOfView, DefaultResponseInterface response);
    }

    private SetCameraInterface setCameraInterface;

    private void RegisterForCommunication(SetCameraInterface setCameraInterface) {
        this.setCameraInterface = setCameraInterface;
    }

    public void SetCamera(float[] positionVec3, float[] rotationQuaternion, float fieldOfView, DefaultResponseInterface responseInterface) {

        setCameraInterface.sendToUnity(positionVec3, rotationQuaternion, fieldOfView, responseInterface);
    }

    // SET EYE COLOR
    public interface SetEyeColorInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, int colorIdx, DefaultResponseInterface response);
    }

    private SetEyeColorInterface setEyeColorInterface;

    private void RegisterForCommunication(SetEyeColorInterface setEyeColorInterface) {
        this.setEyeColorInterface = setEyeColorInterface;
    }

    public void SetEyeColor(String didimoKey, int colorIdx, DefaultResponseInterface responseInterface) {

        setEyeColorInterface.sendToUnity(didimoKey, colorIdx, responseInterface);
    }

    // SET HAIR COLOR
    public interface SetHairColorInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, int colorIdx, DefaultResponseInterface response);
    }

    private SetHairColorInterface setHairColorInterface;

    private void RegisterForCommunication(SetHairColorInterface setHairColorInterface) {
        this.setHairColorInterface = setHairColorInterface;
    }

    public void SetHairColor(String didimoKey, int colorIdx, DefaultResponseInterface responseInterface) {

        setHairColorInterface.sendToUnity(didimoKey, colorIdx, responseInterface);
    }

    // SET HAIRSTYLE
    public interface SetHairstyleInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, String styleId, DefaultResponseInterface response);
    }

    private SetHairstyleInterface setHairstyleInterface;

    private void RegisterForCommunication(SetHairstyleInterface setHairstyleInterface) {
        this.setHairstyleInterface = setHairstyleInterface;
    }

    public void SetHairstyle(String didimoKey, String styleId, DefaultResponseInterface responseInterface) {
        setHairstyleInterface.sendToUnity(didimoKey, styleId, responseInterface);
    }

    // SET ORBIT CONTROLS
    public interface SetOrbitControlsInterface {
        // Send to Unity
        void sendToUnity(boolean enabled, DefaultResponseInterface response);
    }

    private SetOrbitControlsInterface setOrbitControlsInterface;

    private void RegisterForCommunication(SetOrbitControlsInterface setOrbitControlsInterface) {
        this.setOrbitControlsInterface = setOrbitControlsInterface;
    }

    public void SetOrbitControls(boolean enabled, DefaultResponseInterface responseInterface) {

        setOrbitControlsInterface.sendToUnity(enabled, responseInterface);
    }

    // TEXT TO SPEECH
    public interface TextToSpeechInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, String dataPath, String clipPath, DefaultResponseInterface response);
    }

    private TextToSpeechInterface textToSpeechInterface;

    private void RegisterForCommunication(TextToSpeechInterface textToSpeechInterface) {
        this.textToSpeechInterface = textToSpeechInterface;
    }

    public void TextToSpeech(String didimoKey, String dataPath, String clipPath, DefaultResponseInterface responseInterface) {

        textToSpeechInterface.sendToUnity(didimoKey, dataPath, clipPath, responseInterface);
    }

    // GET DEFORMABLE DATA
    public interface GetDeformableDataResponseInterface {
        // Receive from Unity
        void onSuccess(byte[] bytes);

        void onError(String message);
    }

    public interface GetDeformableDataInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, String deformableId, GetDeformableDataResponseInterface response);
    }

    private GetDeformableDataInterface getDeformableDataInterface;

    private void RegisterForCommunication(GetDeformableDataInterface getDeformableDataInterface) {
        this.getDeformableDataInterface = getDeformableDataInterface;
    }

    public void GetDeformableData(String didimoKey, String deformableId, GetDeformableDataResponseInterface responseInterface) {

        getDeformableDataInterface.sendToUnity(didimoKey, deformableId, responseInterface);
    }

    // UPDATE DEFORMABLE
    public interface UpdateDeformableInterface {
        // Send to Unity
        void sendToUnity(String didimoKey, String deformableId, byte[] deformedData, DefaultResponseInterface response);
    }

    private UpdateDeformableInterface updateDeformableInterface;

    private void RegisterForCommunication(UpdateDeformableInterface updateDeformableInterface) {
        this.updateDeformableInterface = updateDeformableInterface;
    }

    public void UpdateDeformable(String didimoKey, String deformableId, byte[] deformedData, DefaultResponseInterface responseInterface) {

        updateDeformableInterface.sendToUnity(didimoKey, deformableId, deformedData, responseInterface);
    }

    // GET CAMERA FRAME IMAGE
    public interface GetCameraFrameImageResponseInterface {
        // Receive from Unity
        void onSuccess(byte[] pngImage);

        void onError(String message);
    }

    public interface GetCameraFrameImageInterface {
        // Send to Unity
        void sendToUnity(Boolean withDidimoWatermark, GetCameraFrameImageResponseInterface response);
    }

    private GetCameraFrameImageInterface getCameraFrameImageInterface;

    private void RegisterForCommunication(GetCameraFrameImageInterface getCameraFrameImageInterface) {
        this.getCameraFrameImageInterface = getCameraFrameImageInterface;
    }

    public void GetCameraFrameImage(Boolean withDidimoWatermark, GetCameraFrameImageResponseInterface responseInterface) {

        getCameraFrameImageInterface.sendToUnity(withDidimoWatermark, responseInterface);
    }
}
