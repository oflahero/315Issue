using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using _315Issue.Dialogs;

// N.B. Not actually used at the moment - just for the silly stuff like who am i, good bot.
// Issues with conversation stack exceptions if we reset. Figure it out later - for the time being, leave that working functionality as is in MessagesController.

namespace _315Issue.Assets
{
    public class QuitConversationScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask task;
        private readonly IDialogStack stack;
        private readonly IBotData botData;
        private readonly IBotToUser botToUser;
        private string[] msgsOfInterest = new string[] {
            "start over", "exit", "quit", "done", "start again", "restart", "leave", "reset",
            "good bot", "help" };

        public QuitConversationScorable(IDialogTask task, IDialogStack stack, IBotData botData, IBotToUser botToUser)
        {
            SetField.NotNull(out this.task, nameof(task), task);
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
        }

        protected override Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                var msg = message.Text.ToLowerInvariant();

                if (msgsOfInterest.Any(msg.Equals))
                {
                    return Task.FromResult(msg);
                }
            }

            return Task.FromResult<string>(null);
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity activity, string state, CancellationToken token)
        {
            string sReply;

            if (state == "good bot")
            {
                sReply = "Thanks! I do appreciate it.";
            }
            else if (state == "help")
            {
                sReply = "Hi there! Use 'quit' to end a conversation you feel stuck in. See the website for more information.";
            }
            else
            {
                await activity.ResetConversationAsync(botData, stack, token);
                sReply = "OK, no problem.";
            }

            var respD = new SimpleResponseDialog(sReply);
            var interruption = respD.Void<object, IMessageActivity>();
            this.task.Call(interruption, null);
            await this.task.PollAsync(token);

            // Simpler version of sending the sReply - just use IBotToUser:
            //await botToUser.PostAsync(sReply);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}