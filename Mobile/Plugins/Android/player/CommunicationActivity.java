package com.unity3d.player;

import android.os.Bundle;
import com.unity3d.communication.DidimoUnityInterface;


public class CommunicationActivity extends UnityPlayerActivity {

    DidimoUnityInterface didimoUnityInterface = null;
    static CommunicationActivity _lastInstance;

    public static CommunicationActivity LastInstance() {

        return _lastInstance;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        _lastInstance = this;
    }

    public DidimoUnityInterface getDidimoUnityInterface() {

        if (didimoUnityInterface == null) {
            didimoUnityInterface = new DidimoUnityInterface();
        }
        return didimoUnityInterface;
    }
}

