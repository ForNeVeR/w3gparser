using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Web;

namespace Deerchao.War3Share.Utility
{
    public class HttpClient
    {
        #region fields
        private bool keepContext;
        private string defaultLanguage = "zh-CN";
        private Encoding defaultEncoding = Encoding.UTF8;
        private string accept = "*/*";
        private string userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private HttpVerb verb = HttpVerb.GET;
        private HttpClientContext context;
        private readonly List<HttpUploadingFile> files = new List<HttpUploadingFile>();
        private readonly Dictionary<string, string> postingData = new Dictionary<string, string>();
        private string url;
        private WebHeaderCollection responseHeaders;
        private int startPoint;
        private int endPoint;
        #endregion

        #region events
        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;

        private void OnStatusUpdate(StatusUpdateEventArgs e)
        {
            EventHandler<StatusUpdateEventArgs> temp = StatusUpdate;

            if (temp != null)
                temp(this, e);
        }
        #endregion

        #region properties
        /// <summary>
        /// �Ƿ��Զ��ڲ�ͬ������䱣��Cookie, Referer
        /// </summary>
        public bool KeepContext
        {
            get { return keepContext; }
            set { keepContext = value; }
        }

        /// <summary>
        /// �����Ļ�Ӧ������
        /// </summary>
        public string DefaultLanguage
        {
            get { return defaultLanguage; }
            set { defaultLanguage = value; }
        }

        /// <summary>
        /// GetString()������ܴ�HTTPͷ��Meta��ǩ�л�ȡ������Ϣ,��ʹ�ô˱�������ȡ�ַ���
        /// </summary>
        public Encoding DefaultEncoding
        {
            get { return defaultEncoding; }
            set { defaultEncoding = value; }
        }

        /// <summary>
        /// ָʾ����Get������Post����
        /// </summary>
        public HttpVerb Verb
        {
            get { return verb; }
            set { verb = value; }
        }

        /// <summary>
        /// Ҫ�ϴ����ļ�.�����Ϊ�����Զ�תΪPost����
        /// </summary>
        public List<HttpUploadingFile> Files
        {
            get { return files; }
        }

        /// <summary>
        /// Ҫ���͵�Form����Ϣ
        /// </summary>
        public Dictionary<string, string> PostingData
        {
            get { return postingData; }
        }

        /// <summary>
        /// ��ȡ������������Դ�ĵ�ַ
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        /// <summary>
        /// �����ڻ�ȡ��Ӧ��,��ʱ��¼��Ӧ��HTTPͷ
        /// </summary>
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        /// <summary>
        /// ��ȡ��������������Դ����
        /// </summary>
        public string Accept
        {
            get { return accept; }
            set { accept = value; }
        }

        /// <summary>
        /// ��ȡ�����������е�HttpͷUser-Agent��ֵ
        /// </summary>
        public string UserAgent
        {
            get { return userAgent; }
            set { userAgent = value; }
        }

        /// <summary>
        /// ��ȡ������Cookie��Referer
        /// </summary>
        public HttpClientContext Context
        {
            get { return context; }
            set { context = value; }
        }

        /// <summary>
        /// ��ȡ�����û�ȡ���ݵ���ʼ��,���ڶϵ�����,���߳����ص�
        /// </summary>
        public int StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        /// <summary>
        /// ��ȡ�����û�ȡ���ݵĽ�����,���ڶϵ�����,���³����ص�.
        /// ���Ϊ0,��ʾ��ȡ��Դ��StartPoint��ʼ��ʣ������
        /// </summary>
        public int EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        #endregion

        #region constructors
        /// <summary>
        /// �����µ�HttpClientʵ��
        /// </summary>
        public HttpClient()
            : this(null)
        {
        }

        /// <summary>
        /// �����µ�HttpClientʵ��
        /// </summary>
        /// <param name="url">Ҫ��ȡ����Դ�ĵ�ַ</param>
        public HttpClient(string url)
            : this(url, null)
        {
        }

