using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient botClient;
    private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
    private static Dictionary<string, Dictionary<string, int>> partMileageThresholds = MileageThresholds.PartMileageThresholds;
    private static Dictionary<string, Dictionary<string, int>> partCounters = Parts.PartCounters;

    class UserState
    {
        public int currentMileage;
        public string engineType;
    }

    static void Main(string[] args)
    {
        string token = "6647436556:AAEZuhGBXfEI4tE-ftwyN_0DywS00J69mn8";
        botClient = new TelegramBotClient(token);
        botClient.OnMessage += Bot_OnMessage;

        botClient.StartReceiving();

        Console.WriteLine("Bot started. Press Enter to exit.");
        Console.ReadLine();

        botClient.StopReceiving();
    }

    private static void Bot_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Message.Text != null)
        {
            var chatId = e.Message.Chat.Id;
            var message = e.Message.Text;

            if (!userStates.ContainsKey(chatId))
            {
                userStates[chatId] = new UserState();
            }
            var userState = userStates[chatId];

            if (message == "/start" || message == "/restart")
            {
                userState.engineType = null;
                userState.currentMileage = 0;
                var keyboard = GetEngineTypeKeyboard();
                botClient.SendTextMessageAsync(chatId, "Выберите тип двигателя:", replyMarkup: keyboard);
            }
            else if (string.IsNullOrEmpty(userState.engineType))
            {
                if (partMileageThresholds.ContainsKey(message))
                {
                    userState.engineType = message;
                    botClient.SendTextMessageAsync(chatId, $"Вы выбрали тип двигателя: {userState.engineType}. Пожалуйста, введите текущий пробег в километрах:");
                }
                else
                {
                    botClient.SendTextMessageAsync(chatId, "Для начала работы, пожалуйста, введите команду /start.");
                }
            }
            else
            {
                if (int.TryParse(message, out int mileage))
                {
                    HandleMileageUpdate(chatId, userState, mileage);
                }
                else
                {
                    botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите корректное значение пробега в километрах.");
                }
            }
        }
    }

    private static void HandleMileageUpdate(long chatId, UserState userState, int mileage)
    {
        if (mileage < userState.currentMileage)
        {
            botClient.SendTextMessageAsync(chatId, "Ошибка: введенный пробег меньше предыдущего значения.");
            return;
        }

        var advice = GetAdvice(userState.engineType, mileage, userState.currentMileage);

        userState.currentMileage = mileage;

        botClient.SendTextMessageAsync(chatId, advice);
    }

    private static ReplyKeyboardMarkup GetEngineTypeKeyboard()
    {
        var engineTypes = partMileageThresholds.Keys.ToArray();
        var keyboardButtons = engineTypes.Select(et => new KeyboardButton(et)).ToArray();
        return new ReplyKeyboardMarkup(keyboardButtons, true, true);
    }


    private static string GetAdvice(string engineType, int newMileage, int currentMileage)
    {
        if (!partMileageThresholds.ContainsKey(engineType))
        {
            return "Ошибка: некорректный тип двигателя.";
        }

        var advice = new List<string>();
        var mileageThresholds = partMileageThresholds[engineType];

        foreach (var part in mileageThresholds.Keys)
        {
            if (!partCounters.ContainsKey(engineType))
            {
                partCounters[engineType] = new Dictionary<string, int>();
            }

            if (!partCounters[engineType].ContainsKey(part))
            {
                partCounters[engineType][part] = 0;
            }

            partCounters[engineType][part] += newMileage - currentMileage;

            if (partCounters[engineType][part] >= mileageThresholds[part])
            {
                advice.Add($"Советуется {part}");
                partCounters[engineType][part] = 0;
            }
        }

        if (advice.Count == 0)
        {
            return "Советы не требуются на данный момент.";
        }

        return string.Join("\n", advice);
    }

}

