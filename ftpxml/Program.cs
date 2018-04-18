using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace ftpxml
{
  internal class Program
  {
    private static void Main(string[] args){
      string servername = ConfigurationManager.AppSettings["ftpServer"];
      string username = ConfigurationManager.AppSettings["ftpUsername"];
      string password = ConfigurationManager.AppSettings["ftpPassword"];
      Serverinfo server = new Serverinfo(servername, username, password);

      List<string> fileList = GetListOfFiles(server);

      string desiredFile = "books.xml";

      if (fileList.Contains(desiredFile))
      {
        DownloadFile(desiredFile, server);
      }
      
      //parseFile();
      //writeResults();
    }

    private static List<string> GetListOfFiles(Serverinfo server){
      FtpWebRequest request = (FtpWebRequest)WebRequest.Create(server.ServerName);
      request.Method = WebRequestMethods.Ftp.ListDirectory;
      request.Credentials = new NetworkCredential(server.Username, server.Password);
      request.UsePassive = true;
      request.UseBinary = true;
      request.KeepAlive = true;
      FtpWebResponse response = (FtpWebResponse) request.GetResponse();
      StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new NullReferenceException());
      String[] fileNameArray = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      var fileList = fileNameArray.ToList();
      return fileList;
    }
    private static void DownloadFile(string fileName, Serverinfo server){
      string fullFilePath = server.ServerName + "/" + fileName;
      string localDestinationPath = ConfigurationManager.AppSettings["localDestinationPath"];
      string localDestinationPathFileName = localDestinationPath + "\\" + fileName;

      var request = (FtpWebRequest) WebRequest.Create(fullFilePath);
      request.Method = WebRequestMethods.Ftp.DownloadFile;
      request.Credentials = new NetworkCredential(server.Username, server.Password);
      request.UsePassive = true;
      request.UseBinary = true;
      request.KeepAlive = false;
      FtpWebResponse response = (FtpWebResponse)request.GetResponse();
      Stream responseStream = response.GetResponseStream();
      StreamReader reader = new StreamReader(responseStream);
      using (FileStream writer = new FileStream(localDestinationPathFileName, FileMode.Create)) {
        long length = response.ContentLength;
        int bufferSize = 2048;
        int readCount;
        byte[] buffer = new byte[bufferSize];
        readCount = responseStream.Read(buffer, 0, bufferSize);
        while (readCount > 0) {
          writer.Write(buffer, 0, readCount);
          readCount = responseStream.Read(buffer, 0, bufferSize);
        }
      };
      reader.Close();
      response.Close();

    }


  }
}