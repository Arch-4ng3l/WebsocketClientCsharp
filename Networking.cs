using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class Networking : MonoBehaviour
{
    public float renderDistance;
    private JsonSettings settings;
    private Client client;
    void Start()
    {
        Uri uri = new("");
        client = new Client(uri);
        settings.renderdistance = renderDistance;

        Task<int> task = client.ConnectAsync();
        task.RunSynchronously();
        if (task.Result == 0)
        {
            return;
        }
        Task task2 = client.GetAssets("./", "./assets");
        task2.RunSynchronously();
        Task task3 = client.SendJson<JsonSettings>(settings);
        task3.RunSynchronously();
        

    }

    // Update is called once per frame
    void Update()
    {

    }

    async void SendObject(GameObject send)
    {
        JsonObject json = new JsonObject();
        json.FromGameObject(send);

        await client.SendJson<JsonObject>(json);
        if (json.name != "Player")
        {
            return;
        }
        await RecvObjects();



    }

    async Task RecvObjects()
    {
        List<JsonObject> objs = await client.ReceiveJson<JsonObject>();
        foreach (JsonObject json in objs)
        {
            json.ToGameObject();
        }
    }
}


public class JsonObject
{
    public float x;
    public float y;
    public float z;
    public string name;

    public JsonObject() : base()
    {
        this.name = "";
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }
    public void FromGameObject(GameObject go)
    {
        this.name = go.name;
        this.x = go.transform.position.x;
        this.y = go.transform.position.y;
        this.z = go.transform.position.z;
    }
    public void ToGameObject()
    {

        GameObject obj = GameObject.Find(name);
        Vector3 position = new Vector3(x, y, z);

        if (obj == null)
        {
            string pattern = @"\d+$";
            string objName = Regex.Replace(name, pattern, "");
            obj = GameObject.Find(objName);
            obj = UnityEngine.Object.Instantiate(obj, position, Quaternion.identity);
            obj.name = name;
            return;
        }

        if (obj.transform.position != position)
        {
            obj.transform.position = position;
        }

        return;
    }
}

class JsonSettings
{
    public float renderdistance;

}
