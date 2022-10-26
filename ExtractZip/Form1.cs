using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.Common;

namespace ExtractZip
{
    public partial class Form1 : Form
    {
        private ZipApplication _application;
        private WinCmdHelper _cmdHelper;
        private ZipHelper _zipHelper;

        public Form1()
        {
            InitializeComponent();
            _cmdHelper = new WinCmdHelper();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            download();
            //var path = @"C:\Users\Dew\Downloads\Slack\Project01_1.zip";
            //var extractPath = @"C:\Users\Dew\Downloads\Slack\Project01_1";
            //var cachePath = @"C:\Users\Dew\Downloads\Slack\";

            //_zipHelper = new ZipHelper(path, cachePath, 1);
            //_zipHelper.ExtractFile(extractPath, null, out string msg, "", "", true);
            //_zipHelper.Dispose();
            //Console.WriteLine(msg);
        }

        // ref: https://stackoverflow.com/questions/11125535/how-to-return-a-file-using-web-api
        //
        //public HttpResponseMessage GetFile(string id)
        //{
        //    if (String.IsNullOrEmpty(id))
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);

        //    string fileName;
        //    string localFilePath;
        //    int fileSize;
        //    localFilePath = getFileFromID(id, out fileName, out fileSize);
        //    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        //    response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
        //    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        //    response.Content.Headers.ContentDisposition.FileName = fileName;
        //    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        //    return response;
        //}

        // https://stackoverflow.com/questions/45711428/download-file-with-webclient-or-httpclient
        // 
        private async Task download()
        {
            var customHeaderValue = Guid.NewGuid().ToString("N");
            string remoteUri = "https://www.dbd.go.th/download/downloads/17_goodgov/";
            string filename = "form_goodgov1.pdf";
            string myStringWebResource = remoteUri + filename;

            using (HttpClient client = new HttpClient())
            {
                // client.DefaultRequestHeaders.Add("X-ResponseStreamTest", customHeaderValue);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("compress"));
                var stream = await client.GetStreamAsync(myStringWebResource);
                using (Stream s = File.Create(@"C:\form_goodgov1.pdf"))
                {
                    stream.CopyTo(s);
                } 
               
            }
        }
    }
}
