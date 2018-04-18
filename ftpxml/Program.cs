using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace ftpxml
{
  internal class Program
  {
    private static void Main(string[] args){
      string servername = ConfigurationManager.AppSettings["ftpServer"];
      string username = ConfigurationManager.AppSettings["ftpUsername"];
      string password = ConfigurationManager.AppSettings["ftpPassword"];
      Serverinfo server = new Serverinfo(servername, username, password);
      const string desiredFile = "books.xml";
      string localDestinationPath = ConfigurationManager.AppSettings["localDestinationPath"];
      string localDestinationPathFileName = localDestinationPath + "\\" + desiredFile;

      List<string> fileList = GetListOfFiles(server);

      if (!fileList.Contains(desiredFile)) {
        Console.WriteLine("File not found on remote server.");
        return;
      }

      DownloadFile(desiredFile, server, localDestinationPathFileName);

      List<string> bookTitles = ParseFile(localDestinationPathFileName);
      WriteResults(bookTitles);
      Console.ReadKey();
    }


    private static List<string> GetListOfFiles(Serverinfo server){
      FtpWebRequest request = (FtpWebRequest) WebRequest.Create(server.ServerName);
      request.Method = WebRequestMethods.Ftp.ListDirectory;
      request.Credentials = new NetworkCredential(server.Username, server.Password);
      request.UsePassive = true;
      request.UseBinary = true;
      request.KeepAlive = true;
      FtpWebResponse response = (FtpWebResponse) request.GetResponse();
      StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new ArgumentNullException());
      string[] fileNameArray =
        reader.ReadToEnd().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToArray();
      List<string> fileList = fileNameArray.ToList();
      return fileList;
    }

    private static void DownloadFile(string fileName, Serverinfo server, string destinationFile){
      string fullFilePath = server.ServerName + "/" + fileName;

      FtpWebRequest request = (FtpWebRequest) WebRequest.Create(fullFilePath);
      request.Method = WebRequestMethods.Ftp.DownloadFile;
      request.Credentials = new NetworkCredential(server.Username, server.Password);
      request.UsePassive = true;
      request.UseBinary = true;
      request.KeepAlive = false;
      FtpWebResponse response = (FtpWebResponse) request.GetResponse();
      Stream responseStream = response.GetResponseStream();
      StreamReader reader = new StreamReader(responseStream ?? throw new ArgumentNullException());
      using (FileStream writer = new FileStream(destinationFile, FileMode.Create)) {
        int length = response.ContentLength;
        const int bufferSize = 2048;
        byte[] buffer = new byte[bufferSize];
        var readCount = responseStream.Read(buffer, 0, bufferSize);
        while (readCount > 0) {
          writer.Write(buffer, 0, readCount);
          readCount = responseStream.Read(buffer, 0, bufferSize);
        }
      }

      reader.Close();
      response.Close();
    }

    private static List<string> ParseFile(string localDestinationPathFileName){
      List<string> allBookTitles = new List<string>();
      XmlReader reader = XmlReader.Create(localDestinationPathFileName);
      while (reader.Read())
        switch (reader.NodeType) {
          case XmlNodeType.Element:
            if (reader.Name == "title") {
              reader.Read();
              allBookTitles.Add(reader.Value);
            }

            break;
        }
      return allBookTitles;
    }

    private static void WriteResults(List<string> bookTitles){
      bookTitles.ForEach(Console.WriteLine);
    }
  }
}