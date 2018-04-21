using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace _315Issue.Assets
{
    /// <summary>
    /// This IPostToBot service converts any unhandled exceptions to a message sent to the user.
    /// </summary>
    public sealed class PostUnhandledExceptionToUser : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IBotToUser botToUser;
        private readonly ResourceManager resources;
        private readonly TraceListener trace;

        public PostUnhandledExceptionToUser(IPostToBot inner, IBotToUser botToUser, ResourceManager resources, TraceListener trace)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.resources, nameof(resources), resources);
            SetField.NotNull(out this.trace, nameof(trace), trace);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            catch (Exception topEx)
            {
                try
                {
                    await this.botToUser.PostAsync("Sorry, I was a bit busy. Can you try again in a moment please?");

                    var sExceptionDetail = "** General error: " + topEx.Message + "\n" + topEx.ToString() + (topEx.InnerException == null ? string.Empty :
                        "\nInner: " + topEx.InnerException.ToString());
                    Trace.TraceError(sExceptionDetail);
                }
                catch (Exception inner)
                {
                    this.trace.WriteLine(inner);
                }

                // throw; // Conceal any further issue, leave it to the trace/logs.
            }
        }
    }

}