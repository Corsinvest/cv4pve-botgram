using Corsinvest.ProxmoxVE.Api.Extension.Helpers.Shell;
using Corsinvest.ProxmoxVE.TelegramBot.Helpers;
using Telegram.Bot.Types;

namespace Corsinvest.ProxmoxVE.TelegramBot.Commands
{
    public class Start : Help
    {
        public override bool ShowInHelp => false;
        public override string Name => "start";
        public override string Description => "Start";
        protected override string GetText(Message message)
            => $@"Corsinvest for Proxmox VE with Telegram
Welcome {message.Chat.FirstName}
You are connect to Proxmox VE {PveHelper.Host}

{base.GetText(message)}

{ShellHelper.REMEMBER_THESE_THINGS}
";
    }
}