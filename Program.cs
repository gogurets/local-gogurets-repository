using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient botClient;
    private static long chatId;
    private static int currentMileage;
    private static string engineType;
    private static Dictionary<string, Dictionary<string, int>> partMileageThresholds = MileageThresholds.PartMileageThresholds;
    private static Dictionary<string, Dictionary<string, int>> partCounters = Parts.PartCounters;

    static void Main(string[] args)
    {
        string token = "6647436556:AAEZuhGBXfEI4tE-ftwyN_0DywS00J69mn8";
        botClient = new TelegramBotClient(token);
        botClient.OnMessage += Bot_OnMessage;

        foreach (var engine in partMileageThresholds.Keys)
        {
            partCounters[engine] = new Dictionary<string, int>();
            foreach (var part in partMileageThresholds[engine].Keys)
            {
                partCounters[engine][part] = 0;
            }
        }

        botClient.StartReceiving();

        Console.WriteLine("Bot started. Press Enter to exit.");
        Console.ReadLine();

        botClient.StopReceiving();
    }

    private static void Bot_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Message.Text != null)
        {
            chatId = e.Message.Chat.Id;
            var message = e.Message.Text;

            if (string.IsNullOrEmpty(engineType))
            {
                if (message == "/start")
                {
                    var keyboard = GetEngineTypeKeyboard();
                    botClient.SendTextMessageAsync(chatId, "Выберите тип двигателя:", replyMarkup: keyboard);
                }
                else if (partMileageThresholds.ContainsKey(message))
                {
                    engineType = message;
                    botClient.SendTextMessageAsync(chatId, $"Вы выбрали тип двигателя: {engineType}. Пожалуйста, введите текущий пробег в километрах:");
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
                    HandleMileageUpdate(chatId, mileage);
                }
                else
                {
                    botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите корректное значение пробега в километрах.");
                }
            }
        }
    }

    private static void HandleMileageUpdate(long chatId, int newMileage)
    {
        if (newMileage < currentMileage)
        {
            botClient.SendTextMessageAsync(chatId, "Ошибка: введенный пробег меньше предыдущего значения.");
            return;
        }

        var advice = GetAdvice(engineType, newMileage);

        currentMileage = newMileage;

        botClient.SendTextMessageAsync(chatId, advice);
    }

    private static ReplyKeyboardMarkup GetEngineTypeKeyboard()
    {
        var engineTypes = partMileageThresholds.Keys.ToArray();
        var keyboardButtons = engineTypes.Select(et => new KeyboardButton(et)).ToArray();
        return new ReplyKeyboardMarkup(keyboardButtons, true, true);
    }

    private static string GetAdvice(string engineType, int mileage)
    {
        if (!partMileageThresholds.ContainsKey(engineType))
        {
            return "Ошибка: некорректный тип двигателя.";
        }

        var advice = new List<string>();
        var mileageThresholds = partMileageThresholds[engineType];

        foreach (var part in mileageThresholds.Keys)
        {
            if (!partCounters[engineType].ContainsKey(part))
            {
                partCounters[engineType][part] = 0;
            }

            partCounters[engineType][part] += mileage - currentMileage;

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
