using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// UnityWebRequest.Get example

// Access a website and use UnityWebRequest.Get to download a page.
// Also try to download a non-existing page. Display the error.

public class Example : MonoBehaviour
{
    [SerializeField] private string uri = "http://localhost:7474/db/data/node";
   
    [SerializeField] private string uriGetRequest = "http://localhost:7474/db/data/transaction/commit";

    [SerializeField] private string log = "neo4j";
    [SerializeField] private string pass = "1";

    void Start()
    {
        //string props = " Name:\"Kaczka\"";
        //string q = "CREATE(n:Planet {" + props + "}) RETURN n";
        string q = "match (n) RETURN n";
        StartCoroutine(Upload(q));
        StartCoroutine(GetRequest(uriGetRequest));
    }


    string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }


    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

            string authorization = authenticate(log, pass);
            webRequest.SetRequestHeader("AUTHORIZATION", authorization);
      
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
               // Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.data);
            }
        }
    }

    IEnumerator Upload(string query)
    {
        
        WWWForm form = new WWWForm();
        form.AddField("query", query);

        using (UnityWebRequest www = UnityWebRequest.Post(uri, form))
        {
            string authorization = authenticate(log, pass);
            www.SetRequestHeader("AUTHORIZATION", authorization);

            yield return www.SendWebRequest();
           
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                Debug.Log(www.downloadHandler.data);
            }
        }
    }


}
