using System;
using System.Runtime.InteropServices;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class MusicManager : MonoBehaviour
{
    EventInstance musicEvent;
    FMOD.Studio.EVENT_CALLBACK markerCallback;
    private float timingWindow = 0.3f;
    private float timer = 0f; // Timer for checking input
    private bool isTiming = false;
    private float markerTime;

    [StructLayout(LayoutKind.Sequential)]
    private struct TimelineMarker
    {
        public IntPtr name; // Pointer to the marker name (FMOD passes this as IntPtr)
        public int position; // Position of the marker in the timeline (in milliseconds)
    }

    void Start()
    {
        musicEvent = FMODUnity.RuntimeManager.CreateInstance("event:/Tutorial");
        markerCallback = new EVENT_CALLBACK(OnMarkerReached);
        musicEvent.setCallback(markerCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        musicEvent.start();
    }

    private static FMOD.RESULT OnMarkerReached(EVENT_CALLBACK_TYPE type, IntPtr eventInstance, IntPtr parameter)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            MusicManager instance = (MusicManager)FindObjectOfType(typeof(MusicManager));
            
            TimelineMarker marker = (TimelineMarker)Marshal.PtrToStructure(parameter, typeof(TimelineMarker));
            string markerName = Marshal.PtrToStringAnsi(marker.name);

            instance.markerTime = marker.position / 1000f;

            
            instance.timer = instance.timingWindow;
            instance.isTiming = true;


            //UnityEngine.Debug.Log($"Marker Reached: {markerName}, Position: {marker.position} ms");
        }
        return FMOD.RESULT.OK;
    }

    void Update()
    {
        // Update the timer if we're within the timing window
        if (isTiming)
        {
            timer -= Time.deltaTime; // Decrease timer by the time passed since the last frame

            // Check for key press within the timing window
            if (Input.GetKeyDown(KeyCode.Space)) // Example with space key
            {
                // Check if the press is within the timing window from the marker
                if (Math.Abs(markerTime - GetCurrentPlaybackTime()) <= timingWindow)
                {
                    UnityEngine.Debug.Log("Hit at the right time!");
                }
                else
                {
                    UnityEngine.Debug.Log("Early hit!");
                }
                isTiming = false; // Reset timing flag
            }

            // Reset the timing if the timer runs out
            if (timer <= 0)
            {
                UnityEngine.Debug.Log("Missed the beat!");
                isTiming = false; // Reset timing flag
            }
        }
    }

    private float GetCurrentPlaybackTime()
    {
        // Get the current playback time of the music event
        musicEvent.getTimelinePosition(out int position);
        return position / 1000f; // Convert milliseconds to seconds
    }

    void OnDestroy()
    {
        if (musicEvent.isValid())
        {
            musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicEvent.release();
        }
    }
}