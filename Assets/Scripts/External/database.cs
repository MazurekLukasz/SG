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


    public string Query(string q)
    {
        try
        {
            WebRequest request = WebRequest.Create("http://" + IP + "/db/data/transaction/commit");
            request.Method = "POST";
            request.Credentials = new NetworkCredential(Username, Password);            
            StreamWriter requestStream = new StreamWriter(request.GetRequestStream()); 
            requestStream.Write("{\"statements\" : [ { \"statement\" : \"" + q + "\", \"resultDataContents\" : [ \"graph\" ] } ]}");
            requestStream.Flush();
            requestStream.Close();
            WebResponse response = request.GetResponse();  
            Stream stream = response.GetResponseStream();
            var streamReader = new StreamReader(stream);
            var responseJson = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            if (DebugLog)
            {
                Debug.Log("Odpowiedź w formacie Json:" + responseJson);
            }
            return responseJson;
        }
        catch (WebException webEx)
        {
            Debug.LogError("Błąd. Powód: " + webEx.Message);
            return "{}";
        }
    }

    /// <summary>
    /// Create sector point in database
    /// </summary>
    /// <param name="sector">sector to create point in database</param>
    /// 


    public void CreateSpaceSector(GameObject sector)
    {
        int x = (int)sector.transform.position.x;
        int y = (int)sector.transform.position.y;

        string props = "x:" + x + ",y:" + y + " ";
        Query("CREATE(n:SpaceSystem {" + props + "})");
    }


    public void CreateSectorConnection(GameObject sector1, GameObject sector2)
    {
        int x = (int)sector1.transform.position.x;
        int y = (int)sector1.transform.position.y;

        int x1 = (int)sector2.transform.position.x;
        int y1 = (int)sector2.transform.position.y;

        string props = "MATCH(a: SpaceSystem ),(b: SpaceSystem ) WHERE a.x = " + x + " AND a.y = " + y + " AND b.x = " + x1 + " AND b.y = " + y1 + " CREATE(a) -[c:SSConnection]->(b)";
        Query(props);
    }


    public void ClearAll()
    {
        string q = " MATCH(n) DETACH DELETE n ";
        Query(q);
    }

    /// <summary>
    /// Test function
    /// </summary>
    /// <param name="q"></param>
    public void sendRequest(string q)
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
        }
        catch (WebException webex)
        {
            Debug.LogError("Failed. Reason: " + webex.Message);
        }
    }

}
