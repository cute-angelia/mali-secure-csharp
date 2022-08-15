namespace Secure
{
  public class Secure {
    /// <summary>
    /// 微信支付MD5签名算法，ASCII码字典序排序0,A,B,a,b
    /// https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=4_3
    /// </summary>
    /// <param name="InDict">待签名名键值对</param>
    /// <param name="TenPayV3_Key">用于签名的Key</param>
    /// <returns>MD5签名字符串</returns>
    public static string WePaySign(IDictionary<string, string> InDict, string TenPayV3_Key)
    {
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

        StrA.Append("key=" + TenPayV3_Key); //注：key为商户平台设置的密钥key
        return StrFormat.GetMd5Hash(StrA.ToString()).ToUpper();
    }
  }
}