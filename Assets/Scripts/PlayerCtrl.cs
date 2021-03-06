﻿using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;


public class PlayerCtrl : MonoBehaviour
{        
    // Leaving for now. TODO: remove this at some point?
    public bool noteCtrlOn = true;

    public int minMidiNote = 36;                                // Note Range for Theremini (Configurable in Theremini settings)
    public int maxMidiNote = 72;

    public int pitchMidiChannel = 20;
    public int volMidiChannel = 2;

    public float tempo = 45;

    public int startDelay;                                      // How long to wait before starting system

    // VR settings and Training Scenes
    public string VRDeviceName = "WindowsMR";

    public GameObject completedMenuPrefab;                      // Menu Prefab to instantiate when a score is completed

    // Start Menu Input Items
    public string MidiScoreResource { get; set; }               // Name of file containing Midi data

    private string midiInputDeviceName;                          // Name of Midi Device to connect to
    public string MidiInputDeviceName                            // Connect to Midi Device as soon as value set (close previous device if there is one)
    {
        get { return midiInputDeviceName; }
        set
        {
            midiInputDeviceName = value;
            MidiIn.StopAndClose(false);
            MidiIn.Connect(midiInputDeviceName);
            MidiIn.Start();
        }
    }

    private GameObject thereminLH;
    private GameObject thereminRH;

    // VRMin Components
    public static PlayerCtrl Control { get; private set; }      // Singleton Accessor
    public LogWriter Logger { get; private set; }               // Logger
    public MidiInputCtrl MidiIn { get; private set; }           // Main Midi Input Controller used to position left and right hands

    private GameObject completedMenu;                           // The instantiated completed menu game object

    void Awake ()
    {
        //Implement Psuedo-Singleton
        if (Control == null)
        {
            DontDestroyOnLoad(gameObject);
            Control = this;
        }
        else if (Control != this)
        {
            Destroy(gameObject);
        }

        //Initialize Midi In (so objects can subscribe to it upon load)
        MidiIn = new MidiInputCtrl();

        //Initialize Logger
        Logger = new LogWriter();
    }

    private void Start()
    {
        // initializes and sets the correct thermin interface for the handedness
        
    }

    public void StartVRMin()
    {
        StartSession();
    }



    private void StartSession()
    {
        Logger.Start(string.Format("Hyperemin-session-score{0}", Path.GetFileNameWithoutExtension(MidiScoreResource)));      // Start Up the Logger
        StartCoroutine(DelayedStart(startDelay));   // Start Notes on a Delay
    }


    private void ResetCameras()
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
            {
                cam.transform.localPosition = Vector3.zero;
                cam.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private IEnumerator DelayedStart(int delay)
    {
        NoteCtrl.Control.MidiScoreFile = MidiScoreResource;     // Set Score before delay to trigger start message
        yield return new WaitForSecondsRealtime(delay);   // add an extra 5 seconds for VR scene to load

        Logger.Log("StartSession\t{0}", Path.GetFileNameWithoutExtension(MidiScoreResource));
        // Start Playing notes
        NoteCtrl.Control.PlayMidi(NoteCtrl.MidiStatus.Play);
    }

    public void MidiComplete()
    {   
        //instatiate completed menu and set player controller to this
        completedMenu = Instantiate(completedMenuPrefab);
        Logger.Stop();
    }

    void OnDisable()
    {
        // Clean up when controller is destroyed
        if(MidiIn != null)
            MidiIn.StopAndClose();
        if (Logger != null)
            Logger.Stop();
    }
}
