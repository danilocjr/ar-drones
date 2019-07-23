using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.UI;

public class StorageManager : MonoBehaviour
{
    [SerializeField]
    string worldmap = "dronewars_session.worldmap";

    [SerializeField]
    string local_url;

    StorageReference reference;
    public Text info;

    bool isFirebaseReady;

    private void Start()
    {
        isFirebaseReady = false;
        reference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://ardronewars.appspot.com/" + worldmap);

#if UNITY_EDITOR
        local_url = Path.Combine(Application.streamingAssetsPath, worldmap);
#else
        local_url = Path.Combine(Application.persistentDataPath, worldmap);
#endif
        StartCoroutine("WaitForFirebase");
    }
    
    IEnumerator WaitForFirebase()
    {
        info.text = "Connecting to the AR CLOUD...";
        yield return new WaitForSeconds(5);
        isFirebaseReady = true;
        yield return null;
        info.text += "...complete!";
    }

    public void OnClickUploadARWorldMap()
    {
        info.text = "> Reading from " + local_url;

        reference.PutFileAsync("file://"+local_url)
          .ContinueWith((Task<StorageMetadata> task) => {
              if (task.IsFaulted || task.IsCanceled)
              {
                  info.text += "\n" + task.Exception.ToString();          
              }
              else
              {
                  info.text += "\n" + "Finished uploading...";
              }
          });
    }

    public void OnClickDownloadARworldMap()
    {
        if (!isFirebaseReady)
            return;

        try
        {
            // Start downloading a file
            Task task = reference.GetFileAsync(local_url, new StorageProgress<DownloadState>((DownloadState state) =>
            {
                info.text = String.Format("Progress: {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount);
            }), CancellationToken.None);

            task.ContinueWith(resultTask => {
                if (!resultTask.IsFaulted && !resultTask.IsCanceled)
                {
                    info.text = "Download finished.";
                }
            });
        }
        catch
        {
            info.text = "Fail to connect to AR Cloud.";
        }
        
    }

}
