using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Obschaga_bot.Scripts;

public static class Controllers
{
  public static class General
  {
    public static async Task CancelAction(long chatId, long userId)
    {
      Program.PendingActions.Remove(chatId);
      Program.ProfilesRequests.Remove(chatId);
      await Program.Bot.SendMessage(
        chatId: chatId,
        text: "Все действия были отменены",
        replyMarkup: new InlineKeyboardMarkup(await Keyboards.Inlines.OpenMenu(userId))
      );
    }
    public static async Task OpenMenu(long chatId, long userId, ProfileType profileType)
    {
      switch (profileType)
      {
        case ProfileType.Empty: await Registration.OpenRegisterWindow(chatId, userId); break;
        case ProfileType.User: break;
      }
    }
  }
  public static class DownloadAndSave
  {
    public static async Task SinglePhoto(Message msg, string folderPath, string fileName)
    {
      if (Directory.Exists(folderPath) == false)
      {
        var t = Directory.CreateDirectory(folderPath);
      }

      var fileId = msg.Photo![^1].FileId;
      var tgFile = await Program.Bot.GetFile(fileId);
      await using var stream = File.Create(folderPath + '/' + fileName);
      await Program.Bot.DownloadFile(tgFile, stream);
    }
  }

  public static class Registration
  {
    public static async Task OpenRegisterWindow(long chatId, long userId)
    {
      var isRegisterRequestExist = BoolOperations.IsRegisterRequestExist(userId);
      await isRegisterRequestExist;
      if (isRegisterRequestExist.Result)
      {
        var text = "Ваша заявка на регистрацию уже отправлена на проверку.\n";

        await Program.Bot.SendMessage(
          chatId,
          text,
          ParseMode.Html,
          replyMarkup: await Keyboards.Markups.SendRegisterRequest(userId)
        );
      }
      else
      {
        var text = "\u274c На данный момент вашего аккаунта нет в базе!\n\n" +
                   "Для того, чтобы зарегистрироваться в базе вам надо нажать кнопку <b>Зарегистрироваться</b>, прикрепленную к данному сообщению, после чего вам надо будет поочередно ввести запрашиваемые данные ботом, а именно:\n\n" +
                   "- Фамилия\n" +
                   "- Имя\n" +
                   "- Отчество\n" +
                   "- Номер комнаты\n" +
                   "- Курс\n" +
                   "- Фото разворота пропуска в общежитие(для подтверждения личности)\n\n" +
                   "После этого создаться заявка, которую обработает администратор. После того, как администратор одобрит заявку, вас добавят в базу и вы сможете пользоваться ботом.";

        await Program.Bot.SendMessage(
          chatId,
          text,
          ParseMode.Html,
          replyMarkup: await Keyboards.Inlines.SendRegisterRequest()
        );
      }
    }

