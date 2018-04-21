using _315Issue.Dialogs;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Autofac.Base;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Builder.Azure;
using System.Configuration;

namespace _315Issue.Assets
{
    public class _315IssueModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<DialogFactory>()
                .Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();


            builder.RegisterType<FindPersonDialog>()
                .Named<ScenarioBaseDialog>("find").InstancePerLifetimeScope();


            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerLifetimeScope();

            // Gubbins for overriding the default 'Sorry, my bot code is having an issue' message.
            builder
                .RegisterKeyedType<PostUnhandledExceptionToUser, IPostToBot>()
                .InstancePerLifetimeScope();

            builder
                .RegisterAdapterChain<IPostToBot>
                (
                    typeof(EventLoopDialogTask),
                    typeof(SetAmbientThreadCulture),
                    typeof(PersistentDialogTask),
                    typeof(ExceptionTranslationDialogTask),
                    typeof(SerializeByConversation),
                    typeof(PostUnhandledExceptionToUser),
                    typeof(PostUnhandledExceptionToUser),
                    typeof(LogPostToBot)
                )
                .InstancePerLifetimeScope();


            var sBotStorageTableName = "BotData315Issue";

            var store = 
                new TableBotDataStore(ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString, sBotStorageTableName);

            builder.Register(c => store)
                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                .AsSelf()
                .SingleInstance();

            builder.Register(c => new CachingBotDataStore(store,
                CachingBotDataStoreConsistencyPolicy
                .ETagBasedConsistency))
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .InstancePerLifetimeScope();

            //Scorables:
            builder
            .Register(c => new QuitConversationScorable(c.Resolve<IDialogTask>(), c.Resolve<IDialogStack>(), c.Resolve<IBotData>(), c.Resolve<IBotToUser>()))
                            .As<IScorable<IActivity, double>>()
                            .InstancePerLifetimeScope();
        }
    }
}