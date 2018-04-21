using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace _315Issue.Assets
{
    public static class Helpers
    {
        public async static Task ResetConversationAsync(this IActivity activity, IBotData botData,
            IDialogStack stack, CancellationToken token)
        {
            // Old version (deprecated)
            await activity.GetStateClient().BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id, token);

            Trace.TraceInformation("Clearing stack etc.");
            await botData.LoadAsync(token);
            stack.Reset();

            botData.UserData.Clear();
            botData.ConversationData.Clear();
            botData.PrivateConversationData.Clear();

            await botData.FlushAsync(token);

            Trace.TraceInformation("Cleared stack etc.");
        }

        public static async Task PostWithFacebookNotificationAsync(this IDialogContext context, IMessageActivity message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.ChannelId=="facebook")
            {
                message.ChannelData = JObject.FromObject(new
                {
                    messaging_type = "RESPONSE",
                    notification_type = "REGULAR"
                });
            }

            await context.PostAsync(message);
        }
    }
}