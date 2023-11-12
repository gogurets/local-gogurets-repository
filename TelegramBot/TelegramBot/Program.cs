using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient botClient;
    private static Dictionary<long, ChatData> chatDataDictionary = new Dictionary<long, ChatData>();
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
        string token = "YOUR_BOT_TOKEN";
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

            if (!chatDataDictionary.ContainsKey(chatId))
            {
                chatDataDictionary[chatId] = new ChatData();
            }

            var chatData = chatDataDictionary[chatId];

            if (string.IsNullOrEmpty(chatData.EngineType))
            {
                if (message == "/start")
                {
                    var keyboard = GetEngineTypeKeyboard();
                    botClient.SendTextMessageAsync(chatId, "Выберите тип двигателя:", replyMarkup: keyboard);
                }
                else if (partMileageThresholds.ContainsKey(message))
                {
                    chatData.EngineType = message;
                    botClient.SendTextMessageAsync(chatId, $"Вы выбрали тип двигателя: {chatData.EngineType}. Пожалуйста, введите текущий пробег в километрах:");
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
                    Task.Run(() => HandleMileageUpdate(chatId, mileage, chatData));
                }
                else
                {
                    botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите корректное значение пробега в километрах.");
                }
            }
        }
    }

    private static void HandleMileageUpdate(long chatId, int newMileage, ChatData chatData)
    {
        if (newMileage < chatData.CurrentMileage)
        {
            botClient.SendTextMessageAsync(chatId, "Ошибка: введенный пробег меньше предыдущего значения.");
            return;
        }

        var advice = GetAdvice(chatData.EngineType, newMileage, chatData);

        chatData.CurrentMileage = newMileage;

        botClient.SendTextMessageAsync(chatId, advice);
    }

    private static ReplyKeyboardMarkup GetEngineTypeKeyboard()
    {
        var engineTypes = partMileageThresholds.Keys.ToArray();
        var keyboardButtons = engineTypes.Select(et => new KeyboardButton(et)).ToArray();
        return new ReplyKeyboardMarkup(keyboardButtons, true, true);
    }

    private static string GetAdvice(string engineType, int mileage, ChatData chatData)
    {
        if (!partMileageThresholds.ContainsKey(engineType))
        {
            return "Ошибка: некорректный тип двигателя.";
        }

        var advice = new List<string>();
        var mileageThresholds = partMileageThresholds[engineType];

        foreach (var part in mileageThresholds.Keys)
        {
            if (!chatData.PartCounters.ContainsKey(part))
            {
                chatData.PartCounters[part] = 0;
            }

            chatData.PartCounters[part] += mileage - chatData.CurrentMileage;

            if (chatData.PartCounters[part] >= mileageThresholds[part])
            {
                advice.Add($"Советуется {part}");
                chatData.PartCounters[part] = 0;
            }
        }

        if (advice.Count == 0)
        {
            return "Советы не требуются на данный момент.";
        }

        return string.Join("\n", advice);
    }

    private class ChatData
    {
        public string EngineType { get; set; }
        public int CurrentMileage { get; set; }
        public Dictionary<string, int> PartCounters { get; } = new Dictionary<string, int>();
    }
}
