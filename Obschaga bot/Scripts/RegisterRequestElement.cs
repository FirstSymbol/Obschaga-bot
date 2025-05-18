namespace Obschaga_bot.Scripts;

public class RegisterRequestElement
{
  public Profile Profile;
  public string? ImagePath = null;

  public RegisterRequestElement()
  {
    Profile = new Profile();
  }
}