using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace _315Issue.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters.");

            var bCommandDialogLaunched = false;
            string[] possibleCommands = new string[] { "find" };
            if (length > 0)
            {
                var sCommand = activity.Text.ToLowerInvariant();
                if (possibleCommands.Any(sCommand.Equals))
                {
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                    {

                        context.Call(
                             scope.ResolveNamed<ScenarioBaseDialog>(
                                    sCommand,
                                    new NamedParameter("dummyId", -1)),
                            StartOverGenericAsync);
                        bCommandDialogLaunched = true;
                    }
                }
            }
            
            if (!bCommandDialogLaunched)
            {
                context.Wait(MessageReceivedAsync);
            }
        }

        async Task StartOverGenericAsync(IDialogContext context, IAwaitable<bool> result)
        {
            bool bResult = await result;
            // ...act on result?
            context.Wait(MessageReceivedAsync);
        }
    }
}