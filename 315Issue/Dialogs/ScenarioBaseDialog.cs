
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _315Issue.Assets;

namespace _315Issue.Dialogs
{
    [Serializable]
    public abstract class DialogWithHelpers<T> : IDialog<T>
    {
        public abstract Task StartAsync(IDialogContext context);

        protected async Task QuitWithAsync(IDialogContext context, string sMsg, bool quitSuccess = true)
        {
            if (!string.IsNullOrEmpty(sMsg))
            {
                var msg = context.MakeMessage();
                msg.Text = sMsg;
                await context.PostWithFacebookNotificationAsync(msg);
            }

            context.Done(quitSuccess);
        }

        protected async Task MessageAndContinueAsync(IDialogContext context, string sMsg)
        {
            if (!string.IsNullOrEmpty(sMsg))
            {
                var msg = context.MakeMessage();
                msg.Text = sMsg;
                await context.PostWithFacebookNotificationAsync(msg);
            }
        }

        protected async Task SendTypingAsync(IDialogContext context)
        {
            var msg = context.MakeMessage();
            msg.Text = string.Empty;
            msg.Type = ActivityTypes.Typing;
            await context.PostWithFacebookNotificationAsync(msg);
        }

        protected IMessageActivity CreateSuggestedActions(IDialogContext context, string sPrompt, IEnumerable<KeyValuePair<string, string>> options)
        {
            var reply = context.MakeMessage();
            reply.Text = sPrompt;
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = options.Select(o => new CardAction() { Type = ActionTypes.ImBack, Title = o.Key, Value = o.Value }).ToList()
            };

            return reply;
        }
    }

    [Serializable]
    public abstract class ScenarioBaseDialog : DialogWithHelpers<bool>
    {
        // Common stuff here (removed)

        // Extend and call this base constructor if this dialog will be called in response to a generated insight (by the scheduler).
        public ScenarioBaseDialog(long dummyId)
        {
            // ...
        }

        // ..other base ctors removed

        public abstract override Task StartAsync(IDialogContext context);
    }
}
