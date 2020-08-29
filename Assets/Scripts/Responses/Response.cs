using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Response
{
    public List<Result> results;
    public List<object> errors;

    public static Response CreateFromJson(string json)
    {
        return JsonUtility.FromJson<Response>(json);
    }
}

[System.Serializable]
public class Result
{
    public List<string> columns;
    public List<Datum> data;
}

[System.Serializable]
public class Datum
{
    public Graph graph;

}

[System.Serializable]
public class Graph
{
    public List<Node> nodes;
    public List<object> relationships;

}

[System.Serializable]
public class Node
{
    public string id;
    public List<string> labels;
    public Properties properties;

}

[System.Serializable]
public class Properties
{
    public int x;
    public int y;

}
