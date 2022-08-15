using GameKit;
using StarkSDKSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class API : SingletonMgr<API>
{
    public static string appid = "202207131455341765125";
    public static string appKey = "c1b0fb7e6709daf7e046a14345d69025";
    public static string apiDomain = "-";

    public static string version = "1.0.0";

    public static Dictionary<string, string> router = new Dictionary<string, string>();
    public static IDictionary<string ,string>  publicParams = new Dictionary<string,string>();

    public static HttpWebRequest request;// 声明一个HttpWebRequest请求
    public static Stream reqstream;//获取一个请求流
    public static HttpWebResponse response; //接收返回来的数据
    public static Stream streamReceive;//获取响应流

    public static string resp = "";


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        //路由
        if(!router.ContainsKey("login"))
        router.Add("login", "/api-auth/dyAuth/getTokenByOpenId");

        //公共参数
        publicParams.Clear();
        publicParams.Add("appid", appid);
        publicParams.Add("cid", "1");
        publicParams.Add("version", version);
        publicParams.Add("device", "aaa");
        publicParams.Add("platform", "andoird");
        publicParams.Add("debug", "1");

        // GameObject.Find("debuglog");
    }

    /*  公共参数：
        appid : 20220713145534176525
        key: c1b0fb7e6709daf7e046a14345d69025
        cid: 1
        version:
        device:
        platform:
    */

    public static string UrlGenerate(string route, string MD5params)
    {
        string GeneratedUrl = apiDomain + router[route] +"?"+ MD5params;
        return GeneratedUrl;
    }


    /// <summary>
    /// 微信支付MD5签名算法，ASCII码字典序排序0,A,B,a,b
    /// </summary>
    /// <param name="InDict">待签名名键值对</param>
    /// <param name="TenPayV3_Key">用于签名的Key</param>
    /// <returns>MD5签名字符串</returns>
    public static string WePaySign(IDictionary<string, string> InDict, string TenPayV3_Key)
    {
        string nonce_str = RandomStr();
        string nonce_time =( new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()).ToString();

        if (InDict.ContainsKey("nonce_str")) InDict.Remove("nonce_str");
        if (InDict.ContainsKey("nonce_time")) InDict.Remove("nonce_time");
        InDict.Add("nonce_str", nonce_str);
        InDict.Add("nonce_time", nonce_time);

        string[] arrKeys = InDict.Keys.ToArray();
        Array.Sort(arrKeys, string.CompareOrdinal);  //参数名ASCII码从小到大排序；0,A,B,a,b;

        var StrA = new StringBuilder();

        foreach (var key in arrKeys)
        {
            string value = InDict[key];
            if (!String.IsNullOrEmpty(value)) //空值不参与签名
            {
                StrA.Append(key + "=")
                   .Append(value + "&");
            }
        }

        string temp = StrA.ToString();
        StrA.Append("key=" + TenPayV3_Key); //注：key为商户平台设置的密钥key
        return temp+"sign="+GetMD5Hash(StrA.ToString()).ToUpper();
    }

    /// <summary>
    /// 随机生成n位字符串
    /// </summary>
    /// <returns></returns>
    public static string RandomStr()
    {
        var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var Charsarr = new char[6];
        var random = new System.Random();

        for (int i = 0; i < Charsarr.Length; i++)
        {
            Charsarr[i] = characters[random.Next(characters.Length)];
        }

        var resultString = new String(Charsarr);
        return resultString;
    }


    /// <summary>
    /// str转换MD5
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetMD5Hash(String str)
    {
        //把字符串转换成字节数组
        byte[] buffer = Encoding.Default.GetBytes(str);

        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        //md5加密
        byte[] cryptBuffer = md5.ComputeHash(buffer);
        string s = "";
        //把每一个字节 0-255，转换成两位16进制数     
        for (int i = 0; i < cryptBuffer.Length; i++)
        {
            //大X转黄的是大写字母，小X转换的是小写字母
            s += cryptBuffer[i].ToString("x2");
        }
        return s;
    }

 
  
    /// <summary>
    /// POST请求
    /// </summary>
    /// <param name="jsondata"></param>
    /// <returns></returns>
    public static IEnumerator PostData(string route, Dictionary<string, string> pdic, Action action)
    {
      
        string uri = UrlGenerate(route, WePaySign(publicParams, appKey));
        /* Stream*/
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in pdic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        Debug.Log("自定义参数：" + builder.ToString());
        byte[] byteArray = System.Text.Encoding.Default.GetBytes(builder.ToString());

        var _request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
        _request.uploadHandler = new UploadHandlerRaw(byteArray);
        _request.downloadHandler = new DownloadHandlerBuffer();
        _request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        yield return _request.SendWebRequest();
        Debug.Log("UnityWebRequest.responseCode:" + _request.responseCode);

        if (_request.isHttpError || _request.isNetworkError)
        {
            resp = _request.error;
            action();
        }
        else
        {
            resp = _request.downloadHandler.text;
            action();
        }
    }
}
