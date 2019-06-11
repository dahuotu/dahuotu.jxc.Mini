using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using StanSoft;
using System.IO.Compression;
using System.Web.Security;
using System.Security.Cryptography;

public partial class _api : System.Web.UI.Page
{
    string postString = "20181207v1.0";
    private static string txtKey = "PatrickpanP=";
    private static string txtIV = "LiuJineagel=";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request["act"] != null)
        {
            switch (Request["act"].ToString())
            {
                case "jixianci":
                    jixianci();
                    break;
                case "xiaoshuo":
                    xiaoshuo();
                    break;
                case "xiaoshuo_list":
                    xiaoshuo_list();
                    break;
                case "xiaoshuo_page":
                    xiaoshuo_page();
                    break;
                case "shenping_list":
                    shenping_list();
                    break;
                default:
                    break;
            }
        }
        else
        {

        }
        Response.Write(postString);
        Response.End();
    }

    /// <summary>
    /// 极限词查询
    /// </summary>
    private void jixianci()
    {
        string strResult = "";
        if (Request["val"] != null)
        {
            strResult = HttpPost("http://www.mgzxzs.com/sytool/chajixianci/ci_post.php", "data=" + HttpUtility.UrlEncode(Request["val"].ToString(), System.Text.Encoding.UTF8) + "&gjif=zhchaci&dataType=text");
        }
        postString = strResult;
    }

    /// <summary>
    /// 追更小说搜索
    /// </summary>
    private void xiaoshuo()
    {
        string strResult = "";
        if (Request["val"] != null)
        {
            strResult = HttpGet("http://www.sodu.cc/result.html", "searchstr=" + HttpUtility.UrlEncode(Request["val"].ToString(), System.Text.Encoding.UTF8));
            strResult = RegList(@"<div style=""width:188px;float:left;"">(.*)</div>", strResult);
            strResult = RegAlink(strResult);
        }
        postString = strResult;
    }

    /// <summary>
    /// 追更小说列表
    /// </summary>
    private void xiaoshuo_list()
    {
        string strResult = "";
        if (Request["val"] != null)
        {
            strResult = GetHtmlAutoEncoding(Decrypt(Request["val"].ToString()));
            strResult = RegList(@"<div class=""main-html"">[\s\t\n]+(.*)</div>", strResult);
            strResult = RegUpdateAlink(strResult);
        }
        postString = strResult;
    }

    /// <summary>
    /// 追更小说内容
    /// </summary>
    private void xiaoshuo_page()
    {
        StringBuilder sbPage = new StringBuilder();
        StringBuilder sbData = new StringBuilder();

        string strResult = "";
        Article article = null;
        if (Request["val"] != null)
        {
            strResult = GetHtmlAutoEncoding(Decrypt(Request["val"].ToString()));
            // article对象包含Title(标题)，PublishDate(发布日期)，Content(正文)和ContentWithTags(带标签正文)四个属性
            article = Html2Article.GetArticle(strResult);
            strResult = article.ContentWithTags;
        }

        postString = strResult;
    }


    /// <summary>
    /// 正则链接搜索到的链接和标题返回JSON
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    string RegAlink(string input)
    {
        StringBuilder sbLink = new StringBuilder();
        StringBuilder sbData = new StringBuilder();
        string pattern = @"<a\b[^>]+\bhref=""([^""]*)"">([\s\S]*?)</a>";
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex(pattern, options);
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            sbData.AppendFormat(",{{");
            sbData.AppendFormat("\"title\": \"{0}\",", match.Groups[2].Value);
            sbData.AppendFormat("\"href\": \"{0}\"", Encrypt(match.Groups[1].Value));
            sbData.AppendFormat("}}");
        }

        sbLink.AppendFormat("{{");
        sbLink.AppendFormat("    \"msg\": \"success\",");
        sbLink.AppendFormat("    \"data\": [{0}],", sbData.ToString().Substring(1, sbData.Length - 1));
        sbLink.AppendFormat("    \"code\": \"0\"");
        sbLink.AppendFormat("}}");

        return sbLink.ToString();
    }

    /// <summary>
    /// 正则追更链接搜索到的链接和标题返回JSON
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    string RegUpdateAlink(string input)
    {
        StringBuilder sbLink = new StringBuilder();
        StringBuilder sbData = new StringBuilder();
        string pattern = @"<a\b[^>]+\bhref=""([^""]*)""[^>]*>([\s\S]*?)</a>";
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex(pattern, options);
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            sbData.AppendFormat(",{{");
            sbData.AppendFormat("\"title\": \"{0}\",", Regex.Replace(match.Groups[2].Value, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase).Replace("(推荐阅读)", "").Trim());
            sbData.AppendFormat("\"href\": \"{0}\"", Encrypt(RegList(@"chapterurl=(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", match.Groups[1].Value).Replace("chapterurl=", "").Trim()));
            sbData.AppendFormat("}}");
        }

        sbLink.AppendFormat("{{");
        sbLink.AppendFormat("    \"msg\": \"success\",");
        sbLink.AppendFormat("    \"data\": [{0}],", sbData.ToString().Substring(1, sbData.Length - 1));
        sbLink.AppendFormat("    \"code\": \"0\"");
        sbLink.AppendFormat("}}");

        return sbLink.ToString();
    }

    /// <summary>
    /// 神评论分页列表
    /// </summary>
    private void shenping_list()
    {
        StringBuilder str = new StringBuilder();
        str.AppendFormat("{{");
        str.AppendFormat("    \"datas\": [{{");
        str.AppendFormat("        \"id\": \"1\",");
        str.AppendFormat("        \"content\": \"1\"");
        str.AppendFormat("    }}, {{");
        str.AppendFormat("        \"id\": \"2\",");
        str.AppendFormat("        \"content\": \"2\"");
        str.AppendFormat("    }}]");
        str.AppendFormat("}}");

        if (Request["size"] != null && Request["index"] != null)
        {

        }
        postString = str.ToString();
    }


    #region 公共方法

    /// <summary>
    /// POST请求与获取结果
    /// </summary>
    private static string HttpPost(string Url, string postDataStr)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
        request.Accept = "application/json, text/javascript, */*; q=0.01";
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        request.ContentLength = postDataStr.Length;
        StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
        writer.Write(postDataStr);
        writer.Flush();
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string encoding = response.ContentEncoding;
        if (encoding == null || encoding.Length < 1)
        {
            encoding = "UTF-8"; //默认编码
        }
        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
        string retString = reader.ReadToEnd();
        return retString;
    }


    /// <summary>
    /// GET请求与获取结果
    /// </summary>
    private static string HttpGet(string Url, string postDataStr)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
        request.Method = "GET";
        request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream myResponseStream = response.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
        string retString = myStreamReader.ReadToEnd();
        myStreamReader.Close();
        myResponseStream.Close();

        return retString;
    }

    /// <summary>
    /// 获取页面源码（自动获取页面的编码格式）                
    /// <param name="url">url</param>
    /// <returns>页面源码</returns>
    /// </summary>
    private static string GetHtmlAutoEncoding(string url)
    {
        HttpWebRequest req = null;
        HttpWebResponse resp = null;
        Stream stream = null;
        StreamReader read = null;
        try
        {
            string sUrl = url;
            req = (HttpWebRequest)HttpWebRequest.Create(sUrl);
            req.UserAgent = "Mozilla/5.0 (Windows NT 5.2; rv:6.0) Gecko/20100101 Firefox/6.0";
            req.Accept = "*/*";
            req.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
            req.ContentType = "text/xml";
            req.Timeout = 20000;

            resp = (HttpWebResponse)req.GetResponse();
            Encoding enc = Encoding.GetEncoding(resp.CharacterSet);
            string sHTML = string.Empty;
            stream = resp.GetResponseStream();
            if (resp.ContentEncoding.ToLower().Contains("gzip"))
            {
                stream = new GZipStream(stream, CompressionMode.Decompress);
                read = new StreamReader(stream, enc);
                sHTML = read.ReadToEnd();
            }
            else
            {
                string sChartSet = "";
                read = new StreamReader(stream, enc);
                sHTML = read.ReadToEnd();
                Match charSetMatch = Regex.Match(sHTML, "charset=(?<code>[a-zA-Z0-9\\-]+)", RegexOptions.IgnoreCase);
                sChartSet = charSetMatch.Groups["code"].Value;
                //if it's not utf-8,we should redecode the html.
                Regex rx = new Regex("([\u4e00-\u9fa5]{2,4})");
                if (!rx.IsMatch(sHTML))
                {
                    if (!string.IsNullOrEmpty(sChartSet.Trim()))
                        sHTML = Encoding.GetEncoding(sChartSet).GetString(enc.GetBytes(sHTML));
                }
            }
            return sHTML;
        }
        catch
        {
            return "";
        }
        finally
        {
            if (resp != null)
            {
                resp.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (read != null)
            {
                read.Close();
            }
        }
    }


    /// <summary>
    /// 正则搜索列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string RegList(string pattern, string input)
    {
        string result = "";
        RegexOptions options = RegexOptions.None;
        Regex regex = new Regex(pattern, options);
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            result += match.Value;
        }
        return result;
    }


    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="sKey"></param>
    /// <returns></returns>
    private static string Encrypt(string Text)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        byte[] inputByteArray;
        inputByteArray = Encoding.Default.GetBytes(Text);
        des.Key = Convert.FromBase64String(txtKey);
        des.IV = Convert.FromBase64String(txtIV);
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        StringBuilder ret = new StringBuilder();
        foreach (byte b in ms.ToArray())
        {
            ret.AppendFormat("{0:X2}", b);
        }
        return ret.ToString();
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="Text"></param>
    /// <param name="sKey"></param>
    /// <returns></returns>
    private static string Decrypt(string Text)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        int len;
        len = Text.Length / 2;
        byte[] inputByteArray = new byte[len];
        int x, i;
        for (x = 0; x < len; x++)
        {
            i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
            inputByteArray[x] = (byte)i;
        }
        des.Key = Convert.FromBase64String(txtKey);
        des.IV = Convert.FromBase64String(txtIV);
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        return Encoding.Default.GetString(ms.ToArray());
    }

    #endregion
}