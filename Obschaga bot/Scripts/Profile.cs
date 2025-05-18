namespace Obschaga_bot.Scripts;

public class Profile
{
  public long? Id;
  public ProfileType? ProfileType;
  public string? FirstName;
  public string? LastName;
  public string? Patronymic;
  public byte? Course;
  public byte? Room;

  public Profile() { }

  public Profile(long id, ProfileType profileType, string firstName, string lastName, string patronymic, byte course, byte room)
  {
    Id = id;
    ProfileType = profileType;
    FirstName = firstName;
    LastName = lastName;
    Patronymic = patronymic;
    Course = course;
    Room = room;
  }

  public bool IsFilled()
  {
    return Id is not null &&
           ProfileType is not null &&
           FirstName is not null &&
           LastName is not null &&
           Patronymic is not null &&
           Course is not null &&
           Room is not null;
  }
}