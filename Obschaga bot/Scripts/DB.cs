using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SQLite;

namespace Obschaga_bot.Scripts;

public static class Db
{
  public static async Task<ProfileType> GetProfileType(long userId)
  {
    var profile = await GetProfile(userId);
    ProfileType profileType = ProfileType.Empty;
    if (profile is not null) 
      profileType = (ProfileType)profile.ProfileType!;
    return profileType;
  }
  
  public static async Task AddRegisterRequest(RegisterRequestElement registerRequest)
  {
    string connectionString = $"Data Source={Program.ExecuteDBPath};Version=3;";
    using SQLiteConnection db = new SQLiteConnection(connectionString);
  
    db.Open();
    
    SQLiteCommand c = new SQLiteCommand(db);
    c.CommandText = $"INSERT INTO Register_Requests VALUES (" +
    $"{registerRequest.Profile.Id}," +
    $"'{registerRequest.Profile.ProfileType.ToString()}'," +
    $"'{registerRequest.Profile.FirstName}'," +
    $"'{registerRequest.Profile.LastName}'," +
    $"'{registerRequest.Profile.Patronymic}'," +
    $"{registerRequest.Profile.Course}," +
    $"{registerRequest.Profile.Room}," +
    $"'{registerRequest.ImagePath}'" +
    $")";
    c.ExecuteNonQuery();
    db.Close();
  }

  public static async Task ChangeRegisterRequestField(long userId, string fieldName, object fieldValue)
  {
    string connectionString = $"Data Source={Program.ExecuteDBPath};Version=3;";
    using SQLiteConnection db = new SQLiteConnection(connectionString);
    string t;
    db.Open();
    
    if (fieldValue is string valueS)
    {
      t = $"'{valueS}'";
    }
    else if (fieldValue is int valueI)
    {
      t = $"{valueI}";      
    }
    else
    {
      t = $"{fieldValue}";
    }
    
    SQLiteCommand c = new SQLiteCommand(db);
    c.CommandText = $"UPDATE Register_Requests SET {fieldName} = {t} WHERE id = {userId}";
    c.ExecuteNonQuery();
    db.Close();
  }
  public static async Task DeleteRegisterRequest(long userId)
  {
    string connectionString = $"Data Source={Program.ExecuteDBPath};Version=3;";
    using SQLiteConnection db = new SQLiteConnection(connectionString);
    string path;
    db.Open();
    
    SQLiteCommand c = new SQLiteCommand(db);
    c.CommandText = $"SELECT image_path FROM Register_Requests WHERE id={userId}";
    path = (string)c.ExecuteScalar();
    c.CommandText = $"DELETE FROM Register_Requests WHERE id={userId}";
    c.ExecuteNonQuery();
    
    db.Close();
    if (File.Exists(path))
    {
      File.Delete(path);  
    }
  }
  public static async Task<RegisterRequestElement?> GetRegisterRequest(long userId)
  {
    string connectionString = $"Data Source={Program.ExecuteDBPath};Version=3;";
    using SQLiteConnection db = new SQLiteConnection(connectionString);
  
    db.Open();
    
    RegisterRequestElement registerRequest = null;
    
    SQLiteCommand c = new SQLiteCommand(db);
    c.CommandText = $"SELECT * FROM Register_Requests WHERE id={userId}";
    
    bool isAvaible = false;
    
    using (SQLiteDataReader r = c.ExecuteReader())
    {
      if (r.Read())
      {
        if (r.GetInt64(0) == userId)
        {
          isAvaible = true;
          registerRequest = new RegisterRequestElement();
          Profile profile = new Profile(r.GetInt64(0),
            Enum.Parse<ProfileType>(r.GetString(1)),
            r.GetString(2),
            r.GetString(3),
            r.GetString(4),
            r.GetByte(5),
            r.GetByte(6));
          registerRequest.Profile = profile;
          registerRequest.ImagePath = r.GetString(7).Replace('\\','/');
        }
      }
    }
    
    db.Close();
    return registerRequest;
  }
  public static async Task<Profile?> GetProfile(long userId)
  {
    string connectionString = $"Data Source={Program.ExecuteDBPath};Version=3;";
    using SQLiteConnection db = new SQLiteConnection(connectionString);
  
    db.Open();
    
    Profile profile = null;
    
    SQLiteCommand c = new SQLiteCommand(db);
    c.CommandText = $"SELECT * FROM Profiles WHERE id={userId}";
    
    bool isAvaible = false;
    
    using (SQLiteDataReader r = c.ExecuteReader())
    {
      if (r.Read())
      {
        if (r.GetInt64(0) == userId)
        {
          isAvaible = true;
          profile = new Profile(r.GetInt64(0),
            Enum.Parse<ProfileType>(r.GetString(1)),
            r.GetString(2),
            r.GetString(3),
            r.GetString(4),
            r.GetByte(5),
            r.GetByte(6));
        }
      }
    }
    
    db.Close();
    return profile;
  }
}