
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace _315Issue.Dialogs
{
    public class SearchablePerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class FindPersonDialog : ScenarioBaseDialog
    {
        List<SearchablePerson> _searchablePeople;

        public FindPersonDialog(long dummyId)
            : base(dummyId)
        {
            // ...
        }

        public override Task StartAsync(IDialogContext context)
        {
            _searchablePeople = new List<SearchablePerson>()
            {
               
                new SearchablePerson() {Id="P2", Name="Jason S", Email="j@ms.com", Phone="+133333333"},
                // (Unrelated?) crazy issue: change 'eummy.com' to 'dummy.com' on the next line and see a 'BadRequest' when PostAsyncing on the Facebook channel. 
                new SearchablePerson() {Id="P1", Name="Owen O'Flaherty", Email="owen@dummy.com", Phone="+15552591"},
                new SearchablePerson() {Id="P3", Name="Someone Else", Email="someone@else.net", Phone="+19999919"},
            };


            PromptForContact(context);

            return Task.CompletedTask;
        }

        void PromptForContact(IDialogContext context)
        {
            PromptDialog.Text(context, SearchEntered, "Enter a few letters of the contact you're looking for.");
        }

        async Task SearchEntered(IDialogContext context, IAwaitable<string> contextSearchText)
        {
            var cSearch = await contextSearchText;

            if (_searchablePeople != null)
            {
                var applicableContacts = _searchablePeople.Where(c => c.Name.IndexOf(cSearch, StringComparison.OrdinalIgnoreCase) >= 0);
                if (applicableContacts.Count() > 10)
                {
                    await MessageAndContinueAsync(context, "I found too many contacts matching that. Can you be more specific?");
                    PromptForContact(context);
                }
                else if (applicableContacts.Count() == 0)
                {
                    await QuitWithAsync(context, "Sorry, I couldn't find any matches.");
                }
                else
                {
                    context.Call(new NumberedChoiceDialog<GenericEntityChoosable>(applicableContacts.Select(a => new GenericEntityChoosable() { EntityID = a.Id, Description = a.Name }).ToList(),
                        "Pick one!"), ContactChosen);
                }
            }
            else
            {
                await QuitWithAsync(context, "Yikes.");
            }
        }


        async Task ContactChosen(IDialogContext context, IAwaitable<GenericEntityChoosable> chosenEntity)
        {
            var ct = await chosenEntity;
            var chosenContact = _searchablePeople.Single(c => c.Id == ct.EntityID);

            // 'badRequest' happening on next postasync, so log it:
            var sMsg = $"Details are:  \nName: {chosenContact.Name}  \nEmail: {chosenContact.Email}  \nPhone: {chosenContact.Phone}";
            Trace.TraceWarning("Look out!!! BadRequest incoming?");
            Trace.TraceWarning(sMsg);
            await MessageAndContinueAsync(context, sMsg);
            context.Done(true);
            //PromptDialog.Confirm(context, EmailDetails, $"Do you want those details emailed to you?");
        }

    }
}
