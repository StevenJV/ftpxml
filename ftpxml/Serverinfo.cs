namespace ftpxml
{
  internal class Serverinfo
  {
    public string ServerName { get; set; }
    //private string _port;
    public string Username { get; set; }
    public string Password { get; set; }

    public Serverinfo(string serverName,  string username, string password){
      ServerName = serverName;
      //_port = port;
      Username = username;
      Password = password;
    }
    
  }
}