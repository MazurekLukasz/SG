using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


using System;
using System.IO;
using System.Net;
using System.Text;

public class database: MonoBehaviour
{
    public string Username = "neo4j";
    public string Password = "1";
    public string IP = "localhost:7474";
    public bool DebugLog = true;


    // ------// MATCH (n) DETACH DELETE n  -------- delete ALL
    // string props = " id:1,x:1,y:1, ";
    // "CREATE(n:Planet {" + props + "})" 
    public void Create(int x,int y)
    {
        string props = "x:"+x+ ",y:" + y + " ";
        Query("CREATE(n:Planet {" + props + "})");
    }

    public void sendRequest(string q)
    {
        // build request
        WebRequest request = WebRequest.Create("http://" + IP + "/db/data/transaction/commit");
        request.Method = "POST";
        request.Credentials = new NetworkCredential(Username, Password);

        // grab request stream so we can send JSON
        StreamWriter requestStream = new StreamWriter(request.GetRequestStream());
        //requestStream.Write("{\"statements\" : [ { \"statement\" : \"" + q + " LIMIT " + Limit + "\", \"resultDataContents\" : [ \"graph\" ] } ]}");
        requestStream.Write("{\"statements\" : [ { \"statement\" : \"" + q + "\", \"resultDataContents\" : [ \"graph\" ] } ]}");
        // close up the IO
        requestStream.Flush();
        requestStream.Close();
    }

    public string Query(string q)
    {
        try
        {
            // build request
            WebRequest request = WebRequest.Create("http://" + IP + "/db/data/transaction/commit");
            request.Method = "POST";
            request.Credentials = new NetworkCredential(Username, Password);

            // grab request stream so we can send JSON
            StreamWriter requestStream = new StreamWriter(request.GetRequestStream());
            //requestStream.Write("{\"statements\" : [ { \"statement\" : \"" + q + " LIMIT " + Limit + "\", \"resultDataContents\" : [ \"graph\" ] } ]}");
            requestStream.Write("{\"statements\" : [ { \"statement\" : \"" + q + "\", \"resultDataContents\" : [ \"graph\" ] } ]}");
            // close up the IO
            requestStream.Flush();
            requestStream.Close();
            //get response
            WebResponse response = request.GetResponse();

            // convert the raw stream into a string readable one
            Stream stream = response.GetResponseStream();
            var streamReader = new StreamReader(stream);
            var responseJson = streamReader.ReadToEnd();

            //close both IO
            streamReader.Close();
            stream.Close();

            if (DebugLog)
            {
                Debug.Log("Request Json:" + responseJson);
            }
            //return JsonUtility.FromJson<Root>(responseJson);
            return responseJson;
        }
        catch (WebException webex)
        {
            Debug.LogError("Failed. Reason: " + webex.Message);
            return "{}";
        }

 
    }

    public void ClearAll()
    {
        string q = " MATCH(n) DETACH DELETE n ";
        Query(q);
    }

}
