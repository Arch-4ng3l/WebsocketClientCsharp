using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;



public class Client
{

    public int receiveBufferSize = 1024;
    private ClientWebSocket _Socket;
    private Uri _Uri;
    public Client(Uri uri)
    {
        _Socket = new ClientWebSocket();
        SetOrigin();
        _Uri = uri;

    }

    private void SetOrigin()
    {
        _Socket.Options.SetRequestHeader("Origin", "http://t.t");
    }


    public async Task<int> ConnectAsync(int count = 0, CancellationToken cancellationToken = default)
    {

        Thread.Sleep(count * 500);
        try
        {
            await _Socket.ConnectAsync(_Uri, cancellationToken);
            return 0;
        }
        catch (Exception e) when (e is WebSocketException)
        {
            _Socket = new ClientWebSocket();
            SetOrigin();
            count++;
            if (count < 15)
            {
                return await ConnectAsync(count);
            }
            else
            {
                return 1;
            }

        }
        catch (Exception)
        {
            return 1;
        }
    }

    public async Task SendJson<T>(T send, CancellationToken cancellationToken = default)
    {

        string jsonMessage = JsonUtility.ToJson(send);
        byte[] byteMessage = Encoding.UTF8.GetBytes(jsonMessage);
        await _Socket.SendAsync(new ArraySegment<byte>(byteMessage), WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task<List<T>> ReceiveJson<T>(CancellationToken cancellationToken = default)
    {
        byte[] receiveBuffer = new byte[receiveBufferSize];
        int bytesReceived = 0;
        WebSocketReceiveResult result;

        while (true)
        {
            result = await _Socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken);
            bytesReceived += result.Count;
            if (result.EndOfMessage)
            {
                break;
            }

            if (bytesReceived >= receiveBufferSize)
            {
                Array.Resize(ref receiveBuffer, receiveBufferSize * 2);
                receiveBufferSize *= 2;
            }
        }

        string messageReceived = Encoding.UTF8.GetString(receiveBuffer);
        string finalString = messageReceived.Replace('\x00', ' ').Trim();

        List<T> list = JsonUtility.FromJson<List<T>>(finalString)!;
        return list;
    }


    public async Task GetAssets(string path, string targetPath)
    {
        try
        {
            HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(_Uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            Stream stream = await response.Content.ReadAsStreamAsync();
            FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }

        ZipExtractor zipExtractor = new(path, targetPath);
        int err = zipExtractor.Extract();
        if (err != 0)
        {
            return;
        }

    }

 
}

