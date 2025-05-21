// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using DotNetEnv;
using Obschaga_bot.Scripts;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Obschaga_bot;

internal abstract class Program
{
  public static readonly Dictionary<long, Func<Message,Task>> PendingActions = new(); 
  public static readonly Dictionary<long, RegisterRequestElement> ProfilesRequests = new();
  public static TelegramBotClient Bot;
  public static string ExecuteLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
  public static string BotToken;
  public static string ExecuteDBPath;
  public static string MediaPath;
  public static async Task Main(string[] args)
  {
    Console.WriteLine(ExecuteLocation);
    Env.Load($"{ExecuteLocation}/../bot.env");
    
    BotToken = Env.GetString("TELEGRAM_BOT_TOKEN");
    ExecuteDBPath = ExecuteLocation + '/' + Env.GetString("DATABASE_PATH");
    MediaPath = Env.GetString("MEDIA_DIRECTORY_PATH");
    using var cts = new CancellationTokenSource();
    Bot = new TelegramBotClient(BotToken, cancellationToken: cts.Token);
    var me = await Bot.GetMe();
    Bot.OnError += OnError;
    Bot.OnMessage += OnMessage;
    Bot.OnUpdate += OnUpdate;

    Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");

    Console.ReadLine();
    cts.Cancel();

    async Task OnMessage(Message msg, UpdateType type)
    {
      if (PendingActions.ContainsKey(msg.Chat.Id))
      {
        await Controllers.ExecuteResponseMessage(msg);
      }
      else
      {
        var isProfileExist = Controllers.BoolOperations.IsProfileExist(msg.From!.Id);
        await isProfileExist;
      
        if (isProfileExist.Result)
        {
          await Controllers.General.OpenMenu(msg.Chat.Id, msg.From.Id, await Db.GetProfileType(msg.From.Id));
        }
        else
        {
          await Controllers.Registration.OpenRegisterWindow(msg.Chat.Id, msg.From.Id);
        }
      }
      
  
    }

    async Task OnUpdate(Update update)
    {
      switch (update.Type)
      {
        case UpdateType.CallbackQuery:
          await OnCallbackQuery(update.CallbackQuery!);
          break;
      }
    }

    async Task OnError(Exception exception, HandleErrorSource source)
    {
      if (exception is Telegram.Bot.Exceptions.RequestException)
        Console.WriteLine("Ошибка запроса.");
      else if (exception is Telegram.Bot.Exceptions.ApiRequestException)
        Console.WriteLine("Ошибка API запроса.");
      else
        Console.WriteLine(exception);
    }

    #region Handlers

    async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
      string[] queryParams = callbackQuery.Data.Split('_');
      
      long userId = callbackQuery.From.Id;
      long chatId = callbackQuery.Message!.Chat.Id;
      
      ProfileType profileType = ProfileType.Empty;
      if (Enum.TryParse<ProfileType>(queryParams[^1], out ProfileType pr)) profileType = pr;

      switch (queryParams[0])
      {
        case "open":
        {
          switch (queryParams[1])
          {
            case "register":
            {
              switch (queryParams[2])
              {
                case "start": await Controllers.Registration.RegisterStart(chatId, userId); break;
                case "view": await Controllers.Registration.OpenRegisterRequestView(chatId, userId); break;
                case "change": await Controllers.Registration.StartChangeRequestField(chatId, userId); break;
              }
             break; 
            }
            case "menu": await Controllers.General.OpenMenu(chatId, userId, profileType); break;
            case "profile":
            {
              if (queryParams[2] == "page")
              {
                Console.WriteLine(profileType);
                await Controllers.General.OpenProfilePage(chatId, userId, profileType);
              }
              break;
            }
            case "admin":
            {
              switch (queryParams[2])
              {
                case "requests":
                  if (queryParams[3] == "register")
                    await Controllers.General.OpenAdminRequestsRegister(chatId, userId, profileType);
                  break;
                case "panel":
                  await Controllers.General.OpenAdminPanel(chatId, userId, profileType);
                  break;
              }
              break;
            }
          }
          break;
        }
        case "cancel":
        {
          switch (queryParams[1])
          {
            case "action": await Controllers.General.CancelAction(chatId, userId); break;
            case "register": await Controllers.General.CancelAction(chatId, userId); break;
          }
          break;
        }
        case "db":
        {
          switch (queryParams[1])
          {
            case "register":
            {
              switch (queryParams[2])
              {
                case "delete": await Controllers.Registration.DeleteRegisterRequest(chatId, userId); break;
              }
              break;
            }
            
          }
          break;
        }
      }
    }

    #endregion
  }

  private static async Task SendInDevelopment(CallbackQuery callbackQuery)
  {
    await Bot.SendMessage(callbackQuery.Message!.Chat.Id, "БОТ НАХОДИТСЯ В РАЗРАБОТКЕ");
  }
}



