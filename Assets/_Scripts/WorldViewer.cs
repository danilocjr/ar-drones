using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.XR.ARKit;
using Firebase.Storage;
using System.Threading.Tasks;
using System.Threading;
using System;

public class WorldViewer : MonoBehaviour
{
    [Tooltip("The ARSession component controlling the session from which to generate ARWorldMaps.")]
    [SerializeField]
    ARSession m_ARSession;

    public GameObject canvasOverlay;

    public ARSession arSession
    {
        get { return m_ARSession; }
        set { m_ARSession = value; }
    }

    string worldmap = "dronewars_session.worldmap";
    string local_url;
    StorageReference reference;
    bool isFirebaseReady;

    string path
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "dronewars_session.worldmap");
        }
    }

    [Tooltip("The UI Text element used to display log messages.")]
    [SerializeField]
    Text m_LogText;


    private void Awake()
    {
        canvasOverlay.SetActive(true);
    }

    void Start()
    {
        isFirebaseReady = false;
        reference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://ardronewars.appspot.com/" + worldmap);
        local_url = Path.Combine(Application.persistentDataPath, worldmap);
        StartCoroutine("WaitForFirebase");
    }

    IEnumerator WaitForFirebase()
    {
        Log("Connecting to the AR CLOUD...");
        yield return new WaitForSeconds(4);
        DownloadFile();
    }

    public void DownloadFile()
    {
        try
        {
            // Start downloading a file
            Task task = reference.GetFileAsync("file://" + local_url, new StorageProgress<DownloadState>((DownloadState state) =>
            {
                Log(String.Format("Progress: {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount));
            }), CancellationToken.None);

            task.ContinueWith(resultTask => {
                if (!resultTask.IsFaulted && !resultTask.IsCanceled)
                {
                    Log("Download finished.");
                    StartCoroutine("Load");
                }
            });
        }
        catch
        {
            Log("Fail to connect to AR Cloud.");
        }

    }

    IEnumerator Load()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            Log("No session subsystem available. Could not load.");
            yield break;
        }

        var file = File.Open(path, FileMode.Open);
        if (file == null)
        {
            Log(string.Format("File {0} does not exist.", path));
            yield break;
        }

        Log(string.Format("Reading {0}...", path));

        int bytesPerFrame = 1024 * 10;
        var bytesRemaining = file.Length;
        var binaryReader = new BinaryReader(file);
        var allBytes = new List<byte>();
        while (bytesRemaining > 0)
        {
            var bytes = binaryReader.ReadBytes(bytesPerFrame);
            allBytes.AddRange(bytes);
            bytesRemaining -= bytesPerFrame;
            yield return null;
        }

        var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
        data.CopyFrom(allBytes.ToArray());

        Log(string.Format("Deserializing to ARWorldMap...", path));
        ARWorldMap worldMap;
        if (ARWorldMap.TryDeserialize(data, out worldMap))
            data.Dispose();

        if (worldMap.valid)
        {
            Log("Deserialized successfully.");
            canvasOverlay.SetActive(false);
        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");
            yield break;
        }

        Log("Apply ARWorldMap to current session.");
        sessionSubsystem.ApplyWorldMap(worldMap);
    }

    void Log(string logMessage)
    {
        m_LogText.text = logMessage;
        //m_LogMessages.Add(logMessage);
    }
}
