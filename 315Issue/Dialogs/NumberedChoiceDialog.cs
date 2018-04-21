using _315Issue.Assets;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _315Issue.Dialogs
{
    // There's probably an easier way to go about this by overriding PromptDialog. This at least gives us most control.

    public interface ICustomDialogChoosable
    {
        string Description { get; set; }
    }

    public class BasicChoosable : ICustomDialogChoosable
    {
        public string Description { get; set; }
    }

    [Serializable]
    public class NumberedChoiceDialog<T> : IDialog<T> where T : ICustomDialogChoosable
    {
        IEnumerable<T> _choices;
        string _preamble;

        public NumberedChoiceDialog(IEnumerable<T> choices, string preamble = "Please choose from one of the following:")
        {
            _choices = choices;
            _preamble = preamble;
        }

        public async Task StartAsync(IDialogContext context)
        {
            // throw exception if choices length == 0?
            if (_choices.Count() == 0)
                throw new IndexOutOfRangeException("There must be at least one option in the list.");
            else if (_choices.Count() == 1)
            {
                var onlyOneMsg = context.MakeMessage();
                onlyOneMsg.Text = $"Only one option - '{_choices.ElementAt(0).Description}'";
                await context.PostWithFacebookNotificationAsync(onlyOneMsg);
                context.Done(_choices.ElementAt(0));
            }
            else
            {
                var opener = context.MakeMessage();
                StringBuilder bldMsg = new StringBuilder($"{_preamble}  \n  \n");

                var index = 1;
                foreach (var c in _choices)
                {
                    bldMsg.Append($"{index++} - {c.Description}  \n");
                }

                opener.Text = bldMsg.ToString();
                await context.PostWithFacebookNotificationAsync(opener);

                context.Wait(this.ChooseFromList);
            }
        }

        async Task ChooseFromList(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            string sChoice = (await result).Text?.Trim() ?? string.Empty;
            int nChoice = -1;
            Int32.TryParse(sChoice, out nChoice);
            var msg = context.MakeMessage();

            if (nChoice == 0 || nChoice > _choices.Count())
            {
                // Test the descriptions instead.
                var numMatches = _choices.Where(c => c.Description.IndexOf(sChoice, StringComparison.InvariantCultureIgnoreCase) >= 0).Count();
                if (numMatches != 1)
                {
                    msg.Text = $"Please enter a number between 1 and {_choices.Count()}.";
                    await context.PostWithFacebookNotificationAsync(msg);

                    context.Wait(this.ChooseFromList);
                }
                else
                {
                    context.Done(_choices.Single(c => c.Description.IndexOf(sChoice, StringComparison.InvariantCultureIgnoreCase) >= 0));
                }
            }
            else
            {
                context.Done(_choices.ElementAt(nChoice - 1));
            }
        }
    }

    // Alternative to PromptDialog.Confirm
    public class YesNoNumberedChoiceDialog : IDialog<bool>
    {
        string sYes = "Yes";
        string sNo = "No";

        class YesNoChoice : ICustomDialogChoosable
        {
            public string Description { get; set; }
        }

        string _preamble;

        public YesNoNumberedChoiceDialog(string preamble)
        {
            _preamble = preamble;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Call(new NumberedChoiceDialog<YesNoChoice>(
                new List<YesNoChoice>() { new YesNoChoice() { Description = sYes }, new YesNoChoice() { Description = sNo } }, _preamble),
                this.YesNoChosen);
            return Task.CompletedTask;
        }

        async Task YesNoChosen(IDialogContext context, IAwaitable<YesNoChoice> result)
        {
            context.Done((await result).Description == sYes);
        }
    }

    // These two could really be combined into a generic choosable. Never know what extra bits you might need for future integrations, though.
    public class GenericEntityChoosable : ICustomDialogChoosable
    {
        public string EntityID { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return this.Description;
        }
    }
}
