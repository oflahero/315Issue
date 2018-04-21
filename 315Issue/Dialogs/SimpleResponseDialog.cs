using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using _315Issue.Assets;

namespace _315Issue.Dialogs
{
    public class SimpleResponseDialog : IDialog<object>
    {
        private readonly string _messageToSend;

        public SimpleResponseDialog(string message)
        {
            _messageToSend = message;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var respMessage = context.MakeMessage();
            respMessage.Text = _messageToSend;
            await context.PostWithFacebookNotificationAsync(respMessage);
            context.Done<object>(null);
        }
    }
}