        /// <summary>
        /// �����µ�HttpClientʵ��
        /// </summary>
        /// <param name="url">Ҫ��ȡ����Դ�ĵ�ַ</param>
        /// <param name="context">Cookie��Referer</param>
        public HttpClient(string url, HttpClientContext context)
            : this(url, context, false)
        {
        }

        /// <summary>
        /// �����µ�HttpClientʵ��
        /// </summary>
        /// <param name="url">Ҫ��ȡ����Դ�ĵ�ַ</param>
        /// <param name="context">Cookie��Referer</param>
        /// <param name="keepContext">�Ƿ��Զ��ڲ�ͬ������䱣��Cookie, Referer</param>
        public HttpClient(string url, HttpClientContext context, bool keepContext)
        {
            this.url = url;
            this.context = context;
            this.keepContext = keepContext;
            if (this.context == null)
                this.context = new HttpClientContext();
        }
        #endregion

        #region AttachFile
        /// <summary>
        /// �����������Ҫ�ϴ����ļ�
        /// </summary>
        /// <param name="fileName">Ҫ�ϴ����ļ�·��</param>
        /// <param name="fieldName">�ļ��ֶε�����(�൱��&lt;input type=file name=fieldName&gt;)���fieldName)</param>
        public void AttachFile(string fileName, string fieldName)
        {
            HttpUploadingFile file = new HttpUploadingFile(fileName, fieldName);
            files.Add(file);
        }

        /// <summary>
        /// �����������Ҫ�ϴ����ļ�
        /// </summary>
        /// <param name="data">Ҫ�ϴ����ļ�����</param>
        /// <param name="fileName">�ļ���</param>
        /// <param name="fieldName">�ļ��ֶε�����(�൱��&lt;input type=file name=fieldName&gt;)���fieldName)</param>
        public void AttachFile(byte[] data, string fileName, string fieldName)
        {
            HttpUploadingFile file = new HttpUploadingFile(data, fileName, fieldName);
            files.Add(file);
        }
        #endregion

        /// <summary>
        /// ���PostingData, Files, StartPoint, EndPoint, ResponseHeaders, ����Verb����ΪGet.
        /// �ڷ���һ������������Ϣ�������,������ô˷������ֹ�������Ӧ������ʹ��һ�����󲻻��ܵ�Ӱ��.
        /// </summary>
        public void Reset()
        {
            verb = HttpVerb.GET;
            files.Clear();
            postingData.Clear();
            responseHeaders = null;
            startPoint = 0;
            endPoint = 0;
        }

        private HttpWebRequest CreateRequest()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.AllowAutoRedirect = true;
            req.CookieContainer = new CookieContainer();
            req.Headers.Add("Accept-Language", defaultLanguage);
            req.Accept = accept;
            req.UserAgent = userAgent;
            req.KeepAlive = false;

            if (context.Cookies != null)
                req.CookieContainer.Add(context.Cookies);
            if (!string.IsNullOrEmpty(context.Referer))
                req.Referer = context.Referer;

            if (verb == HttpVerb.HEAD)
            {
                req.Method = "HEAD";
                return req;
            }

            if (postingData.Count > 0 || files.Count > 0)
                verb = HttpVerb.POST;

            if (verb == HttpVerb.POST)
            {
                req.Method = "POST";

                MemoryStream memoryStream = new MemoryStream();
                StreamWriter writer = new StreamWriter(memoryStream);

                if (files.Count > 0)
                {
                    string newLine = "\r\n";
                    string boundary = Guid.NewGuid().ToString().Replace("-", "");
                    req.ContentType = "multipart/form-data; boundary=" + boundary;

                    foreach (string key in postingData.Keys)
                    {
                        writer.Write("--" + boundary + newLine);
                        writer.Write("Content-Disposition: form-data; name=\"{0}\"{1}{1}", key, newLine);
                        writer.Write(postingData[key] + newLine);
                    }

                    foreach (HttpUploadingFile file in files)
                    {
                        writer.Write("--" + boundary + newLine);
                        writer.Write(
                            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}",
                            file.FieldName,
                            file.FileName,
                            newLine
                            );
                        writer.Write("Content-Type: application/octet-stream" + newLine + newLine);
                        writer.Flush();
                        memoryStream.Write(file.Data, 0, file.Data.Length);
                        writer.Write(newLine);
                        writer.Write("--" + boundary + newLine);
                    }
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded";
                    StringBuilder sb = new StringBuilder();
                    foreach (string key in postingData.Keys)
                    {
                        sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(postingData[key]));
                    }
                    if (sb.Length > 0)
                        sb.Length--;
                    writer.Write(sb.ToString());
                }

