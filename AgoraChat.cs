using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora_RTC_Plugin;
using Agora.Rtc;
using System;
using System.IO;
using Unity.UI;
using UnityEngine.UI;
using Unity.VisualScripting;

public class AgoraChat : MonoBehaviour
{

    internal string _appID = "";   // Put your appID here which u get from the Agora Console
    internal string _channelName = "";  // Put your channel name 
    internal string _token = ""; //Put Your Token Here which you  get from the Agora console 
    internal uint remoteUid; // ID of the remote user 
    internal IRtcEngine agoraEngine;  // Agora Engine to be used 
    internal VideoSurface LocalView;  // Local View to be displayed 
    internal VideoSurface RemoteView;  // Remote View to be displayed 
    
    internal AREA_CODE region = AREA_CODE.AREA_CODE_GLOB;
    internal string userRole = "";
    public Optional<THREAD_PRIORITY_TYPE> threadPriority = new Optional<THREAD_PRIORITY_TYPE>();

    private void Awake()
    {
        Debug.Log("Awake function has been called");
        setupUI();  // To be used for UI elements
    }


    void Start()  // Start is called before the first frame update
    {
        SetupAgoraEngine();
        InitEventHandler();
    }

    void Update()   // Update is called once per frame
    {

    }

    void setupUI()
    {
        GameObject localViewObject = GameObject.Find("LocalViewObject");
        LocalView = localViewObject.AddComponent<VideoSurface>();
        localViewObject.transform.Rotate(0.0f, 0.0f, 180.0f);

        GameObject remoteViewObject = GameObject.Find("RemoteViewObject");
        RemoteView = remoteViewObject.AddComponent<VideoSurface>();
        remoteViewObject.transform.Rotate(0.0f, 0.0f, 180.0f);

        GameObject leaveButtonObject = GameObject.Find("LeaveButton");
        leaveButtonObject.GetComponent<Button>().onClick.AddListener(Leave);

        GameObject joinButtonObject = GameObject.Find("JoinButton");
        joinButtonObject.GetComponent<Button>().onClick.AddListener(Join);

        Debug.Log("UI elements have been set correctly");
    }


    public void SetupAgoraEngine()
    {
        if (_appID == "")
        {
            Debug.Log("Please set an app ID or token");
            return;
        }
        // Create an instance of the video SDK engine.
        agoraEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

        RtcEngineContext context = new RtcEngineContext(_appID, 0, CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION, "",
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, region, null, threadPriority, false, false, true);

        agoraEngine.Initialize(context);

        // Enable the video module.
        agoraEngine.EnableVideo();

        // Set the user role as broadcaster.
        agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Attach the eventHandler
        // InitEventHandler();

        if (agoraEngine != null)
        {
            Debug.Log("Agora Engine has been set up correctly");
        }

    }


    public void Join()
    {
        Debug.Log("Join Button has been pressed");

        if (agoraEngine != null)
        {
            agoraEngine.EnableVideo();     // Enable the video module

            // agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);   // Set the user role as broadcaster.

            LocalView.SetForUser(0, _channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);   // Set the local video view.

            LocalView.SetEnable(true);     // Start rendering local video.

            agoraEngine.JoinChannel(_token, _channelName, "", 0);   // Join a channel.
        }
        else
        {
            Debug.Log("This Error has occured in Join function");
            Debug.Log("There are some problem with initializing Agora Engine");
            return;
        }

    }


    void Leave()  // Define a public function called Leave() to leave the channel on the LocalUser side
    {
        Debug.Log("Leave Button has been pressed");

        if (agoraEngine != null)
        {
            agoraEngine.LeaveChannel();   // Leave the channel and clean up resources
            agoraEngine.DisableVideo();   // Disable the video modules.
            RemoteView.SetEnable(false);  // Disable the remote video rendering. 
            LocalView.SetEnable(false);   // Disable the local video rendering
            DestroyEngine();
        }
        else
        {
            Debug.Log("This Error has occured in Leave function");
            Debug.Log("There are some problem with initializing Agora Engine");
            return;

        }

    }

    public void DestroyEngine()   // Use this function to destroy the engine
    {
        if (agoraEngine != null)
        {
            // Destroy the engine.
            agoraEngine.LeaveChannel();
            agoraEngine.Dispose();
            agoraEngine = null;
        }
    }



    private void InitEventHandler()
    {
        // Creates a UserEventHandler instance.
        UserEventHandler handler = new UserEventHandler(this);
        agoraEngine.InitEventHandler(handler);
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AgoraChat _videoSample;

        internal UserEventHandler(AgoraChat videoSample)
        {
            _videoSample = videoSample;
        }

        // This callback is triggered when the local user joins the channel.
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("You joined the channel: " + connection.channelId);
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);    // Setup remote view.
            _videoSample.RemoteView.SetEnable(true);

            _videoSample.remoteUid = uid;  // Save the remote user ID in a variable.
            Debug.Log("Remote User has JOINED with the ID: " + uid + " to the channel: " + connection.channelId);

        }

        // This callback is triggered when a remote user leaves the channel or drops offline.
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
            // Debug.Log("Remote User has left the channel: " + connection.channelId);
            Debug.Log("Remote User has LEFT with the ID: " + uid + " from the channel: " + connection.channelId);


        }

    }


    private void OnApplicationQuit()
    {
        DestroyEngine();
    }


}
