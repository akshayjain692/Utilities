using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
/*
* @(#)RWFNetworkManager.cs  7/20/2015
*
* Use of this code is subject to the terms and conditions of the contract
* with Reflexis Systems, Inc. Unauthorized reproduction or distribution of
* any material or programming content is prohibited.
*/
namespace RflxWindowsCore
{

    /**  Class Details : 
     * Helper Class for the all server calls. Each server call needs to be called by using getUrlResponse() only.
     * Compatible with Windows 10 : App Version 1.0 and above...</p>
     *
     * @author  Abhishek Pundikar
     * @version 1.0 7/20/2015
     * @since   7/20/2015
     */

    public static class RWCNetworkManager
    {

        private static ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private static HttpClient httpClient = new HttpClient();
        public static event EventHandler<bool> SessionTimeOutEvent;

        // to remove connection
        public static void removeConnection()
        {
            if (httpClient != null)
                httpClient = null;
        }

        /*
       *Get Specific Child by Name form Visualtree
       *@author Nikhil Menon
       *@created 09/16/2016
       */
        public static bool CheckURLValidity(String uriName)
        {
            Uri uriResult;
            return (Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && Uri.CheckSchemeName(uriResult.Scheme));
        }
        public static String getAppUrl(bool isLogin, String serverUrl)
        {
            String appUrl = serverUrl;
            String urlStr = appUrl;
            // set http or https based on local settings
            if (!appUrl.StartsWith("http://") && !appUrl.StartsWith("https://"))
            {
                if (!isLogin)
                {
                    if (localSettings.Values.ContainsKey("SecureEntireApp"))
                    {
                        if (localSettings.Values["SecureEntireApp"].ToString().Equals("YES"))
                        {
                            urlStr = "https://" + appUrl;
                        }
                        else
                        {
                            urlStr = "http://" + appUrl;
                        }
                    }
                }
                else
                {
                    if (localSettings.Values.ContainsKey("SecureLogin"))
                    {
                        if (localSettings.Values["SecureLogin"].ToString().Equals("YES"))
                        {
                            urlStr = "https://" + appUrl;
                        }
                        else
                        {
                            urlStr = "http://" + appUrl;
                        }
                    }
                }
            }
            else if (!appUrl.StartsWith("http://") && !appUrl.StartsWith("https://"))
            {
                urlStr = "http://" + appUrl;
            }

            // put / at end if not present
            if (!urlStr.EndsWith("/"))
            {
                urlStr = urlStr + "/";
            }
            return urlStr;
        }


        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType)
        {
            return await getURLresp(serverUrl, servletPath, urlParams, attachParams, httpMethod, respType, false, false, false, null);
        }
        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType, Dictionary<string, string> requestHeaderParams)
        {
            return await getURLresp(serverUrl, servletPath, urlParams, attachParams, httpMethod, respType, false, false, false, requestHeaderParams);
        }
        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType, bool login)
        {
            return await getURLresp(serverUrl, servletPath, urlParams, attachParams, httpMethod, respType, login, false, false, null);
        }
        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType, bool login, bool isMultipart)
        {
            return await getURLresp(serverUrl, servletPath, urlParams, attachParams, httpMethod, respType, login, true, false, null);
        }
        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType, bool login, bool isMultipart, bool isFeedBackAttachment)
        {
            return await getURLresp(serverUrl, servletPath, urlParams, attachParams, httpMethod, respType, login, isMultipart, isFeedBackAttachment, null);
        }

        public async static Task<String> getURLresp(String serverUrl, String servletPath, Dictionary<String, String> urlParams, Dictionary<String, String> attachParams, String httpMethod, String respType, bool login, bool isMultipart, bool isFeedBackAttachment, Dictionary<string, string> requestHeaderParams)
        {            
            String stringParams = "";
            String retMsg = "";
            // -- need to check with 'secure app check'
            String str = getAppUrl(login, serverUrl);
            String urlStr = str + servletPath;

            // doing this for cognos logout URL only
            if ("GET".Equals(httpMethod) && serverUrl.Contains(".jsp") && "".Equals(servletPath) && urlStr.EndsWith("/"))
            {
                urlStr = urlStr.Remove(urlStr.Length - 1);
            }

            if (String.IsNullOrEmpty(respType) == false)
            {
                urlStr = urlStr + "." + respType;
            }

            HttpResponseMessage _response = new HttpResponseMessage();
            HttpBaseProtocolFilter _filter = new HttpBaseProtocolFilter();
            var _manager = _filter.CookieManager;

            try
            {
                Uri url = new Uri(urlStr);
                if (urlParams.ContainsKey("CorpUserName") || urlParams.ContainsKey("CorpPassword"))
                {
                    PasswordCredential _credentials = new PasswordCredential();
                    _credentials.UserName = urlParams["CorpUserName"].ToString();
                    _credentials.Password = urlParams["CorpPassword"].ToString();
                    _filter.ServerCredential = _credentials;
                    _filter.AllowAutoRedirect = false;
                }


                httpClient = new HttpClient(_filter);
                bool networkStatus = RWCNetworkReachability.CheckNetworkReachability();
                if (requestHeaderParams == null) requestHeaderParams = new Dictionary<string, string>();
                if (!requestHeaderParams.ContainsKey("Referer")) requestHeaderParams.Add("Referer", serverUrl);
                if (requestHeaderParams != null)
                {
                    if (urlParams.ContainsKey("authToken"))
                    {
                        if (!requestHeaderParams.ContainsKey("X-reflexis-csrf-token-X")) requestHeaderParams.Add("X-reflexis-csrf-token-X", urlParams["authToken"]);
                        if (!requestHeaderParams.ContainsKey("x-authToken")) requestHeaderParams.Add("x-authToken", urlParams["authToken"]);
                        if (!requestHeaderParams.ContainsKey("authToken")) requestHeaderParams.Add("authToken", urlParams["authToken"]);
                        if(localSettings.Values["domainKey"] != null)if (!requestHeaderParams.ContainsKey("X-reflexis-domain-X")) requestHeaderParams.Add("X-reflexis-domain-X", localSettings.Values["domainKey"].ToString());
                    } 
                    foreach (KeyValuePair<string, string> headerParam in requestHeaderParams)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerParam.Key, headerParam.Value);
                    }
                }

                // Check Network Reachability
                if (networkStatus == false)
                {
                    return "NC";
                }

                //GET
                if ("GET".Equals(httpMethod))
                {
                    if (!urlParams.ContainsKey("random")) urlParams.Add("random", DateTime.Now.Ticks.ToString());
                    stringParams = RWCUtilities.StringifyURLParameters(urlParams);

                    url = new Uri(urlStr + "?" + stringParams);
                    HttpRequestMessage _requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                    //_requestMessage.Headers.IfModifiedSince = DateTime.Now;
                    _requestMessage.Headers.CacheControl.TryParseAdd("no-store");
                    _response = await httpClient.SendRequestAsync(_requestMessage);
                    //_response = await httpClient.GetAsync(url);

                    RWCLogManager.logDebugMessage("Server call url in GET is = " + url.ToString());
                    RWCLogManager.logDebugMessage("Server call urlparams in GET is = " + stringParams);
                }

                //POST	
                if ("POST".Equals(httpMethod))
                {
                    if (isMultipart)
                    {
                        if (urlParams.Any(var => var.Key.Equals("authToken")))
                        {
                            var authToken = urlParams.Where(var => var.Key.Equals("authToken")).First().Key + "=" + urlParams.Where(var => var.Key.Equals("authToken")).First().Value;
                            url = new Uri(urlStr + "?" + authToken);
                        }
                        HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();

                        //for each url param
                        foreach (KeyValuePair<String, String> param in urlParams)
                        {
                            String value = param.Value;
                            String key = param.Key;
                            if (String.IsNullOrEmpty(value))
                            {
                                value = "";
                            }
                            multipartContent.Add(new HttpStringContent(value), key);
                        }
                        // for each Attachment
                        if (attachParams != null)
                        {
                            IInputStream inputStream = null;
                            foreach (KeyValuePair<String, String> attach in attachParams)
                            {
                                String fileStr = attach.Value.ToLower();
                                StorageFile file = await StorageFile.GetFileFromPathAsync(fileStr);
                                inputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                                if (!isFeedBackAttachment)
                                {
                                    multipartContent.Add(new HttpStreamContent(inputStream), attach.Key, file.Name);
                                }
                                else
                                {
                                    multipartContent.Add(new HttpStreamContent(inputStream), attach.Key, file.Name + file.FileType);
                                }
                            }
                            _response = await httpClient.PostAsync(url, multipartContent);
                            if (inputStream != null)
                                inputStream.Dispose();
                        }


                    }
                    else
                    {
                        if (!urlParams.ContainsKey("random")) urlParams.Add("random", DateTime.Now.Ticks.ToString());
                        HttpFormUrlEncodedContent formContent = new Windows.Web.Http.HttpFormUrlEncodedContent(urlParams);
                        _response = await httpClient.PostAsync(url, formContent);
                    }
                    stringParams = RWCUtilities.StringifyURLParameters(urlParams);

                    RWCLogManager.logDebugMessage("Server call url in POST is = " + urlStr);
                    RWCLogManager.logDebugMessage("Server call urlparams in POST is = " + stringParams);
                }
                else if ("DELETE".Equals(httpMethod))
                {
                    RWCLogManager.logDebugMessage("Server call url in DELETE is = " + urlStr);
                    _response = await httpClient.DeleteAsync(url);
                }


                if (_response.StatusCode == Windows.Web.Http.HttpStatusCode.Unauthorized)
                {
                    SessionTimeOutEvent?.Invoke(new object(), true);
                    return "";
                }
                else
                {
                    string message = await _response.Content.ReadAsStringAsync();
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    // byte[] isoBites = Encoding.GetEncoding("iso-8859-1").GetBytes(message);
                    byte[] isoBites = Encoding.UTF8.GetBytes(message);
                    retMsg = UTF8Encoding.UTF8.GetString(isoBites);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                retMsg = "SD";
            }
            RWCLogManager.logDebugMessage("SERVER RESPONSE :" + retMsg);
            return retMsg;
        }


        public async static Task<String> getURLrespForStringDataParameters(String serverUrl, String servletPath, String data, Dictionary<String, String> urlParams, String httpMethod, String respType, bool isMultipart, List<String> filePath, Dictionary<string, string> requestHeaderParams)
        {
            String str1 = "";
            String retMsg = "";
            // -- need to check with 'secure app check'
            String str = getAppUrl(false, serverUrl);
            String urlStr = str + servletPath;
            if (String.IsNullOrEmpty(respType) == false)
            {
                urlStr = urlStr + "." + respType;
            }
            HttpResponseMessage response = new HttpResponseMessage();
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            var manager = filter.CookieManager;
            filter.AllowUI = false;
            httpClient = new HttpClient(filter);

            str1 = RWCUtilities.StringifyURLParameters(urlParams);
            String tempUrl = urlStr + "?" + str1;
            if (tempUrl.EndsWith("?"))
            {
                tempUrl = tempUrl.TrimEnd('?');
            }
            Uri url = new Uri(tempUrl);

            try
            {
                httpClient = new HttpClient();
                bool networkStatus = RWCNetworkReachability.CheckNetworkReachability();
                // Check Network Reachability
                if (networkStatus == false)
                {
                    return "NC";
                }

                if (requestHeaderParams == null) requestHeaderParams = new Dictionary<string, string>();
                if (!requestHeaderParams.ContainsKey("Referer")) requestHeaderParams.Add("Referer", serverUrl);
                if (!requestHeaderParams.ContainsKey("X-reflexis-csrf-token-X")) requestHeaderParams.Add("X-reflexis-csrf-token-X", localSettings.Values["authToken"].ToString());
                if (!requestHeaderParams.ContainsKey("x-authToken")) requestHeaderParams.Add("x-authToken", localSettings.Values["authToken"].ToString());
                if (!requestHeaderParams.ContainsKey("authToken")) requestHeaderParams.Add("authToken", localSettings.Values["authToken"].ToString());
                //if (!requestHeaderParams.ContainsKey("Content-Type")) requestHeaderParams.Add("Content-Type", "application/json");
                if (requestHeaderParams != null)
                {
                    foreach (KeyValuePair<string, string> headerParam in requestHeaderParams)
                    {
                        if (!httpClient.DefaultRequestHeaders.ContainsKey(headerParam.Key))
                            httpClient.DefaultRequestHeaders.Add(headerParam.Key, headerParam.Value);
                    }
                }
                //GET
                if ("GET".Equals(httpMethod))
                {
                    // url = url;
                    RWCLogManager.logDebugMessage("Server call url in GET is = " + url.ToString());
                    RWCLogManager.logDebugMessage("Server call urlparams in GET is = " + str1);

                    response = await httpClient.GetAsync(url);

                }
                //POST	
                if ("POST".Equals(httpMethod))
                {
                    if (isMultipart)
                    {
                        HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();
                        IInputStream inputStream = null;
                        // for each Attachment
                        foreach (String attach in filePath)
                        {
                            String fileStr = attach.ToLower();
                            StorageFile file = await StorageFile.GetFileFromPathAsync(fileStr);
                            inputStream = await file.OpenAsync(FileAccessMode.Read);
                            multipartContent.Add(new HttpStreamContent(inputStream), "attachments", file.Name);
                        }
                        response = await httpClient.PostAsync(url, multipartContent);
                        if (inputStream != null)
                            inputStream.Dispose();
                    }
                    else
                    {
                        HttpStringContent stringContent = new HttpStringContent(data, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                        response = await httpClient.PostAsync(url, stringContent);
                    }
                    RWCLogManager.logDebugMessage("Server call url in POST is = " + url);
                    RWCLogManager.logDebugMessage("Server call urlparams in POST is = " + data);
                }
                else if ("DELETE".Equals(httpMethod))
                {
                    RWCLogManager.logDebugMessage("Server call url in DELETE is = " + urlStr);
                    response = await httpClient.DeleteAsync(url);
                }
                else if ("PUT".Equals(httpMethod))
                {
                    RWCLogManager.logDebugMessage("Server call url in PUT is = " + url);
                    RWCLogManager.logDebugMessage("Server call urlparams in PUT is = " + data);

                    HttpStringContent stringContent = new HttpStringContent(data, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                    response = await httpClient.PutAsync(url, stringContent);
                }

                if (response.StatusCode == Windows.Web.Http.HttpStatusCode.Unauthorized)
                {
                    SessionTimeOutEvent?.Invoke(new object(), true);
                    return "";
                }
                else
                {
                    //retMsg = response.Content.ToString();
                    string message = await response.Content.ReadAsStringAsync();
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    byte[] isoBites = Encoding.GetEncoding("iso-8859-1").GetBytes(message);
                    //byte[] isoBites = Encoding.UTF8.GetBytes(message);
                    retMsg = UTF8Encoding.UTF8.GetString(isoBites);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                retMsg = "SD";
            }
            RWCLogManager.logDebugMessage("SERVER RESPONSE :" + retMsg);
            return retMsg;
        }

        public async static Task<String> GETResponseForString(String path)
        {
            try
            {
                HttpBaseProtocolFilter _filter = new HttpBaseProtocolFilter();
                //_filter.ClearAuthenticationCache();
                _filter.CookieUsageBehavior = HttpCookieUsageBehavior.NoCookies;
                HttpClient httpClient = new HttpClient(_filter);
                Uri url = new Uri(path);
                HttpRequestMessage _requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                _requestMessage.Headers.CacheControl.TryParseAdd("no-store");
                var response = await httpClient.SendRequestAsync(_requestMessage);
                var message = await response.Content.ReadAsStringAsync();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                byte[] isoBites = Encoding.GetEncoding("iso-8859-1").GetBytes(message);
                string retMsg = UTF8Encoding.UTF8.GetString(isoBites);
                return retMsg;
            }
            catch (Exception e)
            {
                RWCLogManager.logDebugMessage(e.Message);
                return "";
            }
        }

        public async static Task<String> GetURLRespForNotes(String serverUrl, String servletPath, Dictionary<String, String> urlParams)
        {
            String retMsg = "";
            String str = getAppUrl(true, serverUrl);
            String stringParams = "";
            String urlStr = str + servletPath;
            HttpResponseMessage _response = new HttpResponseMessage();
            try
            {
                Uri url = new Uri(serverUrl);

                //Check Network Reachability
                if (!RWCNetworkReachability.HasNetworkReachability)
                    throw new Exception("Network is not reachable.");

                httpClient = new HttpClient();
                //GET
                if (!urlParams.ContainsKey("random")) urlParams.Add("random", DateTime.Now.Ticks.ToString());
                stringParams = RWCUtilities.StringifyURLParameters(urlParams);
                url = new Uri(urlStr + "?" + stringParams);
                HttpRequestMessage _requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                _requestMessage.Headers.CacheControl.TryParseAdd("no-store");
                _response = await httpClient.SendRequestAsync(_requestMessage);
                IBuffer messageBuffer = await _response.Content.ReadAsBufferAsync();
                byte[] byteArray;
                CryptographicBuffer.CopyToByteArray(messageBuffer, out byteArray);
                retMsg = Encoding.UTF8.GetString(byteArray);
                RWCLogManager.logDebugMessage("Server call url in GET is = " + url.ToString());
                RWCLogManager.logDebugMessage("Server call urlparams in GET is = " + stringParams);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                retMsg = "NC";
            }

            RWCLogManager.logDebugMessage("SERVER RESPONSE :" + retMsg);
            return retMsg;
        }
    }
}
