using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Evergrove.UnityClient
{
    [Serializable]
    public sealed class RemoteWorldState
    {
        public int schemaVersion;
        public int revision;
        public int treeStage;
        public long lastWatered;
    }

    [Serializable]
    public sealed class RemoteWorldResponse
    {
        public string status;
        public string error;
        public RemoteWorldState world;
    }

    [Serializable]
    internal sealed class WaterTreeAction
    {
        public string type = "WATER_TREE";
        public string operationId;
        public int expectedRevision;
    }

    /// <summary>
    /// Optional Cloudflare Workers reference client. It is NEVER enabled or
    /// attached by Evergrove's default offline game bootstrap.
    ///
    /// CAUTION: This API controls only the demo treeStage. It does NOT synchronize
    /// local puzzle state, NPC memory, or village decision history.
    /// </summary>
    public sealed class WorldClient : MonoBehaviour
    {
        [SerializeField] private string workerBaseUrl = "";
        private const string CapabilityKey = "evergrove:demo:capability:v1";
        private string capability;
        private WaterTreeAction pendingAction;
        private RemoteWorldState remote;
        private bool inFlight;
        public event Action<RemoteWorldState> OnWorldChanged;

        private void Awake()
        {
            // Bearer capabilities held in PlayerPrefs are a DEMO ONLY.
            // Use user-bound auth + secure keystore for any production game.
            capability = PlayerPrefs.GetString(CapabilityKey, "");
            if (capability.Length == 0)
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    var bytes = new byte[32];
                    rng.GetBytes(bytes);
                    capability = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
                }
                PlayerPrefs.SetString(CapabilityKey, capability);
                PlayerPrefs.Save();
            }
        }

        public void SetPreviewWorkerUrl(string httpsBaseUrl)
        {
            workerBaseUrl = httpsBaseUrl == null ? "" : httpsBaseUrl.TrimEnd('/');
        }

        public void FetchWorld()
        {
            if (!CanSend()) return;
            StartCoroutine(RequestWorld());
        }

        public void WaterTree()
        {
            if (!CanSend()) return;
            // Keep the same operation ID after a transport failure. Creating a
            // fresh ID on each retry could apply the same player tap twice.
            if (pendingAction == null)
            {
                if (remote == null)
                {
                    Debug.LogWarning("Fetch the remote world first, then water the demo tree.");
                    return;
                }
                pendingAction = new WaterTreeAction {
                    operationId = Guid.NewGuid().ToString("D"),
                    expectedRevision = remote.revision
                };
            }
            StartCoroutine(PostWater());
        }

        private bool CanSend()
        {
            if (inFlight || string.IsNullOrEmpty(workerBaseUrl) || !workerBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("Evergrove remote preview requires a configured HTTPS Worker and no other request in flight.");
                return false;
            }
            return true;
        }

        private string ApiUrl { get { return workerBaseUrl + "/api/world"; } }

        private void Authorize(UnityWebRequest request)
        {
            request.SetRequestHeader("Authorization", "Bearer " + capability);
            request.timeout = 12;
        }

        private IEnumerator RequestWorld()
        {
            inFlight = true;
            using (UnityWebRequest request = UnityWebRequest.Get(ApiUrl))
            {
                Authorize(request);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    HandleReply(request.downloadHandler.text);
                else
                    Debug.LogWarning("Evergrove remote fetch failed: HTTP " + request.responseCode);
            }
            inFlight = false;
        }

        private IEnumerator PostWater()
        {
            inFlight = true;
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(pendingAction));
            using (var request = new UnityWebRequest(ApiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                Authorize(request);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    HandleReply(request.downloadHandler.text);
                    pendingAction = null;
                }
                else if (request.responseCode == 409 || request.responseCode == 422)
                {
                    HandleReply(request.downloadHandler.text);
                    // Conflict or fully-grown state: don't replay the stale request.
                    pendingAction = null;
                    Debug.LogWarning("Evergrove remote request rejected with HTTP " + request.responseCode);
                }
                else
                    Debug.LogWarning("Evergrove remote request failed; pending operation retained for retry.");
            }
            inFlight = false;
        }

        private void HandleReply(string json)
        {
            try
            {
                var result = JsonUtility.FromJson<RemoteWorldResponse>(json);
                if (result == null || result.world == null) throw new FormatException("World snapshot missing");
                remote = result.world;
                OnWorldChanged?.Invoke(remote);
                Debug.Log("Evergrove remote preview tree stage: " + remote.treeStage);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Invalid Evergrove remote reply: " + exception.GetType().Name);
            }
        }
    }
}
