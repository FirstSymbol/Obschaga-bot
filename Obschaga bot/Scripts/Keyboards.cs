using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Obschaga_bot.Scripts;

public static class Keyboards
{
  
  // callbackParams:
  // separator is _
  // operation-type_operation-object_additions_profile-type
  // Example:
  // open_register_form_0
  // open_menu_1
  
  
  
  public static class Callbacks
  {
    public static async Task<string> OpenMenu(long userId, ProfileType profileType) => 
      $"open_menu_{profileType}";

    public static async Task<string> StartRegisterRequest() => 
      $"open_register_start";

    public static async Task<string> DeleteRegisterRequest() =>
      $"db_register_delete";

    public static async Task<string> CancelAction() =>
      $"cancel_action";
    
    public static async Task<string> OpenViewRegisterRequest() =>
      $"open_register_view";
    public static async Task<string> OpenChangeRegisterRequest() =>
      $"open_register_change";
    public static async Task<string> CancelRegisterRequest() =>
      $"cancel_register";
    
    public static async Task<string> OpenAdminPanel(long userId, ProfileType profileType) =>
      $"open_admin_panel_{profileType}";
    
    public static async Task<string> OpenAdminRegisterRequests(ProfileType profileType) =>
      $"open_admin_requests_register_{profileType}";
    
    public static async Task<string> OpenProfilePage(long userId, ProfileType profileType) =>
      $"open_profile_page_{profileType}";

  }
  
  public static class Defaults
  {
    
  }

  public static class Inlines
  {
    public static async Task<InlineKeyboardButton> OpenAdminRegisterRequests(ProfileType profileType, string title = "Заявки на регистрацию") => 
      new(title, await Callbacks.OpenAdminRegisterRequests(profileType));
    public static async Task<InlineKeyboardButton> OpenProfilePage(long userId, ProfileType profileType, string title = "Профиль") =>
      new(title, await Callbacks.OpenProfilePage(userId, profileType));

    public static async Task<InlineKeyboardButton> OpenAdminPanel(long userId, ProfileType profileType, string title = "Панель администратора") =>
      new(title, await Callbacks.OpenAdminPanel(userId, profileType));
    
    public static async Task<InlineKeyboardButton> OpenMenu(long userId, ProfileType profileType, string title = "Меню") => 
      new(title, await Callbacks.OpenMenu(userId, profileType));

    public static async Task<InlineKeyboardButton> SendRegisterRequest(string title = "Зарегистрироваться") =>
      new InlineKeyboardButton(title, await Callbacks.StartRegisterRequest());
    
    public static async Task<InlineKeyboardButton> DeleteRegisterRequest(string title = "Удалить заявку") =>
      new InlineKeyboardButton(title, await Callbacks.DeleteRegisterRequest());

    public static async Task<InlineKeyboardButton> CancelAction(string title = "Отмена") =>
      new InlineKeyboardButton(title, await Callbacks.CancelAction());

    public static async Task<InlineKeyboardButton> OpenViewRegisterRequest(string title = "Просмотреть запись") =>
      new InlineKeyboardButton(title, await Callbacks.OpenViewRegisterRequest());
    
    public static async Task<InlineKeyboardButton> OpenChangeRegisterRequest(string title = "Редактировать") =>
      new InlineKeyboardButton(title, await Callbacks.OpenChangeRegisterRequest());
    
    public static async Task<InlineKeyboardButton> CancelRegisterRequest(string title = "Отмена") =>
      new InlineKeyboardButton(title, await Callbacks.CancelRegisterRequest());
    
    
  }

  public static class Markups
  {
    public static async Task<InlineKeyboardMarkup> OpenAdminPanel(long userId, ProfileType profileType)
    {
      InlineKeyboardMarkup markup = new();
      markup.AddButton(await Inlines.OpenAdminRegisterRequests(profileType));
      markup.AddNewRow(await Inlines.OpenMenu(userId, profileType));
      return markup;
    }
    public static async Task<InlineKeyboardMarkup> OpenMenu(long userId, ProfileType profileType)
    {
      InlineKeyboardMarkup markup = null;
      
      if (profileType == ProfileType.Empty){}
      else
      {
        markup = new InlineKeyboardMarkup();
        markup.AddNewRow(await Inlines.OpenProfilePage(userId, profileType));
        
        switch (profileType)
        {
          case ProfileType.Admin:
          {
            markup.AddNewRow(await Inlines.OpenAdminPanel(userId, (ProfileType)profileType));
            break;
          }
        }
      }
      
      return markup;
    }
    public static async Task<InlineKeyboardMarkup> SendRegisterRequest(long userId)
    {
      Task<bool> isRegisterRequestExist = Controllers.BoolOperations.IsRegisterRequestExist(userId);
      await isRegisterRequestExist;
      InlineKeyboardMarkup markup = new InlineKeyboardMarkup();
      if (isRegisterRequestExist.Result)
      {
        markup.AddButtons(await Inlines.OpenViewRegisterRequest(), await Inlines.DeleteRegisterRequest());
        return markup;
      }
      else
      {
        markup.AddButton(await Inlines.SendRegisterRequest());
        return markup;
      }
    }
  }
}