using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ftpxml
{
  internal class Program
  {
    private static void Main(string[] args){
      Serverinfo server = new Serverinfo("ftp://aaa.bbb.com", "username", "password");
      List<string> fileList = GetListOfFiles(server);
      //DisplayListOfFiles(fileList);
      //chooseFile();
      //downloadFile();
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

    //private static void DisplayListOfFiles(string fileList)
    //{
    //  throw new NotImplementedException();
    //}

  }
}