    public static async Task RegisterStart(long chatId, long userId)
    {
      if (await BoolOperations.IsRegisterRequestExist(userId) || 
          Program.ProfilesRequests.ContainsKey(chatId)) return;
      
      Program.ProfilesRequests.Add(chatId, new RegisterRequestElement());
      Program.ProfilesRequests[chatId].Profile.ProfileType = ProfileType.User;
      Program.ProfilesRequests[chatId].Profile.Id = userId;
      await RegisterStep1(chatId);
    }
    public static async Task RegisterStep1(long chatId)
    {
      await EnterFirstName(chatId, RegisterStep2,
        new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
    }

    private static async Task EnterFirstName(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте своё имя:",
        replyMarkup: markup
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep2(Message msg)
    {
      Program.ProfilesRequests[msg.Chat.Id].Profile.FirstName = msg.Text!.Trim();

      await EnterLastName(msg.Chat.Id, RegisterStep3,
        new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
    }

    private static async Task EnterLastName(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте свою фамилию:",
        replyMarkup: markup
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep3(Message msg)
    {
      Program.ProfilesRequests[msg.Chat.Id].Profile.LastName = msg.Text!.Trim();

      await EnterPatronymic(msg.Chat.Id, RegisterStep4,
        new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
    }

    private static async Task EnterPatronymic(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте своё отчество:",
        replyMarkup: markup
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep4(Message msg)
    {
      Program.ProfilesRequests[msg.Chat.Id].Profile.Patronymic = msg.Text!.Trim();

      await EnterCourseNumber(msg.Chat.Id, RegisterStep5,
        new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
    }

    private static async Task EnterCourseNumber(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте номер вашего курса:",
        replyMarkup: markup
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep5(Message msg)
    {
      if (byte.TryParse(msg.Text, out var course) && course is >= 1 and <= 4)
      {
        Program.ProfilesRequests[msg.Chat.Id].Profile.Course = course;

        await EnterRoomNumber(msg.Chat.Id, RegisterStep6,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова."
        );
        await EnterCourseNumber(msg.Chat.Id, RegisterStep5,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
      }
    }

    private static async Task EnterRoomNumber(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте номер своей комнаты:",
        replyMarkup: markup
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep6(Message msg)
    {
      if (byte.TryParse(msg.Text, out var room) && room is >= 34 and <= 158)
      {
        Program.ProfilesRequests[msg.Chat.Id].Profile.Room = room;

        await EnterPhoto(msg.Chat.Id, RegisterStep7,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова.\n"
        );
        await EnterRoomNumber(msg.Chat.Id, RegisterStep6,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
      }
    }

    private static async Task EnterPhoto(long chatId, Func<Message, Task> func, InlineKeyboardMarkup markup)
    {
      await Program.Bot.SendMessage(chatId,
        "Отправьте фото разворота пропуска в общежитие:",
        replyMarkup: new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest())
      );
      await AddResponseMessage(chatId, func);
    }

    public static async Task RegisterStep7(Message msg)
    {
      if (msg.Type == MessageType.Photo)
      {
        var folderPath = $"{Program.MediaPath}/Register/Images";
        var fileName = $"Image_{msg.From!.Id}.jpg";
        await DownloadAndSave.SinglePhoto(msg, folderPath, fileName);
        Program.ProfilesRequests[msg.Chat.Id].ImagePath = folderPath + '/' + fileName;
        await RegisterFinish(msg.Chat.Id, msg.From.Id);
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова.\n" +
          "Фото надо отправить в формате фото, а не документа.\n"
        );
        await EnterPhoto(msg.Chat.Id, RegisterStep7,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelRegisterRequest()));
      }
    }

    public static async Task RegisterFinish(long chatId, long userId)
    {
      await Db.AddRegisterRequest(Program.ProfilesRequests[chatId]);
      InlineKeyboardMarkup markup = await Keyboards.Markups.SendRegisterRequest(userId);
      markup.AddNewRow(await Keyboards.Inlines.OpenMenu(userId));
      await Program.Bot.SendMessage(
        chatId,
        "Ваш запрос на регистрацию был отправлен на проверку. Ожидайте решения.",
        replyMarkup: markup
      );
      Console.WriteLine($"{userId} создал заявку!");
      Program.ProfilesRequests.Remove(chatId);
    }

    public static async Task DeleteRegisterRequest(long chatId, long userId)
    {
      if (await BoolOperations.IsRegisterRequestExist(userId))
      {
        await Db.DeleteRegisterRequest(userId);
        await Program.Bot.SendMessage(chatId,
          "Ваша заявка была удалена!",
          replyMarkup: await Keyboards.Inlines.OpenMenu(userId)
        );
        Console.WriteLine($"{userId} удалил заявку!");
        await OpenRegisterWindow(chatId, userId);
      }
    }

    public static async Task OpenRegisterRequestView(long chatId, long userId)
    {
      var t = await Db.GetRegisterRequest(userId);
      var text = $"Данные вашей заявки:\n" +
                 $"[1] Имя: {t.Profile.FirstName}\n" +
                 $"[2] Фамилия: {t.Profile.LastName}\n" +
                 $"[3] Отчество: {t.Profile.Patronymic}\n" +
                 $"[4] Номер курса: {t.Profile.Course}\n" +
                 $"[5] Номер комнаты: {t.Profile.Room}\n" +
                 $"[6] Фото разворота\n";

      var markup = new InlineKeyboardMarkup(
        await Keyboards.Inlines.OpenChangeRegisterRequest());
      markup.AddNewRow(await Keyboards.Inlines.OpenMenu(userId));
      var tPath = Program.ExecuteLocation + '/' + t.ImagePath;
      if (File.Exists(tPath))
      {
        await using Stream stream = File.OpenRead(tPath);

        await Program.Bot.SendPhoto(
          chatId,
          stream,
          text,
          replyMarkup: markup
        );
      }
      else
      {
        await Program.Bot.SendMessage(
          chatId,
          text + ": ОТСУТСТВУЕТ",
          replyMarkup: markup
        );
      }
    }

    public static async Task StartChangeRequestField(long chatId, long userId)
    {
      await Program.Bot.SendMessage(
        chatId,
        "Введите номер параметра, который вы хотите изменить:",
        replyMarkup: new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction())
      );
      await AddResponseMessage(chatId, ChangeRequestField);
    }

    public static async Task ChangeRequestField(Message msg)
    {
      if (byte.TryParse(msg.Text, out var number) && number is >= 1 and <= 6)
      {
        switch (number)
        {
          case 1:
            await EnterFirstName(msg.Chat.Id, ChangeRequestField1,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
          case 2:
            await EnterLastName(msg.Chat.Id, ChangeRequestField2,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
          case 3:
            await EnterPatronymic(msg.Chat.Id, ChangeRequestField3,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
          case 4:
            await EnterCourseNumber(msg.Chat.Id, ChangeRequestField4,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
          case 5:
            await EnterRoomNumber(msg.Chat.Id, ChangeRequestField5,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
          case 6:
            await EnterPhoto(msg.Chat.Id, ChangeRequestField6,
              new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
            break;
        }
      }
      else
      {
        await Program.Bot.SendMessage(
          msg.Chat.Id,
          "Введены некорректные данные. Попробуйте снова.:"
        );
        await StartChangeRequestField(msg.Chat.Id, msg.From!.Id);
      }
    }

    public static async Task ChangeRequestField1(Message msg)
    {
      await Db.ChangeRegisterRequestField(msg.From!.Id, "first_name", msg.Text!);
      await Program.Bot.SendMessage(msg.Chat.Id, "Ваше имя было успешно изменено");
      await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
    }

    public static async Task ChangeRequestField2(Message msg)
    {
      await Db.ChangeRegisterRequestField(msg.From!.Id, "last_name", msg.Text!);
      await Program.Bot.SendMessage(msg.Chat.Id, "Ваша фамилия была успешно изменена");
      await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
    }

    public static async Task ChangeRequestField3(Message msg)
    {
      await Db.ChangeRegisterRequestField(msg.From!.Id, "patronymic", msg.Text!);
      await Program.Bot.SendMessage(msg.Chat.Id, "Ваше отчество было успешно изменено");
      await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
    }

    public static async Task ChangeRequestField4(Message msg)
    {
      if (byte.TryParse(msg.Text, out var course) && course is >= 1 and <= 4)
      {
        await Db.ChangeRegisterRequestField(msg.From!.Id, "course", course);
        await Program.Bot.SendMessage(msg.Chat.Id, "Ваш номер курса был успешно изменен");
        await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова."
        );
        await EnterCourseNumber(msg.Chat.Id, ChangeRequestField4,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
      }
    }

    public static async Task ChangeRequestField5(Message msg)
    {
      if (byte.TryParse(msg.Text, out var room) && room is >= 34 and <= 158)
      {
        await Db.ChangeRegisterRequestField(msg.From!.Id, "room", room);
        await Program.Bot.SendMessage(msg.Chat.Id, "Ваш номер комнаты был успешно изменен");
        await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова."
        );
        await EnterCourseNumber(msg.Chat.Id, ChangeRequestField4,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
      }
    }

    public static async Task ChangeRequestField6(Message msg)
    {
      if (msg.Type == MessageType.Photo)
      {
        var folderPath = $"{Program.MediaPath}/Register/Images";
        var fileName = $"Image_{msg.From!.Id}.jpg";
        await DownloadAndSave.SinglePhoto(msg, folderPath, fileName);

        await Db.ChangeRegisterRequestField(msg.From.Id, "image_path", folderPath + '/' + fileName);
        await Program.Bot.SendMessage(msg.Chat.Id, "Ваше фото пропуска было успешно изменено");
        await OpenRegisterRequestView(msg.Chat.Id, msg.From.Id);
      }
      else
      {
        await Program.Bot.SendMessage(msg.Chat.Id,
          "Данные введены некорректно - Попробуйте снова.\n" +
          "Фото надо отправить в формате фото, а не документа.\n"
        );
        await EnterPhoto(msg.Chat.Id, ChangeRequestField6,
          new InlineKeyboardMarkup(await Keyboards.Inlines.CancelAction()));
      }
    }
  }


  public static class BoolOperations
  {
    public static async Task<bool> IsProfileExist(long userId) => 
      await Db.GetProfile(userId) != null;

    public static async Task<bool> IsRegisterRequestExist(long userId) => 
      await Db.GetRegisterRequest(userId) != null;
  }
  
  
  public static async Task ChangeResponseMessage(long chatId, Func<Message, Task> action) => 
    Program.PendingActions[chatId] = action;

  public static async Task AddResponseMessage(long chatId, Func<Message, Task> action)
  {
    if (Program.PendingActions.ContainsKey(chatId))
      await ChangeResponseMessage(chatId, action);
    else
      Program.PendingActions.Add(chatId, action);
  }

  public static async Task RemoveResponseMessage(long chatId)
  {
    if (Program.PendingActions.ContainsKey(chatId))
      Program.PendingActions.Remove(chatId);
  }

  public static async Task ExecuteResponseMessage(Message msg)
  {
    if (Program.PendingActions.TryGetValue(msg.Chat.Id, out var handler))
    {
      await RemoveResponseMessage(msg.Chat.Id);
      await handler(msg);
    }
  }
}