using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#nullable enable

namespace ValeraJesus;

class Core {
    TelegramBotClient bot;
    List<Person> clients;

    string HandleReply(string rep_mes, Message msg, out ReplyKeyboardMarkup? kbrd) {
        switch(rep_mes) {
            case "Назовите имя вашего хозяина обязательно ответом на это соо":
                string name = msg.Text ?? "";
                var host = clients.FirstOrDefault(c => c.Name == name);
                if(host != null) {
                    host.AddTrustedPerson(msg.From!.Id);
                    kbrd = null;
                    return $"Вы добавлены как доверенное лицо для {name}"; 
                } else {
                    kbrd = null;   
                    return "Нет такого хозяина. Попробуйте снова.";
                }
            case "Назовите своё имя. Обязательно ответом на это соо":
                name = msg.Text!;
                clients.Add(new Person(msg.From!.Id, name));                
                kbrd = new(
                    new KeyboardButton("Мне плохо! Спасите, помогите!")
                ) { ResizeKeyboard = true };
                return "Вы зарегистрированы как хост";
            default:
                kbrd = null;
                return "";
        }
    }
    async Task HandleTGUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        await bot.GetUpdates();
        ReplyKeyboardMarkup? keyboard = null;
        var chat_id = update.Message!.Chat.Id;
        string message = "Непонятное что-то вы мне написали, давайте по-русски";;
        switch(update.Message.Text) {
            case "/start":
                message = "Пожалуйста, выберите свою роль.";
                keyboard = new(
                    new KeyboardButton("Я - хост"),
                    new KeyboardButton("Я - доверенное лицо")
                ) { ResizeKeyboard = true };
                break;
            case "Я - доверенное лицо":
                message = clients.Any(c => c.TrustedPersonsIDs.Contains(chat_id))
                    ? "Вы уже доверенное лицо и о ком-то заботитесь"
                    : "Назовите имя вашего хозяина обязательно ответом на это соо";
                    keyboard = null;
                break;
            case "Я - хост":
                message = clients.Any(c => c.PersonID == chat_id) 
                    ? "Вы уже хост и о вас кто-то заботится."
                    : "Назовите своё имя. Обязательно ответом на это соо";
                break;
            case "Мне плохо! Спасите, помогите!":
                foreach(var trusteds in clients.Select(c => c.TrustedPersonsIDs))
                    foreach(var id in trusteds)
                        await bot.SendMessage(id, "Вашему хозяину плохо. Насыпьте ему вискаса");
                message = "Ок. Попробуем позвать на помощь";
                break;
            default:
                if(update.Message!.ReplyToMessage is {} repliedMes && repliedMes!.From!.Id == botClient.BotId)
                    message = HandleReply(repliedMes.Text!, update.Message, out keyboard);
                if(update.Message.Photo != null && clients.Any(c => c.PersonID == chat_id)) {
                    var photo = await botClient.GetFile(update.Message!.Photo![update.Message!.Photo.Count() - 1].FileId);
                    foreach(var id in clients.First(c => c.PersonID == chat_id).TrustedPersonsIDs)
                        await bot.SendPhoto(id, photo, "Похоже, вашему хосту не хорошо. Может, вискаса ему?");
                    message = "Ок. Попробуем позвать на помощь";
                }
                break;
        }
        await bot.SendMessage(
            chatId: chat_id,
            text: message,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
    Task HandleTGError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
        var errorMessage = exception switch {
            ApiRequestException apiRequestException => $"Ошибка API: {apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    public void Work() {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions {
            AllowedUpdates = [UpdateType.Message]
        };
        bot.StartReceiving(HandleTGUpdate, HandleTGError, receiverOptions, cts.Token);
    }
    public Core() {
        bot = new("7487805129:AAEqQAgR4qdT84_71x0FOr0UwyP1CHYRnXI");
        clients = new();
    }
}