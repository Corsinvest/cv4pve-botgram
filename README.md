# <img src="icon.png" alt="" height="36" align="top"> cv4pve-botgram

```
   ______                _                      __
  / ____/___  __________(_)___ _   _____  _____/ /_
 / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
/ /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
\____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Telegram Bot for Proxmox VE (Made in Italy)
```

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-botgram.svg?style=flat-square)](LICENSE.md)
[![Release](https://img.shields.io/github/release/Corsinvest/cv4pve-botgram.svg?style=flat-square)](https://github.com/Corsinvest/cv4pve-botgram/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Corsinvest/cv4pve-botgram/total.svg?style=flat-square&logo=download)](https://github.com/Corsinvest/cv4pve-botgram/releases)
[![NuGet](https://img.shields.io/nuget/v/Corsinvest.ProxmoxVE.TelegramBot.Api.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Corsinvest.ProxmoxVE.TelegramBot.Api/)
[![WinGet](https://img.shields.io/winget/v/Corsinvest.cv4pve.botgram?style=flat-square&logo=windows)](https://winstall.app/apps/Corsinvest.cv4pve.botgram)
[![AUR](https://img.shields.io/aur/version/cv4pve-botgram?style=flat-square&logo=archlinux)](https://aur.archlinux.org/packages/cv4pve-botgram)

> **Your Proxmox VE cluster in a Telegram chat**: start, stop and shut down VMs and containers, reboot nodes and call any API path from your phone, restricted to the chats you allow.
>
> **[Documentation](https://corsinvest.github.io/cv4pve-botgram/)**
>
> Prefer a web interface? cv4pve-botgram also runs inside [cv4pve-admin](https://github.com/Corsinvest/cv4pve-admin), as its [Bots](https://corsinvest.github.io/cv4pve-admin/modules/bots/) module.

---

## Why

A VM hangs at night, or a node needs a reboot while you are away from your desk. The Proxmox VE web interface is behind the company network: reaching it means a VPN and a laptop.

cv4pve-botgram puts the actions you need in those moments in a Telegram chat. The bot runs inside your network, next to the cluster, and you talk to it from the phone you already carry. It connects out to Telegram and waits for your messages: no port has to be opened towards your network.

It **runs outside the nodes and uses only the Proxmox VE API**: nothing to install on the cluster, no SSH, no root shell. What a chat can do is exactly what the bot's API token is allowed to do.

---

## What a chat looks like

```
You   /vmstart
Bot   Choose Vm
      [ qemu/100 web01 ]  [ lxc/105 dns ]
You   (tap "qemu/100 web01")
Bot   VM/CT qemu/100 on node pve01 Start!
```

```
You   /qsts
Bot   Choose node
      [ pve01 ]  [ pve02 ]
You   (tap "pve01")
Bot   Choose vmid
      [ 100 ]  [ 101 ]
You   (tap "100")
Bot   (a file with the status of VM 100)
```

`/qsts` is an alias of `get /nodes/{node}/qemu/{vmid}/status/current`: the bot asks for every placeholder. See [Commands](https://corsinvest.github.io/cv4pve-botgram/commands/).

---

## Features

- **VMs and containers**: start, shut down, stop and reset, choosing the guest from buttons grouped by node.
- **Nodes**: reboot or shut down a node, chosen from those online.
- **Any API call**: `/get`, `/set`, `/create` and `/delete` on any path of the Proxmox VE API, with the answer as a table in a file.
- **Placeholders**: leave `{node}` or `{vmid}` in a path and the bot offers the values that exist; leave one in a parameter and it asks you to type it.
- **Aliases**: short names for the calls you repeat. Many are built in for cluster, nodes, VMs and containers, and you add your own from the chat.
- **Only your chats**: the bot answers only the chat IDs you list; any other chat is refused and logged.
- **Runs as a service**: for Linux, Windows and macOS, a native systemd (`Type=notify`) and Windows service, with no wrapper.
- **Keeps running with a node down**: give it more than one host and it uses the first that answers.

---

## Quick start

```bash
# Windows
winget install Corsinvest.cv4pve.botgram

# Linux (other platforms and packages: see the documentation)
wget https://github.com/Corsinvest/cv4pve-botgram/releases/latest/download/cv4pve-botgram-linux-x64.zip
unzip cv4pve-botgram-linux-x64.zip && chmod +x cv4pve-botgram

# Run against any node of the cluster, with an API token and the token of your Telegram bot
./cv4pve-botgram --host=pve01 --api-token='bot@pve!bot=<uuid>' --token='<telegram-bot-token>' --chatsId=<chat-id>
```

Then send `/help` to your bot. The Telegram token comes from BotFather, see [Telegram bot](https://corsinvest.github.io/cv4pve-botgram/telegram-bot/). Always set `--chatsId`: without it the bot answers every chat. The API token needs the privileges listed in [Permissions](https://corsinvest.github.io/cv4pve-botgram/permissions/).

---

## Documentation

| | |
|---|---|
| [Getting started](https://corsinvest.github.io/cv4pve-botgram/getting-started/) | Install, connect, send the first command |
| [Telegram bot](https://corsinvest.github.io/cv4pve-botgram/telegram-bot/) | Creating the bot with BotFather, restricting it to your chats |
| [Permissions](https://corsinvest.github.io/cv4pve-botgram/permissions/) | The user, the API token and the privileges of each command |
| [Connection](https://corsinvest.github.io/cv4pve-botgram/connection/) | Hosts, credentials, certificates, options in a file |
| [Run as a service](https://corsinvest.github.io/cv4pve-botgram/service/) | systemd on Linux, a service on Windows, no wrapper needed |
| [Commands](https://corsinvest.github.io/cv4pve-botgram/commands/) | Guests, nodes, and how a conversation with the bot works |
| [API commands](https://corsinvest.github.io/cv4pve-botgram/commands/api/) | `/get`, `/set`, `/create`, `/delete`, placeholders, `/usage` |
| [Aliases](https://corsinvest.github.io/cv4pve-botgram/commands/aliases/) | The built-in aliases and how to add your own |
| [Troubleshooting](https://corsinvest.github.io/cv4pve-botgram/troubleshooting/) | What the answers and the log mean when something goes wrong |

---

## Related tools

For the same actions from a terminal, with saved clusters and tab completion, see [cv4pve-cli](https://github.com/Corsinvest/cv4pve-cli). The whole suite: [corsinvest.it/cv4pve](https://www.corsinvest.it/en/cv4pve/).

---

## Support

Professional support and consulting available through [Corsinvest](https://www.corsinvest.it/en/cv4pve/).

---

**By sysadmins, for sysadmins.**

Part of [cv4pve](https://www.corsinvest.it/en/cv4pve/) suite | Made with ❤️ in Italy by [Corsinvest](https://www.corsinvest.it)

Proxmox® is a registered trademark of Proxmox Server Solutions GmbH. cv4pve is developed by Corsinvest and is not a Proxmox product.

Copyright © Corsinvest Srl