                writer.Flush();

                using (Stream stream = req.GetRequestStream())
                {
                    memoryStream.WriteTo(stream);
                }
            }

            if (startPoint != 0 && endPoint != 0)
                req.AddRange(startPoint, endPoint);
            else if (startPoint != 0 && endPoint == 0)
                req.AddRange(startPoint);

            return req;
        }

        /// <summary>
        /// ����һ���µ�����,�����ػ�õĻ�Ӧ
        /// ���ô˷�����Զ���ᴥ��StatusUpdate�¼�.
        /// </summary>
        /// <returns>��Ӧ��HttpWebResponse</returns>
        public HttpWebResponse GetResponse()
        {
            HttpWebRequest req = CreateRequest();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            responseHeaders = res.Headers;
            if (keepContext)
            {
                context.Cookies = res.Cookies;
                context.Referer = url;
            }
            return res;
        }

        /// <summary>
        /// ����һ���µ�����,�����ػ�Ӧ���ݵ���
        /// ���ô˷�����Զ���ᴥ��StatusUpdate�¼�.
        /// </summary>
        /// <returns>������Ӧ�������ݵ���</returns>
        public Stream GetStream()
        {
            return GetResponse().GetResponseStream();
        }

        /// <summary>
        /// ����һ���µ�����,�����ֽ�������ʽ���ػ�Ӧ������
        /// ���ô˷����ᴥ��StatusUpdate�¼�
        /// </summary>
        /// <returns>������Ӧ�������ݵ��ֽ�����</returns>
        public byte[] GetBytes()
        {
            HttpWebResponse res = GetResponse();
            int length = (int)res.ContentLength;

            MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = new byte[0x100];
            Stream rs = res.GetResponseStream();
            for (int i = rs.Read(buffer, 0, buffer.Length); i > 0; i = rs.Read(buffer, 0, buffer.Length))
            {
                memoryStream.Write(buffer, 0, i);
                OnStatusUpdate(new StatusUpdateEventArgs((int)memoryStream.Length, length));
            }
            rs.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// ����һ���µ�����,��Httpͷ,��Html Meta��ǩ,��DefaultEncodingָʾ�ı�����Ϣ�Ի�Ӧ�������
        /// ���ô˷����ᴥ��StatusUpdate�¼�
        /// </summary>
        /// <returns>�������ַ���</returns>
        public string GetString()
        {
            byte[] data = GetBytes();
            string encodingName = GetEncodingFromHeaders();

            if (encodingName == null)
                encodingName = GetEncodingFromBody(data);

            Encoding encoding;
            if (encodingName == null)
                encoding = defaultEncoding;
            else
            {
                try
                {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    encoding = defaultEncoding;
                }
            }
            return encoding.GetString(data);
        }

        /// <summary>
        /// ����һ���µ�����,�Ի�Ӧ������������ָ���ı�����н���
        /// ���ô˷����ᴥ��StatusUpdate�¼�
        /// </summary>
        /// <param name="encoding">ָ���ı���</param>
        /// <returns>�������ַ���</returns>
        public string GetString(Encoding encoding)
        {
            byte[] data = GetBytes();
            return encoding.GetString(data);
        }

        private string GetEncodingFromHeaders()
        {
            string encoding = null;
            string contentType = responseHeaders["Content-Type"];
            if (contentType != null)
            {
                int i = contentType.IndexOf("charset=");
                if (i != -1)
                {
                    encoding = contentType.Substring(i + 8);
                }
            }
            return encoding;
        }

        private string GetEncodingFromBody(byte[] data)
        {
            string encodingName = null;
            string dataAsAscii = Encoding.ASCII.GetString(data);
            if (dataAsAscii != null)
            {
                int i = dataAsAscii.IndexOf("charset=");
                if (i != -1)
                {
                    int j = dataAsAscii.IndexOf("\"", i);
                    if (j != -1)
                    {
                        int k = i + 8;
                        encodingName = dataAsAscii.Substring(k, (j - k) + 1);
                        char[] chArray = new char[2] { '>', '"' };
                        encodingName = encodingName.TrimEnd(chArray);
                    }
                }
            }
            return encodingName;
        }

        /// <summary>
        /// ����һ���µ�Head����,��ȡ��Դ�ĳ���
        /// ����������PostingData, Files, StartPoint, EndPoint, Verb
        /// </summary>
        /// <returns>���ص���Դ����</returns>
        public int HeadContentLength()
        {
            Reset();
            HttpVerb lastVerb = verb;
            verb = HttpVerb.HEAD;
            using (HttpWebResponse res = GetResponse())
            {
                verb = lastVerb;
                return (int)res.ContentLength;
            }
        }

        /// <summary>
        /// ����һ���µ�����,�ѻ�Ӧ���������ݱ��浽�ļ�
        /// ���ô˷����ᴥ��StatusUpdate�¼�
        /// ���ָ�����ļ�����,���ᱻ����
        /// </summary>
        /// <param name="fileName">Ҫ������ļ�·��</param>
        public void SaveAsFile(string fileName)
        {
            SaveAsFile(fileName, FileExistsAction.Overwrite);
        }

        /// <summary>
        /// ����һ���µ�����,�ѻ�Ӧ���������ݱ��浽�ļ�
        /// ���ô˷����ᴥ��StatusUpdate�¼�
        /// </summary>
        /// <param name="fileName">Ҫ������ļ�·��</param>
        /// <param name="existsAction">ָ�����ļ�����ʱ��ѡ��</param>
        /// <returns>�Ƿ���Ŀ���ļ�д��������</returns>
        public bool SaveAsFile(string fileName, FileExistsAction existsAction)
        {
            byte[] data = GetBytes();
            switch (existsAction)
            {
                case FileExistsAction.Overwrite:
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)))
                        writer.Write(data);
                    return true;

                case FileExistsAction.Append:
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write)))
                        writer.Write(data);
                    return true;

                default:
                    if (!File.Exists(fileName))
                    {
                        using (
                            BinaryWriter writer =
                                new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                            writer.Write(data);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
    }

    public class HttpClientContext
    {
        private CookieCollection cookies;
        private string referer;

        public CookieCollection Cookies
        {
            get { return cookies; }
            set { cookies = value; }
        }

        public string Referer
        {
            get { return referer; }
            set { referer = value; }
        }
    }

    public enum HttpVerb
    {
        GET,
        POST,
        HEAD,
    }

    public enum FileExistsAction
    {
        Overwrite,
        Append,
        Cancel,
    }

    public class HttpUploadingFile
    {
        private string fileName;
        private string fieldName;
        private byte[] data;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public HttpUploadingFile(string fileName, string fieldName)
        {
            this.fileName = fileName;
            this.fieldName = fieldName;
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                data = bytes;
            }
        }

        public HttpUploadingFile(byte[] data, string fileName, string fieldName)
        {
            this.data = data;
            this.fileName = fileName;
            this.fieldName = fieldName;
        }
    }

    public class StatusUpdateEventArgs : EventArgs
    {
        private readonly int bytesGot;
        private readonly int bytesTotal;

        public StatusUpdateEventArgs(int got, int total)
        {
            bytesGot = got;
            bytesTotal = total;
        }

        /// <summary>
        /// �Ѿ����ص��ֽ���
        /// </summary>
        public int BytesGot
        {
            get { return bytesGot; }
        }

        /// <summary>
        /// ��Դ�����ֽ���
        /// </summary>
        public int BytesTotal
        {
            get { return bytesTotal; }
        }
    }
}
